using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PetOffice.WpfApp.Views
{
    public partial class AnalysisPanel : UserControl
    {
        public AnalysisPanel()
        {
            InitializeComponent();
            Loaded += AnalysisPanel_Loaded;
        }

        private void AnalysisPanel_Loaded(object sender, RoutedEventArgs e)
        {
            SetupOverlayScrollBar(SequentialDataGrid, SequentialOverlayScrollBar);
            SetupOverlayScrollBar(ParallelDataGrid, ParallelOverlayScrollBar);
        }

        private void SetupOverlayScrollBar(DataGrid dataGrid, ScrollBar scrollBar)
        {
            ScrollViewer? viewer = FindVisualChild<ScrollViewer>(dataGrid);

            if (viewer is null)
            {
                return;
            }

            void UpdateScrollBar()
            {
                scrollBar.Maximum = viewer.ScrollableHeight;
                scrollBar.ViewportSize = viewer.ViewportHeight;
                scrollBar.Value = viewer.VerticalOffset;

                scrollBar.Visibility = viewer.ScrollableHeight > 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            viewer.ScrollChanged += (_, _) => UpdateScrollBar();

            scrollBar.ValueChanged += (_, _) =>
            {
                if (Math.Abs(viewer.VerticalOffset - scrollBar.Value) > 0.1)
                {
                    viewer.ScrollToVerticalOffset(scrollBar.Value);
                }
            };

            UpdateScrollBar();
        }

        private static T? FindVisualChild<T>(DependencyObject parent)
            where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is T result)
                {
                    return result;
                }

                T? descendant = FindVisualChild<T>(child);

                if (descendant is not null)
                {
                    return descendant;
                }
            }

            return null;
        }
    }
}