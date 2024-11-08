using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DipesLink.Languages
{
    public interface ILanguage
    {
         ResourceDictionary? LoadLanguageResourceDictionary(string? language = null);
         string GetLanguage(string resourceKey, int? stationIndex = null);
    }
}
