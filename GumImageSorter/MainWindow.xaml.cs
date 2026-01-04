using GumTool;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace GumImageSorter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void ScrollToEndTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
            e.Handled = true;
        }

        private void HyperLink_Click(object sender, RoutedEventArgs e)
        {
            string url = ((Hyperlink)sender).NavigateUri.ToString();
            var task = Task.Run(() => WindowHelper.OpenUrlAsync(url));
            e.Handled = true;
        }

        private void ImageList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (ImageInfo info in e.AddedItems)
            {
                info.IsSelected = true;
                (DataContext as Sorter)?.ImageSelectionChanged(info);
            }
            foreach (ImageInfo info in e.RemovedItems)
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
                    Before = info.Before,
                    After = info.After
                };
            }
            e.Handled = true;
        }

        private void FilesOrDirectorys_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = WindowHelper.IsFilesOrDirectorys(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void ImageList_Drop(object sender, DragEventArgs e)
        {
            var files = WindowHelper.IsFilesOrDirectorys(e);
            if (files == null) return;

            (DataContext as Sorter)?.ImageList_DragAndDrop(files);
            e.Handled = true;
        }

        private void SelectAllTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => ((TextBox)sender).SelectAll();
        private void SelectAllTextBox_GotMouseCapture(object sender, MouseEventArgs e) => ((TextBox)sender).SelectAll();
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private void FileNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = InvalidFileNameChars.Any(e.Text.Contains);

        private void ImageList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                ListView listView = (ListView)sender;
                if (listView.SelectedItems.Count == 0) return;

                string lastSelectedFileName = string.Empty;
                foreach (var item in listView.SelectedItems)
                {
                    if (item is ImageInfo info)
                    {
                        lastSelectedFileName = info.FileName;
                    }
                }

                int selectedIndex = -1;
                ImageCollection videoList = (ImageCollection)listView.ItemsSource;
                for (int i = videoList.Count - 1; i >= 0; i--)
                {
                    if (videoList[i].IsSelected)
                    {
                        if (selectedIndex == -1 && videoList[i].FileName.Equals(lastSelectedFileName, StringComparison.OrdinalIgnoreCase))
                        {
                            selectedIndex = i;
                        }
                        else if (selectedIndex > 0)
                        {
                            selectedIndex--;
                        }
                        (DataContext as Sorter)?.AddDeleteList(videoList[i]);
                        videoList.RemoveAt(i);
                    }
                }

                if (videoList.Count <= selectedIndex)
                {
                    selectedIndex = videoList.Count - 1;
                }

                listView.SelectedIndex = selectedIndex;
                listView.ScrollIntoView(listView.SelectedItem);

                e.Handled = true;

                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                {
                    ListViewItem item = (ListViewItem)listView.ItemContainerGenerator.ContainerFromIndex(selectedIndex);
                    item?.Focus();
                }));
            }
            else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (!Clipboard.ContainsFileDropList()) return;
                var files = new List<string>();
                var fileDropList = Clipboard.GetFileDropList();
                foreach (var item in fileDropList)
                {
                    if (item != null)
                    {
                        files.Add(item);
                    }
                }
                (DataContext as Sorter)?.ImageList_DragAndDrop([.. files]);
                e.Handled = true;
            }
        }
    }
}