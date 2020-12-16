using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace FinalExamSchedulingWpfApp.View
{
    /// <summary>
    /// Attached Property for dynamic column count
    /// As shown here: https://stackoverflow.com/questions/320089/how-do-i-bind-a-wpf-datagrid-to-a-variable-number-of-columns
    /// </summary>
    public class DataGridColumnsBehavior
    {
        public static readonly DependencyProperty BindableColumnsProperty =
            DependencyProperty.RegisterAttached("BindableColumns",
                                                typeof(ObservableCollection<DataGridColumn>),
                                                typeof(DataGridColumnsBehavior),
                                                new UIPropertyMetadata(null, BindableColumnsPropertyChanged));
        /// <summary>
        /// Collection to store collection change handlers - to be able to unsubscribe later.
        /// </summary>
        private static readonly Dictionary<ObservableCollection<DataGridColumn>, NotifyCollectionChangedEventHandler> _handlers;
        static DataGridColumnsBehavior()
        {
            _handlers = new Dictionary<ObservableCollection<DataGridColumn>, NotifyCollectionChangedEventHandler>();
        }
        private static void BindableColumnsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            DataGrid dataGrid = source as DataGrid;
            ObservableCollection<DataGridColumn> columns = e.NewValue as ObservableCollection<DataGridColumn>;
            if (columns != null)
            {
                dataGrid.Columns.Clear();
                // Add columns from this source.
                foreach (DataGridColumn column in columns)
				{
                    var dataGridOwnerProperty = column.GetType().GetProperty("DataGridOwner", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (dataGridOwnerProperty != null)
                        dataGridOwnerProperty.SetValue(column, null);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        dataGrid.Columns.Add(column);
                    }, System.Windows.Threading.DispatcherPriority.ContextIdle);
                    
                }

                // Unsubscribe old handler
                NotifyCollectionChangedEventHandler h;
                if (_handlers.TryGetValue(columns, out h))
                {
                    columns.CollectionChanged -= h;
                    _handlers.Remove(columns);
                }
                // Subscribe new handler
                h = (_, ne) => OnCollectionChanged(ne, dataGrid);
                _handlers[columns] = h;
                columns.CollectionChanged += h;
            }
        }
        static void OnCollectionChanged(NotifyCollectionChangedEventArgs ne, DataGrid dataGrid)
		{
            switch (ne.Action)
            {
                case NotifyCollectionChangedAction.Reset:
                    dataGrid.Columns.Clear();
                    foreach (DataGridColumn column in ne.NewItems)
                        dataGrid.Columns.Add(column);
                    break;
                case NotifyCollectionChangedAction.Add:
                    foreach (DataGridColumn column in ne.NewItems)
                        dataGrid.Columns.Add(column);
                    break;
                case NotifyCollectionChangedAction.Move:
                    dataGrid.Columns.Move(ne.OldStartingIndex, ne.NewStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (DataGridColumn column in ne.OldItems)
                        dataGrid.Columns.Remove(column);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    dataGrid.Columns[ne.NewStartingIndex] = ne.NewItems[0] as DataGridColumn;
                    break;
            }
        }
        public static void SetBindableColumns(DependencyObject element, ObservableCollection<DataGridColumn> value)
        {
            element.SetValue(BindableColumnsProperty, value);
        }
        public static ObservableCollection<DataGridColumn> GetBindableColumns(DependencyObject element)
        {
            return (ObservableCollection<DataGridColumn>)element.GetValue(BindableColumnsProperty);
        }
    }
}
