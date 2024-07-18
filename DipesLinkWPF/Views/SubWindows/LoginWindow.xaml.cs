using DipesLink.Extensions;
using DipesLink.Languages;
using DipesLink.ViewModels;
using DipesLink.Views.Extension;
using RelationalDatabaseHelper.SQLite;
using SharedProgram.Shared;
using SQLite;
using System.IO;
using System.Windows;
using System.Windows.Input;


namespace DipesLink.Views.SubWindows
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public bool IsLoggedIn { get; set; }
        public string? Username { get; private set; }
        public string? Password { get; private set; }
        public bool IsRememberMe { get; set; }
        public LoginWindow()
        {
            InitializeComponent();
            CreateDefaultUser.Init();
            GetRememberLoginInfo();
            setLanguage();
        }

        private void setLanguage()
        {
            ViewModelSharedFunctions.LoadSetting();
            var lang = ViewModelSharedValues.Settings;
            var currentLang = LanguageModel.LoadLanguageResourceDictionary(lang.Language);

            AppDescriptionText.Text = currentLang["Multi_Line_Barcode_Verification_System"] as string;
            UserLoginText.Text = currentLang["User_Login"] as string;
            CheckBoxRememberme.Content = currentLang["Remember_Me"] as string;
            ButtonLogin.Content = currentLang["LOGIN"] as string;
        }

        private void GetRememberLoginInfo()
        {
            var info = SecureStorage.ReadCredentials();
            if (info.Username != null && info.Password != null)
            {
                TextBoxUsername.Text = info.Username;
                TextBoxPassword.Password = info.Password;
                if (info.Username != "" && info.Password != "")
                    CheckBoxRememberme.IsChecked = true;
            }
        }
        private void GetInputsValues()
        {
            Username = TextBoxUsername.Text;
            Password = TextBoxPassword.Password;
            if (CheckBoxRememberme.IsChecked != null) IsRememberMe = (bool)CheckBoxRememberme.IsChecked;
        }
        private void Login_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoginAction();
            }
        }
        private async void LoginAction()
        {
            GetInputsValues();
            if (Username == null || Password == null) { return; }
            var databasePath = Path.Combine(SharedPaths.PathAccountsDb, "AccountDB.db");
            var options = new SQLiteConnectionString(databasePath, true, key: "123456");
            var db = new SQLiteAsyncConnection(options);

            // Retrieve all records from the User table asynchronously
            var users = await db.Table<User>().ToListAsync();
            try
            {
                var user = users.FirstOrDefault(u => u.username == Username);
                if (user != null)
                {
                    if (user.username != null) Application.Current.Properties["Username"] = user.username.ToString();
                    if (user.role != null) Application.Current.Properties["UserRole"] = AuthorizationHelper.GetRole(user.role); // save role

                    if (user.username != null && user.password! != null)
                    {
                        if (Username == user.username.ToString() && Password == user.password.ToString())
                        {
                            IsLoggedIn = true;
                            Hide();
                            if (IsRememberMe == true)
                            {
                                SecureStorage.SaveCredentials(Username, Password); // save file username and password
                            }
                            else
                            {
                                SecureStorage.SaveCredentials("", "");
                            }
                        }
                        else
                        {
                            CusMsgBox.Show(LanguageModel.GetLanguage("IncorrectUsernameOrPassword"), "Login", Enums.ViewEnums.ButtonStyleMessageBox.OK, Enums.ViewEnums.ImageStyleMessageBox.Error);
                        }
                    }
                }
                else
                {
                    CusMsgBox.Show(LanguageModel.GetLanguage("IncorrectUsernameOrPassword"), "Login", Enums.ViewEnums.ButtonStyleMessageBox.OK, Enums.ViewEnums.ImageStyleMessageBox.Error);
                }
            }
            catch (Exception)
            {
                CusMsgBox.Show(LanguageModel.GetLanguage("IncorrectUsernameOrPassword"), "Login", Enums.ViewEnums.ButtonStyleMessageBox.OK, Enums.ViewEnums.ImageStyleMessageBox.Error);
            }
        }


        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void IconImage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginAction();
        }
    }
}
