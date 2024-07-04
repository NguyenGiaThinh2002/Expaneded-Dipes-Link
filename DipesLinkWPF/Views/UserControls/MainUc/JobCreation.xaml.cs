using DipesLink.ViewModels;
using DipesLink.Views.SubWindows;
using log4net.Util;
using SharedProgram.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace DipesLink.Views.UserControls.MainUc
{
    /// <summary>
    /// Interaction logic for JobDetails.xaml
    /// </summary>
    public partial class JobCreation : System.Windows.Controls.UserControl
    {
        public JobCreation()
        {
            InitializeComponent();
            ViewModelSharedEvents.MainListBoxMenuChange += ViewModelSharedEvents_MainListBoxMenuChange; 
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
        private T? CurrentViewModel<T>() where T : class
        {
            if (DataContext is T viewModel)
            {
                return viewModel;
            }
            else
            {
                return null;
            }
        }
        private void ListBoxMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadJobList();
        }
        private void ViewModelSharedEvents_MainListBoxMenuChange(object? sender, EventArgs e)
        {
            LoadJobList();
        }

        private void LoadJobList()
        {
                int index = ListBoxMenu.SelectedIndex;
                CallbackCommand(vm => vm.LoadJobList(index)); // Update Job on UI
                CurrentViewModel<MainViewModel>()?.LockUI(index);
        }
        //ClearTextBoxes(myGroupBox);

        //private void ClearTextBoxes(GroupBox groupBox)
        //{

        //    // Iterate through all child controls within the GroupBox
        //    foreach (var control in FindVisualChildren<TextBox>(groupBox))
        //    {
        //        control.Text = ""; // Set the text of each TextBox to an empty string
        //    }
        //}

        //// Helper method to find all visual children of a specific type within a parent control
        //public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        //{
        //    if (depObj != null)
        //    {
        //        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        //        {
        //            DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
        //            if (child != null && child is T)
        //            {
        //                yield return (T)child;
        //            }

        //            foreach (T childOfChild in FindVisualChildren<T>(child))
        //            {
        //                yield return childOfChild;
        //            }
        //        }
        //    }
        //}

        private int CurrentIndex() => ListBoxMenu.SelectedIndex;
        private void ButtonSaveJob_Click(object sender, RoutedEventArgs e)
        {
            var vm = CurrentViewModel<MainViewModel>();
            int jobIndex = ListBoxMenu.SelectedIndex;
            
           
            CallbackCommand(vm => vm.SaveJob(jobIndex));
            CallbackCommand(vm => vm.LoadJobList(jobIndex)); // Update Job on UI
            //MainViewModel.SelectJob.JobFileList = new();
            var isSaveJob = vm.isSaveJob;


            if (vm is null) return;
            if (isSaveJob)
            {
                vm.CreateNewJob.Name = string.Empty;
                vm.CreateNewJob.StaticText = string.Empty;
                vm.CreateNewJob.DatabasePath = string.Empty;
                vm.CreateNewJob.DataCompareFormat = string.Empty;
                vm.CreateNewJob.ImageExportPath = string.Empty;
                vm.CreateNewJob.TemplateList = new List<string>();
            }
           
        }

        private void ButtonBrowseDb_Click(object sender, RoutedEventArgs e)
        {
            CallbackCommand(vm => vm.BrowseDatabasePath());
            RenewData();
        }
        private void RenewData()
        {
            var vm = CurrentViewModel<MainViewModel>();
            if (vm == null) return;
            vm.CreateNewJob.DataCompareFormat = string.Empty; // Emty TextBox POD Format
            vm.SelectedHeadersList.Clear(); // Remove POD Format custom List
        }

        private void ButtonChooseCompareFormat_Click(object sender, RoutedEventArgs e)
        {
            CompareFormatWindow compareFormatWindow = new()
            {
                DataContext = DataContext as MainViewModel
            };
            compareFormatWindow.ShowDialog();
        }

        private void ButtonAutoNamed_Click(object sender, RoutedEventArgs e)
        {
            CallbackCommand(vm=>vm.AutoNamedForJob(CurrentIndex()));
        }

        private void ButtonReloadPrinterTemplate_Click(object sender, RoutedEventArgs e)
        {
            int jobIndex = ListBoxMenu.SelectedIndex;
            CallbackCommand(vm => vm.RefreshTemplatName(jobIndex));
            
        }

       

        private void TabControlJobSettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //int jobIndex = ListBoxMenu.SelectedIndex;
            //CallbackCommand(vm => vm.LoadJobList(jobIndex));
        }

        private void ListViewJobTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)// avoid Multi event action for ListView
            {
                int jobIndex = ListBoxMenu.SelectedIndex;
                CallbackCommand(vm => vm.GetDetailJobList(jobIndex));
            }
            ListViewSelectedJob.SelectedItem = null; // Lost focus 


        }

        private void ListBoxMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ListViewJobTemplate.Items.Count > 0)
            {

                object firstItem = ListViewJobTemplate.Items[0];
                ListViewJobTemplate.SelectedItem = firstItem;
                ListViewJobTemplate.Focus();

                System.Windows.Controls.ListViewItem? listViewItem = ListViewJobTemplate.ItemContainerGenerator.ContainerFromItem(firstItem) as System.Windows.Controls.ListViewItem;
                listViewItem?.Focus();
            }
        }
        private async Task PerformLoadDbAfterDelay(MainViewModel vm)
        {
            await Task.Delay(1000); // waiting for 3s connection completed
            vm.JobList[CurrentIndex()].RaiseLoadDb(CurrentIndex());
        }
        private void Button_AddJobClick(object sender, RoutedEventArgs e)
        {
           
            var vm = CurrentViewModel<MainViewModel>();
            if (vm is null) return;
            vm.JobList[CurrentIndex()].IsDBExist = false;    // Reset flag for load db

            JobDetails.isAddbutton = true;  
            vm.AddSelectedJob(CurrentIndex());
            vm.LoadJobList(CurrentIndex()); // Update Job on UI
          //  CallbackCommand(vm => vm.RestartDeviceTransfer(jobIndex)); // Update Job on UI
            vm.UpdateJobInfo(CurrentIndex());
           
           ViewModelSharedEvents.OnChangeJobHandler(ButtonAddJob.Name, CurrentIndex()); // kích hoạt sự kiện phát hiện change job
          
            //Task.Run(async () => { await PerformLoadDbAfterDelay(vm); });
            PerformLoadDbAfterDelay(vm);
        }

        private void ButtonChooseImagePath_Click(object sender, RoutedEventArgs e)
        {
            CallbackCommand(vm => vm.SelectImageExportPath());
        }

        private void Button_SaveAllClick(object sender, RoutedEventArgs e)
        {
            CallbackCommand(vm => vm.SaveAllJob());
        }

        private void ListViewSelectedJob_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                int jobIndex = ListBoxMenu.SelectedIndex;
                CallbackCommand(vm => vm.GetDetailJobList(jobIndex,true));
            }
            ListViewJobTemplate.SelectedItem = null; // Lost focus
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            CallbackCommand(vm => vm.SearchText = ((System.Windows.Controls.TextBox)sender).Text);
        }

        private void ButtonDelJob_Click(object sender, RoutedEventArgs e)
        {
            
            int jobIndex = ListBoxMenu.SelectedIndex;
            CurrentViewModel<MainViewModel>()?.DeleteJobAction(jobIndex);
            CurrentViewModel<MainViewModel>()?.UpdateJobInfo(jobIndex);
            ViewModelSharedEvents.OnChangeJobHandler(((Button)sender).Name, jobIndex);
            CallbackCommand(vm => vm.LoadJobList(jobIndex)); // Update Job on UI
        }

        private void RadioButtonRynanSeries_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var rad = sender as RadioButton;
            switch (rad.Name)
            {
                case "RadioButtonStandalone":
                    GroupBoxJobType.IsEnabled = false;
                    RadioButtonCanRead.IsEnabled = true;
                    RadioButtonStaticText.IsEnabled = true;
                    RadioButtonAfterProduction.IsChecked = false;
                    RadioButtonOnProduction.IsChecked = false;
                    RadioButtonVerifyAndPrint.IsChecked = false;
                    CurrentViewModel<MainViewModel>().CreateNewJob.JobType = SharedProgram.DataTypes.CommonDataType.JobType.StandAlone;
                    break;
                case "RadioButtonRynanSeries":
                    GroupBoxJobType.IsEnabled = true;
                    RadioButtonCanRead.IsEnabled = false;
                    RadioButtonStaticText.IsEnabled = false;
                    RadioButtonDatabase.IsChecked = true;
                    RadioButtonAfterProduction.IsChecked = true;
                    break;
                case "RadioButtonCanRead":
                case "RadioButtonStaticText":
                    GroupBoxDatabaseType.IsEnabled = false;
                    break;
                case "RadioButtonDatabase":
                    GroupBoxDatabaseType.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private void ListViewJobTemplate_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void ListViewJobTemplate_MouseDoubleClick_1(object sender, MouseButtonEventArgs e)
        {
            Button_AddJobClick(sender, e);
        }

        private void ListViewSelectedJob_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonDelJob.IsEnabled = false;
            ButtonAddJob.IsEnabled = false;
        }

        private void ListViewJobTemplate_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonDelJob.IsEnabled = true;
            ButtonAddJob.IsEnabled = true;
        }
    }
}
