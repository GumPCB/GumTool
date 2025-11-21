using System.ComponentModel;

namespace GumTool
{
    public class InputData : INotifyPropertyChanged
    {
        private string fFmpegFile = string.Empty;
        private string loadVideo = string.Empty;
        private string saveVideo = string.Empty;
        private List<string> videoEncoders = [];
        private List<string> audioEncoders = [];
        private List<string> subtitleEncoders = [];
        private List<string> subtitleEncoderDescriptions = [];
        private List<string> subtitleExtensions = ["="];
        private List<string> presets = ["="];
        private List<string> tunes = ["="];
        private List<string> profiles = ["="];
        private List<string> levels = ["="];
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
        private int selectedSubtitleEncoder;// Subtitle
        private int selectedPreset;         // Edit
        private int selectedTune;           // Edit
        private int selectedProfile;        // Edit
        private int selectedLevel;          // Edit
        private int bitrate;                // Edit
        private int bitratemax;             // Edit
        private int bufsize;                // Edit
        private double crf = -1.0;          // Edit
        private int qp = -1;                // Edit
        private int saveZeroCount = 5;      // Image
        private int qscale;                 // Edit, Image
        private int qscaleAudio;            // Edit
        private int imageFormat = 1;        // Image
        private List<string> imageFormatType = ["GIF", "PNG", "JPG"];
        private string saveZeroName = "name_00001.jpg"; // Image
        private int selectedSubtitle;       // Subtitle
        private List<string> subtitles = [];// Subtitle
        private StreamCollection streams = [];  // Cut, Edit

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
            SelectedProfile = 0;
            SelectedLevel = 0;
            Bitrate = 0;
            BitrateMax = 0;
            Bufsize = 0;
            CRF = -1.0;
            QP = -1;
            Qscale = 0;
            QscaleAudio = 0;
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
            ImageFormat = 1;
            SaveZeroCount = 5;
            Qscale = 0;
        }

        public void SubtitleTabClear()
        {
            SelectedSubtitleEncoder = 0;
            Subtitles = [];
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
        public List<string> SubtitleEncoderDescriptions
        {
            get => subtitleEncoderDescriptions; set
            {
                subtitleEncoderDescriptions = value;
                OnPropertyChanged(nameof(SubtitleEncoderDescriptions));
            }
        }
        public List<string> SubtitleExtensions
        {
            get => subtitleExtensions; set
            {
                subtitleExtensions = value;
                OnPropertyChanged(nameof(SubtitleExtensions));
            }
        }
        
        public List<string> Presets
        {
            get => presets; set
            {
                presets = value;
                OnPropertyChanged(nameof(Presets));
            }
        }

        public List<string> Tunes
        {
            get => tunes; set
            {
                tunes = value;
                OnPropertyChanged(nameof(Tunes));
            }
        }

        public List<string> Profiles
        {
            get => profiles; set
            {
                profiles = value;
                OnPropertyChanged(nameof(Profiles));
            }
        }
        public List<string> Levels
        {
            get => levels; set
            {
                levels = value;
                OnPropertyChanged(nameof(Levels));
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
        public int SelectedSubtitleEncoder
        {
            get => selectedSubtitleEncoder; set
            {
                selectedSubtitleEncoder = value;
                OnPropertyChanged(nameof(SelectedSubtitleEncoder));
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
        public int SelectedProfile
        {
            get => selectedProfile; set
            {
                selectedProfile = value;
                OnPropertyChanged(nameof(SelectedProfile));
            }
        }
        public int SelectedLevel
        {
            get => selectedLevel; set
            {
                selectedLevel = value;
                OnPropertyChanged(nameof(SelectedLevel));
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
                qscale = Math.Max(0, Math.Min(value, 31));
                OnPropertyChanged(nameof(Qscale));
            }
        }
        public int QscaleAudio
        {
            get => qscaleAudio; set
            {
                qscaleAudio = Math.Max(0, Math.Min(value, 31));
                OnPropertyChanged(nameof(QscaleAudio));
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
        public int SelectedSubtitle
        {
            get => selectedSubtitle;
            set
            {
                selectedSubtitle = value;
                OnPropertyChanged(nameof(SelectedSubtitle));
            }
        }
        public List<string> Subtitles
        {
            get => subtitles;
            set
            {
                if (subtitles.Equals(value))
                    return;

                subtitles = value;
                OnPropertyChanged(nameof(Subtitles));
                SelectedSubtitle = 0;
            }
        }
        public StreamCollection Streams
        {
            get => streams;
            set
            {
                streams = value;
                OnPropertyChanged(nameof(Streams));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}