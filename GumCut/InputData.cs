using System.ComponentModel;

namespace GumCut
{
    public class Time : INotifyPropertyChanged
    {
        private int hour;
        private int minute;
        private int seconds;
        private int milliseconds;
        private bool _checked = true;

        public int Hour
        {
            get => hour; set
            {
                hour = Math.Max(0, Math.Min(value, 9999));
                OnPropertyChanged(nameof(Hour));
                if (hour > 0)
                    Checked = false;
            }
        }
        public int Minute
        {
            get => minute; set
            {
                minute = Math.Max(0, Math.Min(value, 60));
                OnPropertyChanged(nameof(Minute));
                if (minute > 0)
                    Checked = false;
            }
        }
        public int Seconds
        {
            get => seconds; set
            {
                seconds = Math.Max(0, Math.Min(value, 60));
                OnPropertyChanged(nameof(Seconds));
                if (seconds > 0)
                    Checked = false;
            }
        }
        public int Milliseconds
        {
            get => milliseconds; set
            {
                milliseconds = Math.Max(0, Math.Min(value, 999));
                OnPropertyChanged(nameof(Milliseconds));
                if (milliseconds > 0)
                    Checked = false;
            }
        }
        public bool Checked
        {
            get => _checked; set
            {
                _checked = value;
                OnPropertyChanged(nameof(Checked));
                if (_checked == false)
                    return;

                Hour = 0;
                Minute = 0;
                Seconds = 0;
                Milliseconds = 0;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class InputData : INotifyPropertyChanged
    {
        private string fFmpegFile = string.Empty;
        private string inputVideo = string.Empty;
        private string outputVideo = string.Empty;
        private Time start = new Time();
        private Time end = new Time();
        private int rotation;

        public string FFmpegFile
        {
            get => fFmpegFile; set
            {
                fFmpegFile = value;
                OnPropertyChanged(nameof(FFmpegFile));
            }
        }
        public string InputVideo
        {
            get => inputVideo; set
            {
                inputVideo = value;
                OnPropertyChanged(nameof(InputVideo));
            }
        }
        public string OutputVideo
        {
            get => outputVideo; set
            {
                outputVideo = value;
                OnPropertyChanged(nameof(OutputVideo));
            }
        }
        public Time Start
        {
            get => start; set
            {
                start = value;
                OnPropertyChanged(nameof(Start));
            }
        }
        public Time End
        {
            get => end; set
            {
                end = value;
                OnPropertyChanged(nameof(End));
            }
        }
        public int Rotation
        {
            get => rotation; set
            {
                rotation = value;
                OnPropertyChanged(nameof(Rotation));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}