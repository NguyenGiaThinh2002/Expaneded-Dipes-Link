using DipesLink.Views;
using DipesLink.Views.SubWindows;
using SharedProgram.Shared;
using System.Diagnostics;
using System.Reflection;
using System.Windows;

namespace DipesLink
{
    public partial class App : Application
    {
        #region Init
        enum ProcessType
        {
            All,
            MainApp,
            DeviceTransfer
        }
        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            if (!InitializeMutex())
            {
                NotifyAndShutdown();
                return;
            }

            KillProcessByName(new ProcessType[] { ProcessType.DeviceTransfer });
            SQLitePCL.Batteries_V2.Init();
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            var loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
            if (loginWindow.IsLoggedIn)
            {
                try
                {
                    var mainWindow = new MainWindow();
                    MainWindow = mainWindow;
                    MainWindow.Show();
                    loginWindow.Close();
                }
                catch (Exception) { }
            }
            else
            {
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ReleaseMutex();
            KillProcessByName(new ProcessType[] { ProcessType.All });
        }

        private static void KillProcessByName(ProcessType[] processType)
        {
            try
            {
                if (processType != null && processType.Length > 0)
                {
                    if (processType.Any(x => x == ProcessType.DeviceTransfer || x == ProcessType.All))
                    {
                        foreach (Process process in Process.GetProcessesByName($"{SharedValues.DeviceTransferName}"))
                        {
                            process.Kill();
                        }
                    }
                    if (processType.Any(x => x == ProcessType.MainApp || x == ProcessType.All))
                    {
                        foreach (Process process in Process.GetProcessesByName($"{SharedValues.DeviceTransferName}"))
                        {
                            process.Kill();
                        }
                    }
                }
            }
            catch (Exception) { }
        }


        #region Prevent Start App One More Times
        private static Mutex? mutex;
        private bool InitializeMutex()
        {
            string? appName = GetApplicationName();
            bool createdNew;
            mutex = new Mutex(true, appName, out createdNew);
            return createdNew;
        }
        private string? GetApplicationName()
        {
            return Assembly.GetEntryAssembly()?.GetName().Name;
        }
        private void NotifyAndShutdown()
        {
            MessageBox.Show("The application is already running.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            Application.Current.Shutdown();
        }
        private void ReleaseMutex()
        {
            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex = null;
            }
        }
        #endregion Prevent Start App One More Times
    }
}
