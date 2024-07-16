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
    public partial class JobCreation : System.Windows.Controls.UserControl
    {
        public JobCreation()
        {
            InitializeComponent();
            ViewModelSharedEvents.OnMainListBoxMenu += MainListBoxMenuChange; 
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
        private void ListBoxMenuJobCreate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentViewModel<MainViewModel>()?.LoadJobList(ListBoxMenuJobCreate.SelectedIndex); // Update Job on UI
            LockUIPreventChangeJobWhenRun();
            CurrentViewModel<MainViewModel>()?.GetDetailInfoJobList(ListBoxMenuJobCreate.SelectedIndex, true);  // default load info of selected Job
            CurrentViewModel<MainViewModel>()?.AutoNamedForJob(CurrentIndex());
        }
        private void MainListBoxMenuChange(object? sender, EventArgs e)
        {
            if(sender is int indexMenu)
            {
                if(indexMenu == 1)
                {
                    LockUIPreventChangeJobWhenRun();
                }
            }
            
        }

        private int CurrentIndex() => ListBoxMenuJobCreate.SelectedIndex;
        private void ButtonSaveJob_Click(object sender, RoutedEventArgs e)
        {
            var vm = CurrentViewModel<MainViewModel>();
            int jobIndex = ListBoxMenuJobCreate.SelectedIndex;
            
           
            CurrentViewModel<MainViewModel>()?.SaveJob(jobIndex);
            CurrentViewModel<MainViewModel>()?.LoadJobList(jobIndex); // Update Job on UI
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
            CurrentViewModel<MainViewModel>()?.BrowseDatabasePath();
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
            CurrentViewModel<MainViewModel>()?.AutoNamedForJob(CurrentIndex());
        }

        private void ButtonReloadPrinterTemplate_Click(object sender, RoutedEventArgs e)
        {
            int jobIndex = ListBoxMenuJobCreate.SelectedIndex;
            CurrentViewModel<MainViewModel>()?.RefreshTemplatName(jobIndex);
            
        }

        private void TabControlJobSettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //int jobIndex = ListBoxMenu.SelectedIndex;
            //CallbackCommand(vm => vm.LoadJobList(jobIndex));
        }

        private void LockUIPreventChangeJobWhenRun()
        {
         
      
            CurrentViewModel<MainViewModel>()?.LockUI(ListBoxMenuJobCreate.SelectedIndex);
        }

        private void ListViewJobTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
       
            if (e.AddedItems.Count > 0)// avoid Multi event action for ListView
            {
                int jobIndex = ListBoxMenuJobCreate.SelectedIndex;
                CurrentViewModel<MainViewModel>()?.GetDetailInfoJobList(jobIndex);
                ListViewSelectedJob.SelectedIndex = jobIndex; // Lost focus 
            }
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

            //JobDetails.isAddbutton = true;  
            vm.AddSelectedJob(CurrentIndex());
            vm.LoadJobList(CurrentIndex()); // Update Job on UI
          //  CallbackCommand(vm => vm.RestartDeviceTransfer(jobIndex)); // Update Job on UI
            vm.UpdateJobInfo(CurrentIndex());
           
           ViewModelSharedEvents.OnChangeJobHandler(ButtonAddJob.Name, CurrentIndex()); // trigger change job detection event
          
            //Task.Run(async () => { await PerformLoadDbAfterDelay(vm); });
            PerformLoadDbAfterDelay(vm);
        }

        private void ButtonChooseImagePath_Click(object sender, RoutedEventArgs e)
        {
            CurrentViewModel<MainViewModel>()?.SelectImageExportPath();
        }

        private void Button_SaveAllClick(object sender, RoutedEventArgs e)
        {
            CurrentViewModel<MainViewModel>()?.SaveAllJob();
        }

        private void ListViewSelectedJob_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                int jobIndex = ListBoxMenuJobCreate.SelectedIndex;
                   CurrentViewModel<MainViewModel>()?.GetDetailInfoJobList(jobIndex, true);
            }
            ListViewJobTemplate.SelectedItem = null; // Lost focus
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            CurrentViewModel<MainViewModel>()?.UpdateSearchText(((TextBox)sender).Text);
        }

        private void ButtonDelJob_Click(object sender, RoutedEventArgs e)
        {
            
            int jobIndex = ListBoxMenuJobCreate.SelectedIndex;
            CurrentViewModel<MainViewModel>()?.DeleteJobAction(jobIndex);
            CurrentViewModel<MainViewModel>()?.UpdateJobInfo(jobIndex);
            ViewModelSharedEvents.OnChangeJobHandler(ButtonDelJob.Name, jobIndex);
            CurrentViewModel<MainViewModel>()?.LoadJobList(jobIndex); // Update Job on UI
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var rad = sender as RadioButton;
            var vm = CurrentViewModel<MainViewModel>();
            if (rad == null || vm == null) return;
            switch (rad.Name)
            {
                case "RadioButtonStandalone":
                    GroupBoxJobType.IsEnabled = false;
                    RadioButtonCanRead.IsEnabled = true;
                    RadioButtonStaticText.IsEnabled = true;
                    RadioButtonAfterProduction.IsChecked = false;
                    RadioButtonOnProduction.IsChecked = false;
                    RadioButtonVerifyAndPrint.IsChecked = false;
                    vm.CreateNewJob.JobType = SharedProgram.DataTypes.CommonDataType.JobType.StandAlone;
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
                    GroupBoxTemplate.IsEnabled = false;
                    break;
                case "RadioButtonDatabase":
                    GroupBoxDatabaseType.IsEnabled = true;
                    GroupBoxTemplate.IsEnabled = true;
                    break;
                default:
                    break;
            }
        }

        private void ListViewJobTemplate_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Button_AddJobClick(sender, e);
        }

        private void ListViewSelectedJob_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonDelJob.IsEnabled = false;
            ButtonAddJob.IsEnabled = false;
        }

        private void ListViewJobTemplate_MouseEnter(object sender, MouseEventArgs e)
        {
            ListViewSelectedJob.SelectedItem = null;
            ButtonDelJob.IsEnabled = true;
            ButtonAddJob.IsEnabled = true;

        }

     
    }
}
