using DipesLink.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace DipesLink.Views.UserControls.MainUc
{
    /// <summary>
    /// Interaction logic for HomeUc.xaml
    /// </summary>
    public partial class HomeUc : UserControl
    {
        
        public HomeUc()
        {
            InitializeComponent();
            Loaded += HomeUc_Loaded;     
        }

        private void HomeUc_Loaded(object sender, RoutedEventArgs e)
        {
            InitTabControlUI(2);
        }

        public void CallbackCommand(Action<MainViewModel> execute)
        {
            try
            {
                if (DataContext is MainViewModel model)
                {
                    execute?.Invoke(model);
                }
                else
                {
                    return;
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private void InitTabControlUI(int numberOfItem = 1)
        {
            //for (int i = 0; i < numberOfItem; i++)
            //{
            //    TabItem tabItem = new()
            //    {
            //        Header = $"Station {i + 1}"
            //    };
            //    StationDetailUc stationDetailUc = new();
               
            //    tabItem.Content = stationDetailUc;
            //    TabControl_Station.Items.Add(tabItem);
            //}
        }

        bool toggle = false;
        private void btntest_Click(object sender, RoutedEventArgs e)
        {
            toggle = !toggle;

                CallbackCommand(vm => vm.JobViewVisibility1 = Visibility.Collapsed);
                CallbackCommand(vm => vm.JobViewVisibility = Visibility.Visible);
        }
    }
}
