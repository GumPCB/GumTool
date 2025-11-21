using GumTool;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace GumSorter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [GeneratedRegex("[^0-9]+")]
        private static partial Regex UnsignedIntRegex();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void HyperLink_Click(object sender, RoutedEventArgs e)
        {
            string url = ((Hyperlink)sender).NavigateUri.ToString();
            var task = Task.Run(() => WindowHelper.OpenUrlAsync(url));
            e.Handled = true;
        }

        private void TempDirectory_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = WindowHelper.IsDirectory(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void TempDirectory_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            var directory = WindowHelper.IsDirectory(e);
            if (directory == null) return;

            (DataContext as Sorter)?.DragAndDropTempDirectory(directory);
        }

        private void VideoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (VideoInfo info in e.AddedItems)
            {
                info.IsSelected = true;
                (DataContext as Sorter)?.VideoSelectionChanged(info);
            }
            foreach (VideoInfo info in e.RemovedItems)
            {
                info.IsSelected = false;
            }
            e.Handled = true;
        }

        private void ReplaceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (ReplaceInfo info in e.AddedItems)
            {
                (DataContext as Sorter)?.ReplaceName = new ReplaceInfo()
                {
                    Original = info.Original,
                    Replace = info.Replace
                };
            }
            e.Handled = true;
        }

        private void VideoList_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = WindowHelper.IsFilesOrDirectorys(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void VideoList_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            var files = WindowHelper.IsFilesOrDirectorys(e);
            if (files == null) return;
            
            (DataContext as Sorter)?.VideoList_DragAndDrop(files);
        }

        private void ThumbnailImage_Wheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                (DataContext as Sorter)?.PreviousThumbnailImage();
            }
            else
            {
                (DataContext as Sorter)?.NextThumbnailImage();
            }
            e.Handled = true;
        }

        private void IntTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = UnsignedIntRegex().IsMatch(e.Text);
        private void SelectAllTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => ((TextBox)sender).SelectAll();
        private void SelectAllTextBox_GotMouseCapture(object sender, MouseEventArgs e) => ((TextBox)sender).SelectAll();
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private void FileNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = InvalidFileNameChars.Any(e.Text.Contains);
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e) => (DataContext as Sorter)?.DeleteThumbnailDirectorys();

        private void VideoList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                (DataContext as Sorter)?.PreviousThumbnailImage();
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                (DataContext as Sorter)?.NextThumbnailImage();
                e.Handled = true;
            }
            else if (e.Key == Key.Delete)
            {
                e.Handled = true;

                ListView listView = (ListView)sender;
                if (listView.SelectedItem == null || listView.SelectedIndex < 0) return;

                VideoCollection videoList = (VideoCollection)listView.ItemsSource;

                int selectedIndex = listView.SelectedIndex;
                if (videoList.Count == 0 || videoList.Count <= selectedIndex) return;

                (DataContext as Sorter)?.AddDeleteList((VideoInfo)listView.SelectedItem);
                videoList.RemoveAt(selectedIndex);

                if (videoList.Count == 0)
                {
                    (DataContext as Sorter)?.ClearThumbnailImage();
                    return;
                }
                else if (selectedIndex >= videoList.Count)
                {
                    selectedIndex = videoList.Count - 1;
                }

                listView.SelectedIndex = selectedIndex;
                listView.ScrollIntoView(listView.SelectedItem);

                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                {
                    ListViewItem item = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(selectedIndex);
                    item?.Focus();
                }));
            }
        }
    }
}