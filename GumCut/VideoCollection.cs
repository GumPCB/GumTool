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
        private string resolution = string.Empty;
        private string duration = string.Empty;
        private double fps;
        private string bitrate = string.Empty;
        public bool IsSelected;
        public List<string> Subtitles = [];

        public VideoInfo(string _filename)
        {
            fileName = _filename;
        }

        public string FileName
        {
            get => fileName; set
            {
                fileName = value;
                OnPropertyChanged(nameof(FileName));
            }
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
        public string Resolution
        {
            get => resolution; set
            {
                resolution = value;
                OnPropertyChanged(nameof(Resolution));
            }
        }
        public string Duration
        {
            get => duration; set
            {
                duration = value;
                OnPropertyChanged(nameof(Duration));
            }
        }
        public double FPS
        {
            get => fps; set
            {
                fps = value;
                OnPropertyChanged(nameof(FPS));
            }
        }
        public string Bitrate
        {
            get => bitrate; set
            {
                bitrate = value;
                OnPropertyChanged(nameof(Bitrate));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
