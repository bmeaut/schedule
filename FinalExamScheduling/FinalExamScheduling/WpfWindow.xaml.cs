using FinalExamScheduling.Model;
using OfficeOpenXml.FormulaParsing.Excel.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FinalExamScheduling
{
    /// <summary>
    /// Interaction logic for WpfWindow.xaml
    /// </summary>
    public partial class WpfWindow : Window
    {
        public WpfWindow()
        {
            InitializeComponent();
        }

        private void bt_Click(object sender, RoutedEventArgs e)
        {
            Program.RunHeuristic(cb_order.SelectedIndex);

            hideSchedule();
            hideStatistic();
            hideSearch();
        }

        private void bt_schedule_Click(object sender, RoutedEventArgs e)
        {
            hideStatistic();
            hideSearch();
            bt_schedule.IsEnabled = false;

            lb_text.Visibility = Visibility.Visible;
            lb_text.Content = "A szóbeli záróvizsga beosztása";

            dg_schedule.Visibility = Visibility.Visible;
            dg_schedule.ItemsSource = Program.LoadCollectionData();
        }

        private void bt_statistic_Click(object sender, RoutedEventArgs e)
        {
            hideSchedule();
            hideSearch();
            bt_statistic.IsEnabled = false;

            lb_text.Visibility = Visibility.Visible;
            lb_text.Content = "Az oktatók terheléseloszlása";
        }

        private void bt_search_Click(object sender, RoutedEventArgs e)
        {
            hideSchedule();
            hideStatistic();
            bt_search.IsEnabled = false;

            bt_search_instructor.Visibility = Visibility.Visible;
            bt_search_student.Visibility = Visibility.Visible;
        }

        private void bt_search_instructor_Click(object sender, RoutedEventArgs e)
        {
            hideSearchStudent();
            bt_search_instructor.IsEnabled = false;
            tb_search_input.Visibility = Visibility.Visible;
            bt_search_start.Visibility = Visibility.Visible;
        }

        private void bt_search_student_Click(object sender, RoutedEventArgs e)
        {
            hideSearchInstructors();
            bt_search_student.IsEnabled = false;

            tb_search_input.Visibility = Visibility.Visible;
            bt_search_start.Visibility = Visibility.Visible;
            
        }

        private void bt_search_start_Click(object sender, RoutedEventArgs e)
        {
            dg_schedule.Visibility = Visibility.Visible;

            //if (Program.GetFinalExamForInstructor(tb_search_input.Text) == null)
            //{
            //    dg_schedule.Visibility = Visibility.Hidden;
            //    lb_search_empty.Content = "Nem található a keresett személy";
            //}

            if (bt_search_student.IsEnabled)
                dg_schedule.ItemsSource = Program.GetFinalExamForInstructor(tb_search_input.Text);
            
            else
                dg_schedule.ItemsSource = Program.GetFinalExamForStudent(tb_search_input.Text);
        }

        void hideSchedule()
        {
            bt_schedule.IsEnabled = true;
            lb_text.Visibility = Visibility.Hidden;
            dg_schedule.Visibility = Visibility.Hidden;
        }

        void hideStatistic()
        {
            bt_statistic.IsEnabled = true;
            lb_text.Visibility = Visibility.Hidden;
        }

        void hideSearch()
        {
            bt_search.IsEnabled = true;
            bt_search_instructor.Visibility = Visibility.Hidden;
            bt_search_student.Visibility = Visibility.Hidden;
            hideSearchInstructors();
            hideSearchStudent();
        }
        
        void hideSearchInstructors()
        {
            bt_search_instructor.IsEnabled = true;
            hideSearchOptions();
        }

        void hideSearchStudent()
        {
            bt_search_student.IsEnabled = true;
            hideSearchOptions();
            
        }

        void hideSearchOptions()
        {
            tb_search_input.Clear();
            tb_search_input.Visibility = Visibility.Hidden;
            bt_search_start.Visibility = Visibility.Hidden;
            dg_schedule.Visibility = Visibility.Hidden;
            //lb_search_empty.Visibility = Visibility.Hidden;
        }

    }
}
