using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GumCut
{
    /// <summary>
    /// BatchList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BatchList : Window
    {
        public BatchList()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => Hide());
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
        }

        private void BatchDragOver(object sender, DragEventArgs args)
        {
            args.Effects = IsDirectory(args) != null ? DragDropEffects.Copy : DragDropEffects.None;
            args.Handled = true;
        }

        private void BatchDrop(object sender, DragEventArgs args)
        {
            args.Handled = true;

            var directorys = IsDirectory(args);
            if (directorys == null) return;

            (DataContext as Cut)?.DragAndDropBatchDirectory(directorys);
        }

        private string[]? IsDirectory(DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var directorys = args.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (directorys != null)
                {
                    foreach (var directory in directorys)
                    {
                        if (Directory.Exists(directory) == false)
                            return null;
                    }
                    return directorys;
                }
            }
            return null;
        }
    }
}
