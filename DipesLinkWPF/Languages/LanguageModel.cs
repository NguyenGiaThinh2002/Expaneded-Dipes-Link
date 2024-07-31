using CommunityToolkit.Mvvm.ComponentModel;
using DipesLink.ViewModels;
using System.IO;
using System.Windows;


namespace DipesLink.Languages
{
    public class LanguageModel : ObservableObject
    {
        public const string ApplicationDefaultLanguage = "en-US";

        public static ResourceDictionary? LangResource { get; set; }

        public static string GetFormattedString(string resourceKey, params object[] args)
        {
            var resourceString = Application.Current.Resources[resourceKey] as string;
            if (resourceString != null)
            {
                return string.Format(resourceString, args);
            }
            else
            {
                return string.Empty;
            }
        }

        public static string GetLanguage(string newString, int? stationIndex = null)
        {
            if (stationIndex.HasValue)
            {
                return GetFormattedString(newString, stationIndex.Value + 1);
            }
            else
            {
                return GetFormattedString(newString);
            }
        }

        public static ResourceDictionary? LoadLanguageResourceDictionary(String? lang = null)
        {
            try
            {
                lang = String.IsNullOrWhiteSpace(lang) ? ApplicationDefaultLanguage : lang;
                var langUri = new Uri($@"\Languages\Language\{lang}.xaml", UriKind.Relative);
                return Application.LoadComponent(langUri) as ResourceDictionary;
            }
            catch (IOException)
            {
                return null;
            }
        }


        public void UpdateApplicationLanguage(string choosenLanguage)
        {
            try
            {
                string newLanguage = ViewModelSharedValues.Settings.Language;
                if (string.IsNullOrEmpty(newLanguage))
                {
                    newLanguage = ApplicationDefaultLanguage;
                }

                if (!string.IsNullOrEmpty(choosenLanguage))
                {
                    newLanguage = choosenLanguage;
                }
                ViewModelSharedValues.Settings.Language = newLanguage;
                ViewModelSharedFunctions.SaveSetting();
                LangResource = LoadLanguageResourceDictionary(newLanguage) ?? LoadLanguageResourceDictionary();
                Application.Current.Resources.MergedDictionaries.Clear();
                OnPropertyChanged(nameof(LangResource));
                Application.Current.Resources.MergedDictionaries.Add(LangResource);
            }
            catch (Exception)
            {
            }
        }
    }
}
