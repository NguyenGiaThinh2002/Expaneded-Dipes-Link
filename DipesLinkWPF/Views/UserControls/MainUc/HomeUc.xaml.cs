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
        #region DependencyProperty
        
        public static readonly DependencyProperty SelectedTabIndexProperty =
        DependencyProperty.Register("SelectedTabIndex", typeof(int), typeof(HomeUc), new PropertyMetadata(0));

        public int SelectedTabIndex
        {
            get => (int)GetValue(SelectedTabIndexProperty);
            set => SetValue(SelectedTabIndexProperty, value);
        }

        public static readonly DependencyProperty TabControlEnableProperty =
      DependencyProperty.Register("TabControlEnable", typeof(bool), typeof(HomeUc), new PropertyMetadata(true));

        public bool TabControlEnable
        {
            get => (bool)GetValue(TabControlEnableProperty);
            set => SetValue(TabControlEnableProperty, value);
        }

        #endregion End DependencyProperty

        public HomeUc()
        {
            InitializeComponent();
        
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


        bool toggle = false;
        private void btntest_Click(object sender, RoutedEventArgs e)
        {
            toggle = !toggle;

                CallbackCommand(vm => vm.JobViewVisibility1 = Visibility.Collapsed);
                CallbackCommand(vm => vm.JobViewVisibility = Visibility.Visible);
        }
    }
}
