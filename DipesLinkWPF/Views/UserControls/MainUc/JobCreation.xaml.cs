﻿using DipesLink.Languages;
using DipesLink.ViewModels;
using DipesLink.Views.Extension;
using DipesLink.Views.SubWindows;
using SharedProgram.Shared;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static DipesLink.Views.Enums.ViewEnums;

namespace DipesLink.Views.UserControls.MainUc
{
    public partial class JobCreation : UserControl
    {
        static int currentSelectedTemplate = 0;
        public JobCreation()
        {
            InitializeComponent();
            ViewModelSharedEvents.OnMainListBoxMenu += MainListBoxMenuChange;
            ViewModelSharedEvents.OnDataTableLoading += DataTableLoadingChange; // Event notify done load database
            ViewModelSharedEvents.OnChangeJobStatus += JobStatusChanged;
            Loaded += JobCreation_Loaded;
            LoadUIPrinter();
        }

        private void LoadUIPrinter()
        {
            if (ViewModelSharedValues.Settings.NumberOfPrinter > 1)
            {
                ButtonItemsControl.ItemsSource = Enumerable.Range(1, ViewModelSharedValues.Settings.NumberOfPrinter).Select(n => n.ToString()).ToList();
                PrinterSelection.Visibility = Visibility.Visible;
            }
            else
            {
                PrinterSelection.Visibility = Visibility.Collapsed;
            }
        }

        private void JobStatusChanged(object? sender, EventArgs e)
        {
           LockUIPreventChangeJobWhenRun();
        }

        private void DataTableLoadingChange(object? sender, EventArgs e)
        {
           LockUIPreventChangeJobWhenRun();
        }

        private void JobCreation_Loaded(object sender, RoutedEventArgs e)
        {
            RadioButtonRynanSeries.IsChecked = true;
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
            try
            {
                CurrentViewModel<MainViewModel>()?.LoadJobList(ListBoxMenuJobCreate.SelectedIndex); // Update Job on UI
                LockUIPreventChangeJobWhenRun();
                CurrentViewModel<MainViewModel>()?.GetDetailInfoJobList(ListBoxMenuJobCreate.SelectedIndex, true);  // default load info of selected Job
                CurrentViewModel<MainViewModel>()?.AutoNamedForJob(CurrentIndex());
                GetTemplateFromPrinter();
            }
            catch (Exception)
            {
            }
        }

        private void MainListBoxMenuChange(object? sender, EventArgs e)
        {
            if (sender is int indexMenu)
            {
                if (indexMenu == 1)
                {
                    LockUIPreventChangeJobWhenRun();
                    GetTemplateFromPrinter();
                }
            }
        }

        private int CurrentIndex() => ListBoxMenuJobCreate.SelectedIndex;

