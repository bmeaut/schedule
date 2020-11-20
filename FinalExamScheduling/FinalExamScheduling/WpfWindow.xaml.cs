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
using System.Windows.Controls.DataVisualization.Charting;
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

            cb_instructor_roles.Visibility = Visibility.Visible;
            bt_statistic_start.Visibility = Visibility.Visible;
            lb_text.Visibility = Visibility.Visible;
            gs_statistic.Visibility = Visibility.Visible;
            lb_text.Content = "Az oktatók terheléseloszlása";
        }

        private void bt_statistic_start_Click(object sender, RoutedEventArgs e)
        {
            mcChart.Visibility = Visibility.Visible;

            switch (cb_instructor_roles.SelectedIndex)
            {
                case 0:
                    ((ColumnSeries)mcChart.Series[0]).ItemsSource = Program.LoadColumnChartPresidents(Program.context);
                    break;
                case 1:
                    ((ColumnSeries)mcChart.Series[0]).ItemsSource = Program.LoadColumnChartSecretaries(Program.context);
                    break;
                case 2:
                    ((ColumnSeries)mcChart.Series[0]).ItemsSource = Program.LoadColumnChartMembers(Program.context);
                    break;
                case 3:
                    ((ColumnSeries)mcChart.Series[0]).ItemsSource = Program.LoadColumnChartExaminers(Program.context, "Adatvezérelt alkalmazások fejlesztése");
                    break;
                case 4:
                    ((ColumnSeries)mcChart.Series[0]).ItemsSource = Program.LoadColumnChartExaminers(Program.context, "Adatvezérelt rendszerek");
                    break;
                case 5:
                    ((ColumnSeries)mcChart.Series[0]).ItemsSource = Program.LoadColumnChartExaminers(Program.context, "Alkalmazásfejlesztési környezetek");
                    break;
            }
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
            lb_search_text.Visibility = Visibility.Visible;
            gs_search.Visibility = Visibility.Visible;
            lb_search_text.Content = "Adja meg az oktató nevét!";
        }

        private void bt_search_student_Click(object sender, RoutedEventArgs e)
        {
            hideSearchInstructors();
            bt_search_student.IsEnabled = false;

            tb_search_input.Visibility = Visibility.Visible;
            bt_search_start.Visibility = Visibility.Visible;
            lb_search_text.Visibility = Visibility.Visible;
            gs_search.Visibility = Visibility.Visible;
            lb_search_text.Content = "Adja meg a hallgató nevét vagy Neptun-kódját!";

        }

        private void bt_search_start_Click(object sender, RoutedEventArgs e)
        {
            //instructor search
            if (bt_search_student.IsEnabled)
            {
                if(Program.GetFinalExamForInstructor(tb_search_input.Text) == null)
                {
                    dg_schedule.Visibility = Visibility.Hidden;
                    lb_search_empty.Visibility = Visibility.Visible;
                    lb_search_empty.Content = "A keresett oktató nem található";
                }
                else
                {
                    lb_search_empty.Visibility = Visibility.Hidden;
                    dg_schedule.Visibility = Visibility.Visible;
                    dg_schedule.ItemsSource = Program.GetFinalExamForInstructor(tb_search_input.Text);
                }
            }
            //student search
            else
            {
                if (Program.GetFinalExamForStudent(tb_search_input.Text) == null)
                {
                    dg_schedule.Visibility = Visibility.Hidden;
                    lb_search_empty.Visibility = Visibility.Visible;
                    lb_search_empty.Content = "A keresett hallgató nem található";
                }
                else
                {
                    lb_search_empty.Visibility = Visibility.Hidden;
                    dg_schedule.Visibility = Visibility.Visible;
                    dg_schedule.ItemsSource = Program.GetFinalExamForStudent(tb_search_input.Text);
                }
            }
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
            cb_instructor_roles.Visibility = Visibility.Hidden;
            bt_statistic_start.Visibility = Visibility.Hidden;
            mcChart.Visibility = Visibility.Hidden;
            gs_statistic.Visibility = Visibility.Hidden;
        }

        void hideSearch()
        {
            bt_search.IsEnabled = true;
            bt_search_instructor.Visibility = Visibility.Hidden;
            bt_search_student.Visibility = Visibility.Hidden;
            lb_search_text.Visibility = Visibility.Hidden;
            gs_search.Visibility = Visibility.Hidden;
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
            lb_search_empty.Visibility = Visibility.Hidden;
        }

    }
}
