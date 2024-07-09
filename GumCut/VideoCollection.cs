using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace GumCut
{
    public class VideoCollection : ObservableCollection<VideoInfo>
    {
    }

    public class VideoInfo : INotifyPropertyChanged
    {
        private string fileName = string.Empty;
        private string saveName = string.Empty;
        private string encoder = string.Empty;
        private Size resolution = new();
        private Time duration = new();

        public VideoInfo(string _filename)
        {
            fileName = _filename;
        }

        public string FileName
        {
            get => fileName;
        }
        public string SaveName
        {
            get => saveName; set
            {
                saveName = value;
                OnPropertyChanged(nameof(SaveName));
            }
        }
        public string Encoder
        {
            get => encoder; set
            {
                encoder = value;
                OnPropertyChanged(nameof(Encoder));
            }
        }
        public Size Resolution
        {
            get => resolution; set
            {
                resolution = value;
                OnPropertyChanged(nameof(Resolution));
            }
        }
        public Time Duration
        {
            get => duration; set
            {
                duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
