using DipesLink.Views.SubWindows;
using System.Windows;
using System.Windows.Threading;
using static DipesLink.Views.Enums.ViewEnums;

namespace DipesLink.Views.Extension
{
    public class CusMsgBox
    {
        public static Task<bool> Show(string message, string caption,ButtonStyleMessageBox btnStyle, ImageStyleMessageBox imgStyle)
        {
           return Task.FromResult(Application.Current.Dispatcher.Invoke(() =>
            {
                CustomMessageBox cmb = new(message, caption, btnStyle, imgStyle);
                if (cmb.ShowDialog() == true)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }));
        }
    }
}
