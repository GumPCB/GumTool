using System.Diagnostics;
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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ScrollToEndTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
        }

        private void IntTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void DoubleTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "." && ((TextBox)sender).Text.Contains('.', StringComparison.Ordinal))
            {
                e.Handled = true;
                return;
            }

            Regex regex = new("[^0-9.]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void HyperLink_Click(object sender, RoutedEventArgs e)
        {
            string url = ((Hyperlink)sender).NavigateUri.ToString();
            var task = Task.Run(() => OpenUrlAsync(url));
        }

        public static async Task OpenUrlAsync(string url)
        {
            ProcessStartInfo cmd = new(url);

            cmd.CreateNoWindow = false;
            cmd.UseShellExecute = true;
            cmd.RedirectStandardInput = false;
            cmd.RedirectStandardOutput = false;
            cmd.RedirectStandardError = false;

            Process process = new();
            process.EnableRaisingEvents = false;
            process.StartInfo = cmd;
            process.Start();

            await process.WaitForExitAsync();
            process.Close();
        }

        private void Subtitles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            (DataContext as Cut)?.SelectionChangedSubtitles();
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
            (DataContext as Cut)?.UpdateSubtitles();
        }

        private void BatchDragOver(object sender, DragEventArgs args)
        {
            args.Effects = IsFilesOrDirectorys(args) != null ? DragDropEffects.Copy : DragDropEffects.None;
            args.Handled = true;
        }

        private void BatchDrop(object sender, DragEventArgs args)
        {
            args.Handled = true;

            var files = IsFilesOrDirectorys(args);
            if (files == null) return;

            (DataContext as Cut)?.DragAndDropBatch(files);
        }

        private string[]? IsFilesOrDirectorys(DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var files = args.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (File.Exists(file) == false && Directory.Exists(file) == false)
                            return null;
                    }
                    return files;
                }
            }
            return null;
        }

        private void SaveDirectoryDragOver(object sender, DragEventArgs args)
        {
            args.Effects = IsDirectory(args) != null ? DragDropEffects.Copy : DragDropEffects.None;
            args.Handled = true;
        }

        private void SaveDirectoryDrop(object sender, DragEventArgs args)
        {
            args.Handled = true;

            var directory = IsDirectory(args);
            if (directory == null) return;

            (DataContext as Cut)?.DragAndDropSaveDirectory(directory);
        }

        private string? IsDirectory(DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var directorys = args.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (directorys != null)
                {
                    foreach (var directory in directorys)
                    {
                        if (Directory.Exists(directory) == true)
                            return directory;
                    }
                    return null;
                }
            }
            return null;
        }

        private void SelectAllTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void SelectAllTextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }
    }
}