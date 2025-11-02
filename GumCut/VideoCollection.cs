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
        private string vcodec = string.Empty;
        private string resolution = string.Empty;
        private string duration = string.Empty;
        private double fps;
        private string pixel = string.Empty;
        private string vbitrate = string.Empty;
        private string acodec = string.Empty;
        private string abitrate = string.Empty;
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
        public string vCodec
        {
            get => vcodec; set
            {
                vcodec = value;
                OnPropertyChanged(nameof(vCodec));
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
        public string Pixel
        {
            get => pixel; set
            {
                pixel = value;
                OnPropertyChanged(nameof(Pixel));
            }
        }
        public string vBitrate
        {
            get => vbitrate; set
            {
                vbitrate = value;
                OnPropertyChanged(nameof(vBitrate));
            }
        }
        public string aCodec
        {
            get => acodec; set
            {
                acodec = value;
                OnPropertyChanged(nameof(aCodec));
            }
        }
        public string aBitrate
        {
            get => abitrate; set
            {
                abitrate = value;
                OnPropertyChanged(nameof(aBitrate));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
