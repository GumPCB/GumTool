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

        public override string ToString() => $"{hour}:{minute:D2}:{seconds:D2}.{milliseconds:D3}";
        public bool IsZero => hour == 0 && minute == 0 && seconds == 0 && milliseconds == 0;
        public long TotalMilliseconds => (hour * 3600000L) + (minute * 60000) + (seconds * 1000) + milliseconds;

        public static bool operator >(Time time1, Time time2) => time1.TotalMilliseconds > time2.TotalMilliseconds;
        public static bool operator <(Time time1, Time time2) => time1.TotalMilliseconds < time2.TotalMilliseconds;

        public int Hour
        {
            get => hour; set
            {
                hour = Math.Max(0, Math.Min(value, 9999));
                OnPropertyChanged(nameof(Hour));
                if (hour != 0)
                    Checked = false;
            }
        }
        public int Minute
        {
            get => minute; set
            {
                minute = Math.Max(0, Math.Min(value, 59));
                OnPropertyChanged(nameof(Minute));
                if (minute != 0)
                    Checked = false;
            }
        }
        public int Seconds
        {
            get => seconds; set
            {
                seconds = Math.Max(0, Math.Min(value, 59));
                OnPropertyChanged(nameof(Seconds));
                if (seconds != 0)
                    Checked = false;
            }
        }
        public int Milliseconds
        {
            get => milliseconds; set
            {
                milliseconds = Math.Max(0, Math.Min(value, 999));
                OnPropertyChanged(nameof(Milliseconds));
                if (milliseconds != 0)
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

    public class Size : INotifyPropertyChanged
    {
        private uint width;
        private uint height;

        public override string ToString() => $"{width}:{height}";
        public string ToStringNotZero => $"{(width > 0 ? width : "-1")}:{(height > 0 ? height : "-1")}";
        public bool IsZero => width == 0 && height == 0;

        public uint Width
        {
            get => width; set
            {
                width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
        public uint Height
        {
            get => height; set
            {
                height = value;
                OnPropertyChanged(nameof(Height));
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
        private string loadVideo = string.Empty;
        private string saveVideo = string.Empty;
        private List<string> videoEncoders = [];
        private List<string> audioEncoders = [];
        private List<string> subtitleEncoders = [];
        private List<string> presets = [" ", "ultrafast", "superfast", "veryfast", "faster", "fast", "medium", "slow", "slower", "veryslow", "placebo"];
        private List<string> tunes = [" ", "film", "animation", "grain", "stillimage", "fastdecode", "zerolatency", "psnr", "ssim"];
        private Time start = new();
        private Time end = new();
        private bool streaming = true;
        private Size scale = new();         // Edit, Image
        private Size cropStart = new();     // Edit, Image
        private Size cropSize = new();      // Edit, Image
        private double fps;                 // Edit, Image
        private bool hFlip = false;         // Edit, Image
        private bool vFlip = false;         // Edit, Image
        private int rotation;               // Edit, Image
        private int selectedVideoEncoder;   // Edit
        private int selectedAudioEncoder;   // Edit
        private int selectedPreset;         // Edit
        private int selectedTune;           // Edit
        private int bitrate;                // Edit
        private int bitratemax;             // Edit
        private int bufsize;                // Edit
        private double crf = -1.0;          // Edit
        private int qp = -1;                // Edit
        private int saveZeroCount = 5;      // Image
        private int qscale;                 // Image
        private int imageFormat;            // Image
        private List<string> imageFormatType = ["GIF", "PNG", "JPG"];
        private string saveZeroName = "name_00001.jpg"; // Image

        public void EditTabClear()
        {
            Scale.Width = 0;
            Scale.Height = 0;
            CropStart.Width = 0;
            CropStart.Height = 0;
            CropSize.Width = 0;
            CropSize.Height = 0;
            Fps = 0;
            HFlip = false;
            VFlip = false;
            Rotation = 0;
            SelectedVideoEncoder = 0;
            SelectedAudioEncoder = 0;
            SelectedPreset = 0;
            SelectedTune = 0;
            Bitrate = 0;
            BitrateMax = 0;
            Bufsize = 0;
            CRF = -1.0;
            QP = -1;
        }

        public void ImageTabClear()
        {
            Scale.Width = 0;
            Scale.Height = 0;
            CropStart.Width = 0;
            CropStart.Height = 0;
            CropSize.Width = 0;
            CropSize.Height = 0;
            Fps = 0;
            HFlip = false;
            VFlip = false;
            Rotation = 0;
            ImageFormat = 0;
            SaveZeroCount = 5;
            Qscale = 0;
        }

        public string FFmpegFile
        {
            get => fFmpegFile; set
            {
                fFmpegFile = value;
                OnPropertyChanged(nameof(FFmpegFile));
            }
        }
        public string LoadVideo
        {
            get => loadVideo; set
            {
                loadVideo = value;
                OnPropertyChanged(nameof(LoadVideo));
            }
        }
        public string SaveVideo
        {
            get => saveVideo; set
            {
                saveVideo = value;
                OnPropertyChanged(nameof(SaveVideo));
            }
        }
        public List<string> VideoEncoders
        {
            get => videoEncoders; set
            {
                videoEncoders = value;
                OnPropertyChanged(nameof(VideoEncoders));
            }
        }
        public List<string> AudioEncoders
        {
            get => audioEncoders; set
            {
                audioEncoders = value;
                OnPropertyChanged(nameof(AudioEncoders));
            }
        }
        public List<string> SubtitleEncoders
        {
            get => subtitleEncoders; set
            {
                subtitleEncoders = value;
                OnPropertyChanged(nameof(SubtitleEncoders));
            }
        }
        public List<string> Presets => presets;
        public List<string> Tunes => tunes;
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
        public bool Streaming
        {
            get => streaming; set
            {
                streaming = value;
                OnPropertyChanged(nameof(Streaming));
            }
        }
        public Size Scale
        {
            get => scale; set
            {
                scale = value;
                OnPropertyChanged(nameof(Scale));
            }
        }
        public Size CropStart
        {
            get => cropStart; set
            {
                cropStart = value;
                OnPropertyChanged(nameof(CropStart));
            }
        }
        public Size CropSize
        {
            get => cropSize; set
            {
                cropSize = value;
                OnPropertyChanged(nameof(CropSize));
            }
        }
        public double Fps
        {
            get => fps; set
            {
                fps = value;
                OnPropertyChanged(nameof(Fps));
            }
        }
        public bool VFlip
        {
            get => vFlip; set
            {
                vFlip = value;
                OnPropertyChanged(nameof(VFlip));
            }
        }
        public bool HFlip
        {
            get => hFlip; set
            {
                hFlip = value;
                OnPropertyChanged(nameof(HFlip));
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
        public int SelectedVideoEncoder
        {
            get => selectedVideoEncoder; set
            {
                selectedVideoEncoder = value;
                OnPropertyChanged(nameof(SelectedVideoEncoder));
            }
        }
        public int SelectedAudioEncoder
        {
            get => selectedAudioEncoder; set
            {
                selectedAudioEncoder = value;
                OnPropertyChanged(nameof(SelectedAudioEncoder));
            }
        }
        public int SelectedPreset
        {
            get => selectedPreset; set
            {
                selectedPreset = value;
                OnPropertyChanged(nameof(SelectedPreset));
            }
        }
        public int SelectedTune
        {
            get => selectedTune; set
            {
                selectedTune = value;
                OnPropertyChanged(nameof(SelectedTune));
            }
        }
        public int Bitrate
        {
            get => bitrate; set
            {
                bitrate = value;
                OnPropertyChanged(nameof(Bitrate));
            }
        }
        public int BitrateMax
        {
            get => bitratemax; set
            {
                bitratemax = value;
                OnPropertyChanged(nameof(BitrateMax));
            }
        }
        public int Bufsize
        {
            get => bufsize; set
            {
                bufsize = value;
                OnPropertyChanged(nameof(Bufsize));
            }
        }
        public double CRF
        {
            get => crf; set
            {
                crf = value;
                OnPropertyChanged(nameof(CRF));
            }
        }
        public int QP
        {
            get => qp; set
            {
                qp = value;
                OnPropertyChanged(nameof(QP));
            }
        }
        public int ImageFormat
        {
            get => imageFormat; set
            {
                imageFormat = value;
                OnPropertyChanged(nameof(ImageFormat));
            }
        }
        public int SaveZeroCount
        {
            get => saveZeroCount; set
            {
                saveZeroCount = Math.Max(1, Math.Min(value, 100));
                OnPropertyChanged(nameof(SaveZeroCount));
                SaveZeroName = $"name_{new string('0', saveZeroCount-1)}1.jpg";
            }
        }
        public int Qscale
        {
            get => qscale; set
            {
                qscale = value;
                OnPropertyChanged(nameof(Qscale));
            }
        }
        public List<string> ImageFormatType
        {
            get => imageFormatType; set
            {
                imageFormatType = value;
                OnPropertyChanged(nameof(ImageFormatType));
            }
        }
        public string SaveZeroName
        {
            get => saveZeroName; set
            {
                saveZeroName = value;
                OnPropertyChanged(nameof(SaveZeroName));
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}