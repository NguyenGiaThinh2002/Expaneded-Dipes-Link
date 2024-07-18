using CommunityToolkit.Mvvm.ComponentModel;
using DipesLink.ViewModels;
using SharedProgram.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using SharedProgram.Models;
using SharedProgram.Shared;
using MahApps.Metro.Controls;


namespace DipesLink.Languages
{
    //     // ViewModelBase
    public class LanguageModel : ObservableObject
    {
        public const string ApplicationDefaultLanguage = "en-US";
        //ViewModelSharedValues


        public static ResourceDictionary? _langResource = LoadLanguageResourceDictionary(ApplicationDefaultLanguage) ??
                                               LoadLanguageResourceDictionary();
        public static ResourceDictionary? Language
        {
            get { return _langResource; }
            set
            {

                if (_langResource != value)
                {
                    _langResource = value;
                    //OnPropertyChanged(nameof(Language));
                    //OnPropertyChanged();
                }

            }
        }
        //public static string? GetResource(string key)
        //{
        //    return Language?[key]?.ToString();
        //}
        public static string GetFormattedString(string resourceKey, params object[] args)
        {
            var resourceString = Application.Current.Resources[resourceKey] as string;
            return string.Format(resourceString, args);
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
            string newLanguage = ViewModelSharedValues.Settings.Language;
            if (string.IsNullOrEmpty(newLanguage))
            {
                newLanguage = ApplicationDefaultLanguage;
            }

            if (!string.IsNullOrEmpty(choosenLanguage))
            {
                newLanguage = choosenLanguage;
            }
            //SettingsModel.Language = newLanguage;
            ViewModelSharedValues.Settings.Language = newLanguage;
            ViewModelSharedFunctions.SaveSetting();
            Language = LoadLanguageResourceDictionary(newLanguage) ??
                                               LoadLanguageResourceDictionary();
            // If you have used other languages, clear it first.
            // Since the dictionary are cleared, the output of debugging will warn "Resource not found",
            // but it is not a problem in most case.
            System.Diagnostics.Debug.WriteLine("Clearing MergedDictionaries");
            Application.Current.Resources.MergedDictionaries.Clear();
            System.Diagnostics.Debug.WriteLine("Cleared");
            var a = Language?["Setting_Apply"];
            // Add new language.
            OnPropertyChanged(nameof(Language));
            Application.Current.Resources.MergedDictionaries.Add(Language);

        }
    }
}
