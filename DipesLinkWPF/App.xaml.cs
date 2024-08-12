using DipesLink.Languages;
using DipesLink.Views;
using DipesLink.Views.Extension;
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
        private SplashScreenLoading? splashScreen;
        private static Mutex? mutex;
       
        #endregion

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!InitializeMutex()) // Check Application is running 
            {
                NotifyAndShutdown();
                return;
            }

            splashScreen = new(); //Show Loading Screen
            splashScreen.Show();
            KillProcessByName(new ProcessType[] { ProcessType.DeviceTransfer }); // Kill old process
            await Task.Delay(DipesLink.Properties.Settings.Default.TimeCheckOldProcess);
            splashScreen.Hide();

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
                    splashScreen.Close();
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
       
        private static bool InitializeMutex()
        {
            try
            {
                string? appName = GetApplicationName();
                bool createdNew;
                mutex = new Mutex(true, appName, out createdNew);
                return createdNew;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static string? GetApplicationName() => Assembly.GetEntryAssembly()?.GetName().Name;
        private static void NotifyAndShutdown()
        {
            try
            {
                string msg = LanguageModel.GetLanguage("AppAlreadyOpen");
                string caption = LanguageModel.GetLanguage("WarningDialogCaption");
                CusMsgBox.Show(msg, caption, Views.Enums.ViewEnums.ButtonStyleMessageBox.OK, Views.Enums.ViewEnums.ImageStyleMessageBox.Warning);
                Current.Shutdown();
            }
            catch (Exception)
            {
            }
        }
        private static void ReleaseMutex()
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
