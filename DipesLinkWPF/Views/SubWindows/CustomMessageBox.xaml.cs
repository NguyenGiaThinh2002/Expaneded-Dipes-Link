using FontAwesome.Sharp;
using System.Windows;
using System.Windows.Media;
using static DipesLink.Views.Enums.ViewEnums;

namespace DipesLink.Views.SubWindows
{
    public partial class CustomMessageBox : Window
    {
        public string Message { get; set; }
        public string Caption { get; set; }
        public static ImageStyleMessageBox ImageStyle { get; set; }
        public static ButtonStyleMessageBox ButtonStyle { get; set; }
        public IconChar IconMessageBox { get; set; } = IconChar.None;
        public SolidColorBrush? HeaderColor { get; set; }
        public Visibility Button1Vis { get; set; }
        public Visibility Button2Vis { get; set; }
        public string? Button1Content { get; set; }
        public string? Button2Content { get; set; }
        public CustomMessageBox(string message, string caption, ButtonStyleMessageBox buttonStyle, ImageStyleMessageBox imageStyle)
        {
            InitializeComponent();
            StyleSelection(buttonStyle, imageStyle);
            Message = message;
            Caption = caption;
            DataContext = this;
        }
     
        private void StyleSelection(ButtonStyleMessageBox buttonStyle, ImageStyleMessageBox imageStyle)
        {
            switch (buttonStyle)
            {
                case ButtonStyleMessageBox.OK:
                    Button1Vis = Visibility.Visible;
                    Button2Vis = Visibility.Collapsed;
                    Button1Content = "Ok";
                    break;
                case ButtonStyleMessageBox.YesNo:
                    Button1Vis = Visibility.Visible;
                    Button2Vis = Visibility.Visible;
                    Button1Content = "Yes";
                    Button2Content = "No";
                    break;
                case ButtonStyleMessageBox.OKCancel:
                    Button1Vis = Visibility.Visible;
                    Button2Vis = Visibility.Visible;
                    Button1Content = "OK";
                    Button2Content = "Cancel";
                    break;
            }

            switch (imageStyle)
            {
                case ImageStyleMessageBox.Info:
                    IconMessageBox = IconChar.InfoCircle;
                    HeaderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26BEA5")); 
                    break;
                case ImageStyleMessageBox.Warning:
                    IconMessageBox = IconChar.Warning;
                    HeaderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8B00"));
                    break;
                case ImageStyleMessageBox.Error:
                    IconMessageBox = IconChar.ExclamationCircle;
                    HeaderColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E04F60"));
                    break;
                default:
                    break;
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
