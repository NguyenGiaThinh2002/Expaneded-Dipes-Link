using DipesLink.ViewModels;
using DipesLink.Views.Extension;
using SharedProgram.Shared;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace DipesLink.Views.UserControls.MainUc
{
    public partial class JobEventsLog : UserControl
    {
        private JobEventsLogHelper _jobEventsLogHelper = new();
        public JobEventsLog()
        {
            InitializeComponent();
           ViewModelSharedEvents.OnListBoxMenuSelectionChange += MainWindow_ListBoxMenuSelectionChange;
        }

        private void MainWindow_ListBoxMenuSelectionChange(object? sender, EventArgs e)
        {
            if (sender == null) return;
            var index = (int)sender;
            if (index == 3) // Is Event Log
            {
                Task.Run(() => LoadEventDataFromFile());
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
            Task.Run(()=> LoadEventDataFromFile());
        }

        private void LoadEventDataFromFile()
        {
            try
            {
                Application.Current?.Dispatcher.Invoke(new Action(() =>
                {
                    int index = ListBoxMenu.SelectedIndex;
                    _jobEventsLogHelper?.DisplayList?.Clear();
                    DataGridEventLogs.AutoGenerateColumns = false;
                    JobEventsLogHelper.CreateDataTemplate(DataGridEventLogs);
                    var logPath = CurrentViewModel<MainViewModel>()?.JobList[index].EventsLogPath;
                    var logDirectoryPath = SharedPaths.PathEventsLog + $"Job{index + 1}";
                    logPath = $"{logDirectoryPath}\\_JobEvents_{CurrentViewModel<MainViewModel>()?.JobList[index].Name}.csv";

                    if (logPath != null && File.Exists(logPath))
                    {
                        _jobEventsLogHelper?.InitEventsLogDatabase(logPath);
                    }
                    DataGridEventLogs.ItemsSource = _jobEventsLogHelper?.DisplayList;
                }));
            }
            catch (Exception)
            {

            }
          
        }
    }
}
