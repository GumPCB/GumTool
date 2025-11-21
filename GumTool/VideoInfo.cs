using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GumTool
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
        private bool isSelected = false;
        public List<string> Streams = [];
        public List<string> Subtitles = [];
        public List<string> Thumbnails = [];    //Sorter

        public VideoInfo(string file_name)
        {
            fileName = file_name;
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
        public bool IsSelected
        {
            get => isSelected; set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static VideoInfo SplitInfo(string ffmpegResult, string filename)
        {
            VideoInfo info = new(filename);
            string[]? splitResult = ffmpegResult?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (splitResult == null || splitResult.Length == 0) return info;

            char[] delimiterChars = { ' ', ',', '.', ':' };
            foreach (string line in splitResult)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.Contains("Stream", StringComparison.Ordinal))
                {
                    string stream = line.Substring(line.IndexOf('#') + 1);
                    if (stream.Length != 0)
                    {
                        stream = stream.Replace(": ", " ")
                            .Replace('(', ' ')
                            .Replace(")", string.Empty)
                            .Replace('[', ' ')
                            .Replace("]", string.Empty)
                            .Replace("  ", " ");
                        info.Streams.Add(stream.Trim());
                    }
                }

                if (line.Contains("Stream", StringComparison.Ordinal) && line.Contains("Video", StringComparison.Ordinal) && !line.Contains("attached pic", StringComparison.Ordinal))
                {
                    string[] splitLine = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (string split in splitLine)
                    {
                        if (split.Contains("Video", StringComparison.Ordinal) && info.vCodec.Length == 0)
                        {
                            string[] encoder = split.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            for (int i = 0; i < encoder.Length; ++i)
                            {
                                if (encoder[i].Equals("Video:") && encoder.Length >= i + 2)
                                {
                                    info.vCodec = encoder[i + 1];
                                    break;
                                }
                            }
                        }
                        else if (info.Pixel.Length == 0 && (split.Contains("yuv", StringComparison.Ordinal) || split.Contains("nv", StringComparison.Ordinal) || split.Contains("rgb", StringComparison.Ordinal) || split.Contains("bgr", StringComparison.Ordinal) || split.Contains("gbr", StringComparison.Ordinal) || split.Contains("gray", StringComparison.Ordinal) || split.Contains("mono", StringComparison.Ordinal)))
                        {
                            string[] pixel = split.Split('(', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            if (pixel.Length >= 1)
                            {
                                info.Pixel = pixel[0];
                            }
                        }
                        else if (split.Contains("fps", StringComparison.Ordinal))
                        {
                            string[] fps = split.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            if (fps.Length >= 2)
                            {
                                if (double.TryParse(fps[0], out double fpsValue))
                                {
                                    info.FPS = fpsValue;
                                }
                            }
                        }
                        else if (split.Contains("b/s", StringComparison.Ordinal) && info.vBitrate.Length == 0)
                        {
                            info.vBitrate = split;
                        }
                        else if (split.Contains('x', StringComparison.Ordinal) && info.Resolution.Length == 0)
                        {
                            char[] resolutionDelimiterChars = { 'x', ' ' };
                            string[] resolution = split.Split(resolutionDelimiterChars, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            uint temp;
                            if (resolution.Length < 2 || info.Resolution.Length != 0 || uint.TryParse(resolution[0], out temp) == false || uint.TryParse(resolution[1], out temp) == false)
                                continue;

                            info.Resolution = resolution[0] + 'x' + resolution[1];
                        }
                    }
                }
                else if (line.Contains("Stream", StringComparison.Ordinal) && line.Contains("Audio", StringComparison.Ordinal))
                {
                    string[] splitLine = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (string split in splitLine)
                    {
                        if (split.Contains("Audio", StringComparison.Ordinal) && info.aCodec.Length == 0)
                        {
                            string[] encoder = split.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            for (int i = 0; i < encoder.Length; ++i)
                            {
                                if (encoder[i].Equals("Audio:") && encoder.Length >= i + 2)
                                {
                                    info.aCodec = encoder[i + 1];
                                    break;
                                }
                            }
                        }
                        else if (split.Contains("b/s", StringComparison.Ordinal) && info.aBitrate.Length == 0)
                        {
                            string[] bitrate = split.Split('(', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            if (bitrate.Length >= 1)
                                info.aBitrate = bitrate[0];
                        }
                    }
                }
                else if (line.Contains("Duration", StringComparison.Ordinal))
                {
                    string[] splitLine = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (string split in splitLine)
                    {
                        if (split.Contains("Duration", StringComparison.Ordinal))
                        {
                            string[] duration = split.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            if (duration.Length >= 2)
                            {
                                info.Duration = duration[1];
                                break;
                            }
                        }
                    }
                }
                else if (line.Contains("Stream", StringComparison.Ordinal) && line.Contains("Subtitle", StringComparison.Ordinal))
                {
                    string subtitle = line.Substring(line.IndexOf('#') + 1);
                    if (subtitle.Length != 0)
                    {
                        subtitle = subtitle.Replace("Subtitle: ", string.Empty)
                            .Replace(": ", " ")
                            .Replace('(', ' ')
                            .Replace(")", string.Empty)
                            .Replace('[', ' ')
                            .Replace("]", string.Empty)
                            .Replace("  ", " ");
                        info.Subtitles.Add(subtitle.Trim());
                    }
                }
            }

            return info;
        }
    }
    public class ReplaceCollection : ObservableCollection<ReplaceInfo>
    {
    }

    public class ReplaceInfo : INotifyPropertyChanged
    {
        private string original = string.Empty;
        private string replace = string.Empty;
        public string Original
        {
            get => original; set
            {
                original = value;
                OnPropertyChanged(nameof(Original));
            }
        }
        public string Replace
        {
            get => replace; set
            {
                replace = value;
                OnPropertyChanged(nameof(Replace));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

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

    public class StreamCollection : ObservableCollection<Stream>
    {
    }

    public class Stream : INotifyPropertyChanged
    {
        private bool _checked = false;
        private string line = string.Empty;

        public bool Checked
        {
            get => _checked; set
            {
                _checked = value;
                OnPropertyChanged(nameof(Checked));
            }
        }
        public string Line
        {
            get => line; set
            {
                line = value;
                OnPropertyChanged(nameof(Line));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
