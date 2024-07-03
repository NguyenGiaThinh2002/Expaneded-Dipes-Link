using System.IO;
using System.Windows;
using ViewModelBase = DipesLink.ViewModels.ViewModelBase;


namespace DipesLink.Languages
{
    public class LanguageModel : ViewModelBase
    {
        public const string ApplicationDefaultLanguage = "en-US";
        
        //private String? _selectedLanguage;

        //public string SelectedLanguage
        //{
        //    get { return _selectedLanguage; }
        //    set { 
        //        _selectedLanguage = value;
        //        OnPropertyChanged();
        //    }
        //}
        public static ResourceDictionary? _langResource = LoadLanguageResourceDictionary(ApplicationDefaultLanguage) ??
                                               LoadLanguageResourceDictionary();
        public static ResourceDictionary? Language
        {
            get { return _langResource; }
            set
            {
                _langResource = value;
               // OnPropertyChanged();
            }
        }
        //public static string? GetResource(string key)
        //{
        //    return Language?[key]?.ToString();
        //}

        private static ResourceDictionary? LoadLanguageResourceDictionary(String? lang = null)
        {
            try
            {
                // if is null or blank string, set lang as default.
                lang = String.IsNullOrWhiteSpace(lang) ? ApplicationDefaultLanguage : lang;
                var langUri = new Uri($@"\Languages\Language\{lang}.xaml", UriKind.Relative);
                return Application.LoadComponent(langUri) as ResourceDictionary;
            }
            // The file does not exist.
            catch (IOException)
            {
                return null;
            }
        }
        


        public void UpdateApplicationLanguage(string choosenLanguage)
        {
            Language = LoadLanguageResourceDictionary(choosenLanguage) ??
                                               LoadLanguageResourceDictionary();
            // If you have used other languages, clear it first.
            // Since the dictionary are cleared, the output of debugging will warn "Resource not found",
            // but it is not a problem in most case.
            System.Diagnostics.Debug.WriteLine("Clearing MergedDictionaries");
            Application.Current.Resources.MergedDictionaries.Clear();
            System.Diagnostics.Debug.WriteLine("Cleared");
            var a = Language?["Setting_Apply"];
            // Add new language.
            Application.Current.Resources.MergedDictionaries.Add(Language);
        }
    }
}
