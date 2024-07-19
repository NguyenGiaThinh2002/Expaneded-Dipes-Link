using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DipesLink.Views.SubWindows
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class AboutPopup : Window
    {
        public AboutPopup()
        {
            InitializeComponent();
            LoadVersionInfo();
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            e.Handled = true;
        }

        private void LoadVersionInfo()
        {
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string versionFilePath = System.IO.Path.Combine(currentDirectory, "Assets", "SoftwareVersion.txt");
           

            if (File.Exists(versionFilePath))
            {
                string versionInfo = File.ReadAllText(versionFilePath);
                VersionRichTextBox.Document.Blocks.Clear();
                VersionRichTextBox.Document.Blocks.Add(new Paragraph(new Run(versionInfo)));
            }
            else
            {
                VersionRichTextBox.Document.Blocks.Clear();
                VersionRichTextBox.Document.Blocks.Add(new Paragraph(new Run("Version information not available.")));
            }
        }
    }
}