        private void ButtonSaveJob_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var vm = CurrentViewModel<MainViewModel>();
                if (vm is null) return;
                int jobIndex = ListBoxMenuJobCreate.SelectedIndex;
                CurrentViewModel<MainViewModel>()?.SaveJob(jobIndex);
                CurrentViewModel<MainViewModel>()?.LoadJobList(jobIndex); // Update Job on UI
                var isSaveJob = vm.isSaveJob;
                if (isSaveJob)
                {
                    vm.CreateNewJob.Name = string.Empty;
                    vm.CreateNewJob.StaticText = string.Empty;
                    vm.CreateNewJob.DatabasePath = string.Empty;
                    vm.CreateNewJob.DataCompareFormat = string.Empty;
                    vm.CreateNewJob.ImageExportPath = string.Empty;
                   // vm.CreateNewJob.TemplateList = new List<string>();
                }
            }
            catch (Exception)
            {
            }
        }

        private void ButtonBrowseDb_Click(object sender, RoutedEventArgs e)
        {
            CurrentViewModel<MainViewModel>()?.BrowseDatabasePath();
            RenewData();
        }

        private void RenewData()
        {
            try
            {
                var vm = CurrentViewModel<MainViewModel>();
                if (vm == null) return;
                vm.CreateNewJob.DataCompareFormat = string.Empty; // Emty TextBox POD Format
                vm.SelectedHeadersList.Clear(); // Remove POD Format custom List
            }
            catch (Exception)
            {
            }
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
            GetTemplateFromPrinter();
        }

        private void GetTemplateFromPrinter()
        {
            try
            {
                int jobIndex = ListBoxMenuJobCreate.SelectedIndex;
                if (CurrentViewModel<MainViewModel>() is MainViewModel vm)
                {
                    vm.CreateNewJob.TemplateList = new List<string>();
                    //vm.CreateNewJob.TemplateManager.TemplateLists[SharedValues.SelectedPrinter] = new List<string>();
                    vm.RefreshTemplatName(jobIndex);
                }
            }
            catch (Exception)
            {
            }  
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
                ListViewSelectedJob.SelectedIndex = -1; // Lost focus 
            }
        }

        private void ListBoxMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (ListViewJobTemplate.Items.Count > 0)
                {
                    object firstItem = ListViewJobTemplate.Items[0];
                    ListViewJobTemplate.SelectedItem = firstItem;
                    ListViewJobTemplate.Focus();
                    ListViewItem? listViewItem = ListViewJobTemplate.ItemContainerGenerator.ContainerFromItem(firstItem) as System.Windows.Controls.ListViewItem;
                    listViewItem?.Focus();
                }
            }
            catch (Exception)
            {
            }

        }
        private async Task PerformLoadDbAfterDelay(MainViewModel vm)
        {
            await Task.Delay(1000); // waiting for 3s connection completed
                                    //     Debug.WriteLine("JOB CREATION LOAD DB");
                                    //    vm.JobList[CurrentIndex()].RaiseLoadDb(CurrentIndex());
        }
        private void Button_AddJobClick(object sender, RoutedEventArgs e)
        {
            if (ListViewJobTemplate.SelectedItem == null) return;
            var jobIndex = CurrentIndex();
            var msgBoxAddJob = CusMsgBox.Show(
                      LanguageModel.GetLanguage("AddJobConfirmation", jobIndex),
                      LanguageModel.GetLanguage("InfoDialogCaption"),
                      ButtonStyleMessageBox.OKCancel,
                      ImageStyleMessageBox.Info);

            if (msgBoxAddJob.Result)
            {
                try
                {
                    var vm = CurrentViewModel<MainViewModel>();
                    if (vm is null) return;
                    vm.JobList[jobIndex].IsDBExist = false;    // Reset flag for load db
                    vm.AddSelectedJob(jobIndex);
                    vm.LoadJobList(jobIndex);
                    ViewModelSharedEvents.OnChangeJobHandler(ButtonAddJob.Name, jobIndex);
                }
                catch (Exception)
                {
                }
            }

        }

        private void Button_SaveAllClick(object sender, RoutedEventArgs e)
        {
            CurrentViewModel<MainViewModel>()?.SaveAllJob();
        }

        private void ListViewSelectedJob_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.AddedItems.Count > 0)
                {
                    int jobIndex = ListBoxMenuJobCreate.SelectedIndex;
                    CurrentViewModel<MainViewModel>()?.GetDetailInfoJobList(jobIndex, true);
                }
                ListViewJobTemplate.SelectedItem = null; // Lost focus
            }
            catch (Exception)
            {
            }
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            CurrentViewModel<MainViewModel>()?.UpdateSearchText(((TextBox)sender).Text);
        }

        private void ButtonDelJob_Click(object sender, RoutedEventArgs e)
        {
            if (ListViewJobTemplate.SelectedItem == null) return;
            int jobIndex = ListBoxMenuJobCreate.SelectedIndex;
            var msgBoxDelJob = CusMsgBox.Show(
                       LanguageModel.GetLanguage("DeleteJobConfirmation", jobIndex),
                       LanguageModel.GetLanguage("WarningDialogCaption"),
                       ButtonStyleMessageBox.OKCancel,
                       ImageStyleMessageBox.Warning);

            if (msgBoxDelJob.Result)
            {

                CurrentViewModel<MainViewModel>()?.DeleteJobAction(jobIndex);
                CurrentViewModel<MainViewModel>()?.UpdateJobInfo(jobIndex);
                ViewModelSharedEvents.OnChangeJobHandler(ButtonDelJob.Name, jobIndex);
                CurrentViewModel<MainViewModel>()?.LoadJobList(jobIndex); // Update Job on UI
            }

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var vm = CurrentViewModel<MainViewModel>();
                if (vm == null) return;
                if (RadioButtonStandalone.IsChecked == true)
                {
                    GroupBoxJobType.IsEnabled = false;
                    RadioButtonCanRead.IsEnabled = true;
                    RadioButtonStaticText.IsEnabled = true;
                    RadioButtonAfterProduction.IsChecked = false;
                    RadioButtonOnProduction.IsChecked = false;
                    RadioButtonVerifyAndPrint.IsChecked = false;
                    GroupBoxTemplate.IsEnabled = false;
                    vm.CreateNewJob.JobType = SharedProgram.DataTypes.CommonDataType.JobType.StandAlone;
                }
                if (RadioButtonRynanSeries.IsChecked == true)
                {
                    RadioButtonStandalone.IsChecked = false;
                    GroupBoxJobType.IsEnabled = true;
                    RadioButtonCanRead.IsEnabled = false;
                    RadioButtonStaticText.IsEnabled = false;
                    RadioButtonDatabase.IsChecked = true;
                    RadioButtonAfterProduction.IsChecked = true;
                    GroupBoxTemplate.IsEnabled = true;
                }
                if (RadioButtonCanRead.IsChecked == true || RadioButtonStaticText.IsChecked == true)
                {
                    GroupBoxDatabaseType.IsEnabled = false;
                   
                }
                if (RadioButtonDatabase.IsChecked == true)
                {
                    GroupBoxDatabaseType.IsEnabled = true;
                   // GroupBoxTemplate.IsEnabled = true;
                }
            }
            catch (Exception)
            {
            }
        }



        private void ListViewSelectedJob_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ButtonDelJob.IsEnabled = false;
            ButtonAddJob.IsEnabled = false;
        }

        private void ListViewJobTemplate_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (ListViewSelectedJob.SelectedItem != null)
                {
                    ListViewSelectedJob.SelectedItem = null;
                    Keyboard.ClearFocus();
                }
                ButtonDelJob.IsEnabled = true;
                ButtonAddJob.IsEnabled = true;
            }
            catch (Exception)
            {
            }

        }

        private void ListViewJobTemplate_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var lv = (ListView)sender;
            if (lv.SelectedItem != null)
            {
                Button_AddJobClick(sender, e);
            }
        }
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild)
                {
                    return tChild;
                }

                var result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
        private void TemplateLoadClick(object sender, RoutedEventArgs e)
        {
            foreach (var item in ButtonItemsControl.Items)
            {
                var container = ButtonItemsControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                if (container != null)
                {
                    var button = FindVisualChild<Button>(container);
                    if (button != null)
                    {
                        button.Tag = null; // Clear selection
                    }
                }
            }

            // Cast the sender to a Button
            if (sender is Button clickedButton && int.TryParse(clickedButton.Content.ToString(), out int templateNumber))
            {
                if (clickedButton.Tag == null || clickedButton.Tag.ToString() != "Selected")
                {
                    clickedButton.Tag = "Selected";  // Set as selected
                }
                else
                {
                    clickedButton.Tag = null;  // Deselect
                }

                HandleTemplateSelection(templateNumber);
            }
            else
            {
                MessageBox.Show("Unknown template selected.");
            }
        }

        private void HandleTemplateSelection(int templateNumber)
        {

            SharedValues.SelectedTemplate = templateNumber - 1;
            if (CurrentViewModel<MainViewModel>() is MainViewModel vm)
            {
                if (vm?.CreateNewJob?.TemplateManager?.TemplateLists != null)
                {
                    foreach (var sublist in vm.CreateNewJob.TemplateManager.TemplateLists)
                    {
                        sublist?.Clear(); // Clears the elements of each sublist, keeping the structure intact
                    }
                }
                if (vm?.CreateNewJob?.TemplateManager?.TemplateListFirstFound != null)
                {
                    foreach (var sublist in vm.CreateNewJob.TemplateManager.TemplateListFirstFound)
                    {
                        sublist?.Clear(); // Clears the elements of each sublist, keeping the structure intact
                    }
                }

                GetTemplateFromPrinter();
                vm.CreateNewJob.PrinterTemplate = vm.CreateNewJob.TemplateManager.PrinterTemplateList[SharedValues.SelectedTemplate];
            }
        }

        private void ListViewTemplate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // thinh noww
            if (CurrentViewModel<MainViewModel>() is MainViewModel vm && ListViewTemplate.SelectedItem != null)
            {
                vm.CreateNewJob.TemplateManager.PrinterTemplateList[SharedValues.SelectedTemplate] = ListViewTemplate.SelectedItem.ToString();
            }
        }
    }
}
