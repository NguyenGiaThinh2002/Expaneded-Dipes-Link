using DipesLink.Views;
using DipesLink.Views.SubWindows;
using SharedProgram.Shared;
using System.Diagnostics;
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
                catch (Exception){}
            }
            else
            {
                Shutdown();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
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
            catch (Exception) {}
        }

    }
}
