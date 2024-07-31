using DipesLink.Views.SubWindows;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DipesLink.Views.UserControls.CustomControl
{
    /// <summary>
    /// Interaction logic for IpAddressControl.xaml
    /// </summary>
    public partial class IpAddressControl : UserControl
    {

        public event TextChangedEventHandler TextChanged;

        public IpAddressControl()
        {
           
            InitializeComponent();
            TextBoxPart1.PreviewTextInput += IpPart_PreviewTextInput;
            TextBoxPart1.PreviewKeyDown += IpPart_PreviewKeyDown;
            TextBoxPart2.PreviewTextInput += IpPart_PreviewTextInput;
            TextBoxPart2.PreviewKeyDown += IpPart_PreviewKeyDown;
            TextBoxPart3.PreviewTextInput += IpPart_PreviewTextInput;
            TextBoxPart3.PreviewKeyDown += IpPart_PreviewKeyDown;
            TextBoxPart4.PreviewTextInput += IpPart_PreviewTextInput;
            TextBoxPart4.PreviewKeyDown += IpPart_PreviewKeyDown;
           
            TextBoxPart1.TextChanged += UpdateText;
            TextBoxPart2.TextChanged += UpdateText;
            TextBoxPart3.TextChanged += UpdateText;
            TextBoxPart4.TextChanged += UpdateText;

            TextBoxPart1.GotFocus += TextBoxPart_GotFocus;
            TextBoxPart2.GotFocus += TextBoxPart_GotFocus;
            TextBoxPart3.GotFocus += TextBoxPart_GotFocus;
            TextBoxPart4.GotFocus += TextBoxPart_GotFocus;
        }

        private void TextBoxPart_GotFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            tb.SelectAll();
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
   "Text", typeof(string), typeof(IpAddressControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            try
            {
                var control = (IpAddressControl)d;
                string[] parts = ((string)e.NewValue)?.Split('.') ?? new string[0];
                if (parts.Length == 4)
                {
                    control.TextBoxPart1.Text = int.Parse(parts[0]) > 253 ? "253" : parts[0];
                    control.TextBoxPart2.Text = int.Parse(parts[1]) > 255 ? "255" : parts[1];
                    control.TextBoxPart3.Text = int.Parse(parts[2]) > 255 ? "255" : parts[2];
                    control.TextBoxPart4.Text = int.Parse(parts[3]) > 255 ? "255" : parts[3];
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
        private void UpdateText(object sender, TextChangedEventArgs e)
        {
            try
            {
                Text = $"{TextBoxPart1.Text}.{TextBoxPart2.Text}.{TextBoxPart3.Text}.{TextBoxPart4.Text}";
                TextChanged?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void IpPart_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                e.Handled = !int.TryParse(e.Text, out int value);
                var tb = sender as TextBox;
                if (tb != null && (tb.Text.Length >= 3 && tb.SelectionLength == 0) || (tb?.Text.Length == 0 && e.Text == ".") && !(tb.SelectedText.Length == tb.Text.Length))
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void IpPart_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                var tb = sender as TextBox;
                if (tb != null)
                {
                    if ((e.Key == Key.OemPeriod || e.Key == Key.Decimal))
                    {
                        e.Handled = true;
                        Dispatcher.InvokeAsync((() =>
                        {
                            tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                            
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                    }
                    else if (e.Key == Key.Space || e.Key == Key.Enter && tb.Text.Length == 3)
                    { 
                        e.Handled = true;
                        Dispatcher.InvokeAsync((() =>
                        {
                            tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                           
                        }), System.Windows.Threading.DispatcherPriority.Normal);
                    }
                  
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
