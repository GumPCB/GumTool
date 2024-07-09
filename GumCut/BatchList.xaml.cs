using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    }
}
