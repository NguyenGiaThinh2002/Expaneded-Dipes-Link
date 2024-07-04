using DipesLink.Models;
using DipesLink.ViewModels;
using DipesLink.Views.Extension;
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
    /// Interaction logic for OverviewJobUc.xaml
    /// </summary>
    public partial class OverviewJobUc : UserControl
    {
        public JobOverview ViewModel
        {
            get { return (JobOverview)DataContext; }
            set { DataContext = value; }
        }
        public OverviewJobUc()
        {
            InitializeComponent();
            this.Loaded += OverviewJobUc_Loaded;
        }

        private void OverviewJobUc_Loaded(object sender, RoutedEventArgs e)
        {
           

        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                int index = Convert.ToInt32(button.Tag); // Retrieve the index from the Tag property
             //   ExportResult.ExportNewResult(index);
                // Add further handling logic here
                ViewModel.OnExportButtonCommandHandler(index);
            }
        }
    }
}
