using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        private void EhDragOver(object sender, DragEventArgs args)
        {
            args.Effects = IsFileOrDirectory(args) != null ? DragDropEffects.Copy : DragDropEffects.None;
            args.Handled = true;
        }

        private void EhDrop(object sender, DragEventArgs args)
        {
            args.Handled = true;

            var fileName = IsFileOrDirectory(args);
            if (fileName == null) return;

            (DataContext as Cut)?.DragAndDropFile(fileName);
        }

        private string? IsFileOrDirectory(DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var fileNames = args.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (fileNames != null)
                {
                    if (File.Exists(fileNames[0]) || Directory.Exists(fileNames[0]))
                    {
                        return fileNames[0];
                    }
                }
            }
            return null;
        }
    }
}