using DipesLink.ViewModels;
using DipesLink.Views.Extension;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ListBox = System.Windows.Controls.ListBox;
using UserControl = System.Windows.Controls.UserControl;

namespace DipesLink.Views.UserControls.MainUc
{
    /// <summary>
    /// Interaction logic for AllStationUc.xaml
    /// </summary>
    public partial class AllStationUc : UserControl
    {
        public static event EventHandler? DoneLoadUIEvent;
        private MainViewModel ViewModel;
        public AllStationUc()
        {
            InitializeComponent();
            Loaded += AllStationUc_Loaded1;
           // LoadItems();
            EventRegister();
        }

        private void AllStationUc_Loaded1(object sender, RoutedEventArgs e)
        {
            ViewModel = (MainViewModel)DataContext;
            if (ViewModel != null)
            {
                LoadItems();
            }
        }

        private void LoadItems()
        {
            foreach (var vm in ViewModel.JobList)
            {
                var uc = new OverviewJobUc
                {
                    Margin = new Thickness(0, 10, 0, 0),
                    Height = 210,
                    ViewModel = vm
                };
                ItemsPanel.Children.Add(uc);
            }
        }

        void EventRegister()
        {
            /// ListBoxJobs.DragOver += ListBoxJobs_DragOver;
            Loaded += AllStationUc_Loaded;
            MainWindow.MainWindowSizeChangeCustomEvent += MainWindow_MainWindowSizeChangeCustomEvent;
        }

        #region Reposive ListBox Item Grid 
        private void MainWindow_MainWindowSizeChangeCustomEvent(object? sender, EventArgs e)
        {

            double? mainWindowWidth = (double?)sender;
            foreach (var item in FindChildControlUI.grids)
            {
                if (item is not null && mainWindowWidth is not null)
                    item.Width = (double)(mainWindowWidth - 150);
            }
        }
        private void AllStationUc_Loaded(object sender, RoutedEventArgs e)
        {
            //  FindChildControlUI.FindVisualChild<Grid>(ListBoxJobs, "GridCoverItems");
            DoneLoadUIEvent?.Invoke(null, EventArgs.Empty);
        }
        #endregion Reposive ListBox Item Grid 

        #region ListBoxDragToScroll
        private void ListBoxJobs_DragOver(object sender, DragEventArgs e)
        {
            ListBox? li = sender as ListBox;
            ScrollViewer? sv = FindVisualChild<ScrollViewer>(li);
            double tolerance = 24;
            double verticalPos = e.GetPosition(li).Y;
            double topMargin = tolerance;
            var bottomMargin = li.ActualHeight - tolerance;
            if (sv?.ComputedHorizontalScrollBarVisibility == Visibility.Visible)
            {
                System.Windows.Controls.Primitives.ScrollBar? horizontalScrollBar = sv.Template.FindName("PART_HorizontalScrollBar", sv) as System.Windows.Controls.Primitives.ScrollBar;

                if (horizontalScrollBar != null)
                {
                    bottomMargin -= horizontalScrollBar.ActualHeight;
                }
            }

            double distanceToScroll = 3;
            if (verticalPos < topMargin)
            {
                sv?.ScrollToVerticalOffset(sv.VerticalOffset - distanceToScroll);
            }
            else if (verticalPos > bottomMargin)
            {
                sv?.ScrollToVerticalOffset(sv.VerticalOffset + distanceToScroll);
            }
        }
        public static childItem? FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                if (child != null && child is childItem)
                    return (childItem)child;

                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);

                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }
        #endregion ListBoxDragToScroll
    }
}
