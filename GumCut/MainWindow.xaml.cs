using GumTool;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace GumCut
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [GeneratedRegex("[^0-9]+")]
        private static partial Regex UnsignedIntRegex();
        [GeneratedRegex("[^0-9.]+")]
        private static partial Regex UnsignedDoubleRegex();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ScrollToEndTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
            e.Handled = true;
        }

        private void DoubleTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "." && ((TextBox)sender).Text.Contains('.', StringComparison.Ordinal))
            {
                e.Handled = true;
                return;
            }

            e.Handled = UnsignedDoubleRegex().IsMatch(e.Text);
        }

        private void HyperLink_Click(object sender, RoutedEventArgs e)
        {
            string url = ((Hyperlink)sender).NavigateUri.ToString();
            var task = Task.Run(() => WindowHelper.OpenUrlAsync(url));
            e.Handled = true;
        }

        private void Subtitles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as Cut)?.SelectionChangedSubtitles();
            e.Handled = true;
        }

        private void VideoList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (VideoInfo info in e.AddedItems)
            {
                info.IsSelected = true;
            }
            foreach (VideoInfo info in e.RemovedItems)
            {
                info.IsSelected = false;
            }
            (DataContext as Cut)?.UpdateStreams();
            (DataContext as Cut)?.UpdateSubtitles();
            e.Handled = true;
        }

        private void BatchDragOver(object sender, DragEventArgs e)
        {
            e.Effects = WindowHelper.IsFilesOrDirectorys(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void BatchDrop(object sender, DragEventArgs e)
        {
            var files = WindowHelper.IsFilesOrDirectorys(e);
            if (files == null) return;

            (DataContext as Cut)?.DragAndDropBatch(files);
            e.Handled = true;
        }

        private void SaveDirectoryDragOver(object sender, DragEventArgs e)
        {
            e.Effects = WindowHelper.IsDirectory(e) != null ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void SaveDirectoryDrop(object sender, DragEventArgs e)
        {
            var directory = WindowHelper.IsDirectory(e);
            if (directory == null) return;

            (DataContext as Cut)?.DragAndDropSaveDirectory(directory);
            e.Handled = true;
        }
        private void IntTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = UnsignedIntRegex().IsMatch(e.Text);
        private void SelectAllTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => ((TextBox)sender).SelectAll();
        private void SelectAllTextBox_GotMouseCapture(object sender, MouseEventArgs e) => ((TextBox)sender).SelectAll();
        private static readonly char[] InvalidFileNameChars = Path.GetInvalidFileNameChars();
        private void FileNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) => e.Handled = InvalidFileNameChars.Any(e.Text.Contains);

        private void VideoList_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
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
                (DataContext as Cut)?.DragAndDropBatch([.. files]);
                e.Handled = true;
            }
        }
    }
}