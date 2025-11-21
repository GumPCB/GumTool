using GumTool;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace GumCut
{
    public class Cut : INotifyPropertyChanged
    {
        private InputData data = new();
        private bool working;
        private bool fastCutTabControl = true;
        private bool editTabControl;
        private bool imageTabControl;
        private bool subtitleTabControl;
        private bool openCmd;
        private string resultText = string.Empty;
        private VideoCollection videoList = [];
        private string batchSaveDirectory = string.Empty;
        private long batchProgressCurrent;
        private long batchProgressMaximum;
        private string batchReplaceNameOld = "_cut";
        private string batchReplaceNameNew = string.Empty;
        public Command FFmpegOpenButton { get; set; }
        public Command CmdButton { get; set; }
        public Command VideoInfoButton { get; set; }
        public Command EraseButton { get; set; }
        public Command EraseCRFButton { get; set; }
        public Command EraseQPButton { get; set; }
        public Command BatchOpenFileButton { get; set; }
        public Command BatchOpenDirectoryButton { get; set; }
        public Command BatchSaveDirectoryButton { get; set; }
        public Command BatchRemoveSelectedButton { get; set; }
        public Command BatchRemoveAllButton { get; set; }
        public Command BatchMoveSelectedButton { get; set; }
        public Command BatchMoveAllButton { get; set; }
        public Command BatchCutSelectedButton { get; set; }
        public Command BatchCutAllButton { get; set; }
        public Command BatchReplaceNameButton { get; set; }

        public InputData Data
        {
            get => data;
            set
            {
                data = value;
                OnPropertyChanged(nameof(Data));
            }
        }
        public bool Working
        {
            get => working; set
            {
                working = value;
                OnPropertyChanged(nameof(Working));
            }
        }
        public bool FastCutTabControl
        {
            get => fastCutTabControl; set
            {
                fastCutTabControl = value;
                OnPropertyChanged(nameof(FastCutTabControl));
                if (fastCutTabControl == false) return;

                data.EditTabClear();
                data.ImageTabClear();
                data.SubtitleTabClear();
                UpdateStreams();
            }
        }
        public bool EditTabControl
        {
            get => editTabControl; set
            {
                editTabControl = value;
                OnPropertyChanged(nameof(EditTabControl));
                if (editTabControl == false) return;

                data.ImageTabClear();
                data.SubtitleTabClear();
                UpdateStreams();
            }
        }
        public bool ImageTabControl
        {
            get => imageTabControl; set
            {
                imageTabControl = value;
                OnPropertyChanged(nameof(ImageTabControl));
                if (imageTabControl == false) return;

                data.EditTabClear();
                data.SubtitleTabClear();
            }
        }
        public bool SubtitleTabControl
        {
            get => subtitleTabControl; set
            {
                subtitleTabControl = value;
                OnPropertyChanged(nameof(SubtitleTabControl));
                if (subtitleTabControl == false) return;

                data.EditTabClear();
                data.ImageTabClear();
                UpdateSubtitles();
            }
        }
        public bool OpenCmd
        {
            get => openCmd; set
            {
                openCmd = value;
                OnPropertyChanged(nameof(OpenCmd));
                SetupfileSave();
            }
        }
        public string ResultText
        {
            get => resultText; set
            {
                resultText = value;
                OnPropertyChanged(nameof(ResultText));
            }
        }
        public VideoCollection VideoList
        {
            get => videoList; set
            {
                videoList = value;
                OnPropertyChanged(nameof(VideoList));
            }
        }
        public string BatchSaveDirectory
        {
            get => batchSaveDirectory; set
            {
                batchSaveDirectory = value;
                OnPropertyChanged(nameof(BatchSaveDirectory));
                SetupfileSave();
            }
        }
        public long BatchProgressCurrent
        {
            get => batchProgressCurrent; set
            {
                batchProgressCurrent = value;
                OnPropertyChanged(nameof(BatchProgressCurrent));
            }
        }
        public long BatchProgressMaximum
        {
            get => batchProgressMaximum; set
            {
                batchProgressMaximum = value;
                OnPropertyChanged(nameof(BatchProgressMaximum));
            }
        }
        public string BatchReplaceNameOld
        {
            get => batchReplaceNameOld; set
            {
                batchReplaceNameOld = value;
                OnPropertyChanged(nameof(BatchReplaceNameOld));
            }
        }
        public string BatchReplaceNameNew
        {
            get => batchReplaceNameNew; set
            {
                batchReplaceNameNew = value;
                OnPropertyChanged(nameof(BatchReplaceNameNew));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Cut()
        {
            FFmpegOpenButton = new(FFmpegOpenExecutedCommand, EmptyCanExecuteCommand);
            CmdButton = new(CmdExecutedCommand, EmptyCanExecuteCommand);
            VideoInfoButton = new(VideoInfoExecutedCommand, EmptyCanExecuteCommand);
            EraseButton = new(EraseExecutedCommand, EmptyCanExecuteCommand);
            EraseCRFButton = new(EraseCRFExecutedCommand, EmptyCanExecuteCommand);
            EraseQPButton = new(EraseQPExecutedCommand, EmptyCanExecuteCommand);
            BatchOpenFileButton = new(BatchOpenFileExecutedCommand, EmptyCanExecuteCommand);
            BatchOpenDirectoryButton = new(BatchOpenDirectoryExecutedCommand, EmptyCanExecuteCommand);
            BatchSaveDirectoryButton = new(BatchSaveDirectoryExecutedCommand, EmptyCanExecuteCommand);
            BatchRemoveSelectedButton = new(BatchRemoveSelectedExecutedCommand, EmptyCanExecuteCommand);
            BatchRemoveAllButton = new(BatchRemoveAllExecutedCommand, EmptyCanExecuteCommand);
            BatchMoveSelectedButton = new(BatchMoveSelectedExecutedCommand, EmptyCanExecuteCommand);
            BatchMoveAllButton = new(BatchMoveAllExecutedCommand, EmptyCanExecuteCommand);
            BatchCutSelectedButton = new(BatchCutSelectedExecutedCommand, BatchCutCanExecuteCommand);
            BatchCutAllButton = new(BatchCutAllExecutedCommand, BatchCutCanExecuteCommand);
            BatchReplaceNameButton = new(BatchReplaceNameExecutedCommand, EmptyCanExecuteCommand);

            SetupfileLoad();
            IniFileLoad();
        }

        private const string LevelsFile = ".\\ini\\levels.ini";
        private const string PresetsFile = ".\\ini\\presets.ini";
        private const string ProfileFile = ".\\ini\\profile.ini";
        private const string TunesFile = ".\\ini\\tunes.ini";
        private const string SubtitleExtensionFile = ".\\ini\\subtitleExtension.ini";
        private void IniFileLoad()
        {
            if (File.Exists(LevelsFile))
            {
                using StreamReader sr = new(LevelsFile);
                string? line;
                while (sr.EndOfStream == false)
                {
                    line = sr.ReadLine();
                    if (line != null)
                        data.Levels.Add(line);
                }
                sr.Close();
            }
            if (File.Exists(PresetsFile))
            {
                using StreamReader sr = new(PresetsFile);
                string? line;
                while (sr.EndOfStream == false)
                {
                    line = sr.ReadLine();
                    if (line != null)
                        data.Presets.Add(line);
                }
                sr.Close();
            }
            if (File.Exists(ProfileFile))
            {
                using StreamReader sr = new(ProfileFile);
                string? line;
                while (sr.EndOfStream == false)
                {
                    line = sr.ReadLine();
                    if (line != null)
                        data.Profiles.Add(line);
                }
                sr.Close();
            }
            if (File.Exists(TunesFile))
            {
                using StreamReader sr = new(TunesFile);
                string? line;
                while (sr.EndOfStream == false)
                {
                    line = sr.ReadLine();
                    if (line != null)
                        data.Tunes.Add(line);
                }
                sr.Close();
            }
            if (File.Exists(SubtitleExtensionFile))
            {
                using StreamReader sr = new(SubtitleExtensionFile);
                string? line;
                while (sr.EndOfStream == false)
                {
                    line = sr.ReadLine();
                    if (line != null)
                        data.SubtitleExtensions.Add(line);
                }
                sr.Close();
            }
        }

        private const string SetupFile = ".\\ini\\Setup.ini";

        private void SetupfileLoad()
        {
            if (!File.Exists(SetupFile))
                return;

            using StreamReader sr = new(SetupFile);
            string? ffmpeg = sr.ReadLine();
            string? opencmd = sr.ReadLine();
            string? saveDirectory = sr.ReadLine();
            sr.Close();

            if (ffmpeg != null)
            {
                data.FFmpegFile = ffmpeg;
                GetEncoderInfo();
            }

            if (opencmd != null)
            {
                OpenCmd = opencmd.Equals("1");
            }

            if (saveDirectory != null)
            {
                BatchSaveDirectory = saveDirectory;
            }
        }

        private void SetupfileSave()
        {
            FileStream fs = new(SetupFile, FileMode.Create);
            using StreamWriter sw = new(fs);
            sw.WriteLine(data.FFmpegFile);
            sw.WriteLine(OpenCmd ? '1' : '0');
            sw.WriteLine(BatchSaveDirectory);
            sw.Close();
        }

        private void GetEncoderInfo()
        {
            ResultText += "===== Get Encoder Info : ";
            var task = Task.Run(() => FFmpegAsync("\"" + data.FFmpegFile + "\"", "-hide_banner -encoders", false, true)).ContinueWith((antecedent) =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    SplitEncoderInfo(FFmpegResultText);
                    if (data.VideoEncoders.Count == 0)
                    {
                        ResultText += "Fail =========\n";
                    }
                    else
                    {
                        ResultText += "Success! =========\n";
                    }
                    FFmpegResultText = string.Empty;
                });
            });
        }

        private void SplitEncoderInfo(string ffmpegResult)
        {
            data.VideoEncoders.Clear();
            data.AudioEncoders.Clear();
            data.SubtitleEncoders.Clear();
            data.SubtitleEncoderDescriptions.Clear();

            string[]? splitResult = ffmpegResult?.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (splitResult == null || splitResult.Length == 0) return;

            foreach (string line in splitResult)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] splits = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (splits.Length < 2)
                    continue;

                switch (splits[0][0])
                {
                    case 'A':
                        data.AudioEncoders.Add(splits[1]);
                        break;
                    case 'V':
                        data.VideoEncoders.Add(splits[1]);
                        break;
                    case 'S':
                        data.SubtitleEncoders.Add(splits[1]);
                        data.SubtitleEncoderDescriptions.Add(line.Substring(splits[0].Length + splits[1].Length + 2).Trim());
                        break;
                    default:
                        break;
                }
            }
        }

        private bool EmptyCanExecuteCommand(object? obj)
        {
            return true;
        }

        private void FFmpegOpenExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "ffmpeg",
                DefaultExt = ".exe",
                Filter = "FFmpeg file (.exe)|*.exe"
            };

            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            data.FFmpegFile = dialog.FileName;

            SetupfileSave();
            GetEncoderInfo();
        }

        private string GetSelectedTabArguments()
        {
            if (fastCutTabControl == true)
                return FFmpegArguments.FastCut(data, !openCmd);

            if (editTabControl == true)
                return FFmpegArguments.Edit(data, !openCmd);

            if (imageTabControl == true)
                return FFmpegArguments.Image(data, !openCmd);

            if (subtitleTabControl == true)
                return FFmpegArguments.Subtitle(data, !openCmd);

            return string.Empty;
        }

        private static string FFmpegResultText = string.Empty;

        public static async Task FFmpegAsync(string ffmpegFile, string arguments, bool openWindow, bool isStandardOutput)
        {
            ProcessStartInfo cmd = new(ffmpegFile, arguments);

            cmd.CreateNoWindow = !openWindow;
            cmd.UseShellExecute = false;
            cmd.RedirectStandardInput = false;
            cmd.RedirectStandardOutput = isStandardOutput && !openWindow;
            cmd.RedirectStandardError = !isStandardOutput && !openWindow;

            Process process = new();
            process.EnableRaisingEvents = false;
            process.StartInfo = cmd;
            process.Start();

            if (isStandardOutput && !openWindow)
            {
                FFmpegResultText += await process.StandardOutput.ReadToEndAsync();
            }
            else if (!openWindow)
            {
                FFmpegResultText += await process.StandardError.ReadToEndAsync();
            }

            await process.WaitForExitAsync();
            process.Close();
        }

        private void CmdExecutedCommand(object? obj)
        {
            bool isEmpty = false;
            if (data.LoadVideo.Length == 0 && data.SaveVideo.Length == 0 && VideoList.Count != 0)
            {
                isEmpty = true;
                foreach (VideoInfo info in VideoList)
                {
                    if (info.IsSelected == false)
                        continue;

                    data.LoadVideo = info.FileName;
                    data.SaveVideo = BatchSaveDirectory + "\\" + info.SaveName;
                    break;
                }
                if (data.LoadVideo.Length == 0 && data.SaveVideo.Length == 0)
                {
                    data.LoadVideo = VideoList.First().FileName;
                    data.SaveVideo = BatchSaveDirectory + "\\" + VideoList.First().SaveName;
                }
            }

            ResultText += $"\"{data.FFmpegFile}\" {GetSelectedTabArguments()}\n";

            if (isEmpty == true)
            {
                data.LoadVideo = string.Empty;
                data.SaveVideo = string.Empty;
            }
        }

        private List<string> videoFileNames = new();

        private void VideoInfoExecutedCommand(object? obj)
        {
            videoFileNames.Clear();
            foreach (VideoInfo info in VideoList)
            {
                if (info.IsSelected == false)
                    continue;

                videoFileNames.Add(info.FileName);
            }
            if (videoFileNames.Count == 0)
            {
                foreach (VideoInfo info in VideoList)
                {
                    videoFileNames.Add(info.FileName);
                }
            }

            GetVideoInfo();
        }

        private void GetVideoInfo()
        {
            if (videoFileNames.Count == 0)
                return;

            string fileName = videoFileNames.First();
            videoFileNames.RemoveAt(0);

            ResultText += "===== Get Video Info : " + fileName + "\n";
            var task = Task.Run(() => FFmpegAsync("\"" + data.FFmpegFile + "\"", "-hide_banner -i \"" + fileName + "\"", false, false)).ContinueWith((antecedent) =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    if (SplitVideoInfo(FFmpegResultText) == false)
                    {
                        ResultText += "Fail =========\n";
                        ResultText += FFmpegResultText;
                    }

                    FFmpegResultText = string.Empty;
                    GetVideoInfo();
                });
            });
        }

        private bool SplitVideoInfo(string ffmpegResult)
        {
            string[]? splitResult = ffmpegResult?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (splitResult == null || splitResult.Length == 0) return false;

            int textLength = ResultText.Length;

            foreach (string line in splitResult)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.Contains("Stream", StringComparison.Ordinal))
                {
                    ResultText += line + "\n";
                }
                else if (line.Contains("Duration", StringComparison.Ordinal))
                {
                    ResultText += line + "\n";
                }
            }

            return textLength < ResultText.Length;
        }

        private void EraseExecutedCommand(object? obj)
        {
            ResultText = string.Empty;
        }

        private void EraseCRFExecutedCommand(object? obj)
        {
            data.CRF = -1.0;
        }

        private void EraseQPExecutedCommand(object? obj)
        {
            data.QP = -1;
        }

        private void BatchGetInfo()
        {
            if (Working == true)
                return;

            string filename = string.Empty;
            foreach (VideoInfo info in VideoList)
            {
                if (info.vCodec.Length == 0)
                {
                    filename = info.FileName;
                    break;
                }
            }

            if (filename.Length == 0)
            {
                UpdateStreams();
                UpdateSubtitles();
                return;
            }

            Working = true;
            var task = Task.Run(() => FFmpegAsync("\"" + data.FFmpegFile + "\"", "-hide_banner -i \"" + filename + "\"", false, false)).ContinueWith((antecedent) =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    VideoInfo temp = VideoInfo.SplitInfo(FFmpegResultText, filename);
                    if (temp.vCodec.Length == 0)
                        temp.vCodec = "Not Found";
                    if (temp.aCodec.Length == 0)
                        temp.aCodec = "Not Found";
                    foreach (VideoInfo info in VideoList)
                    {
                        if (info.vCodec.Length != 0 || info.FileName.Equals(temp.FileName) == false)
                            continue;

                        info.vCodec = temp.vCodec;
                        info.Resolution = temp.Resolution;
                        info.Duration = temp.Duration;
                        info.FPS = temp.FPS;
                        info.Pixel = temp.Pixel;
                        info.vBitrate = temp.vBitrate;
                        info.aCodec = temp.aCodec;
                        info.aBitrate = temp.aBitrate;
                        info.SaveName = Path.GetFileNameWithoutExtension(info.FileName) + "_cut" + Path.GetExtension(info.FileName);
                        info.Streams = temp.Streams;
                        info.Subtitles = temp.Subtitles;
                        break;
                    }

                    FFmpegResultText = string.Empty;
                    Working = false;
                    BatchGetInfo();
                });
            });
        }

        private void BatchOpenFileExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            AddVideoList(dialog.FileNames);
            BatchGetInfo();
        }

        private void BatchOpenDirectoryExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            AddVideoList(dialog.FolderNames);
            BatchGetInfo();
        }

        private void BatchSaveDirectoryExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            BatchSaveDirectory = dialog.FolderName;
        }

        private void BatchRemoveSelectedExecutedCommand(object? obj)
        {
            for (int i = VideoList.Count - 1; i >= 0; --i)
            {
                if (VideoList[i].IsSelected)
                {
                    VideoList.RemoveAt(i);
                }
            }
        }

        private void BatchRemoveAllExecutedCommand(object? obj)
        {
            VideoList.Clear();
        }

        private void BatchMoveSelectedExecutedCommand(object? obj) => BatchMove(false);
        private void BatchMoveAllExecutedCommand(object? obj) => BatchMove(true);

        private void BatchMove(bool IsAll)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            foreach (VideoInfo info in VideoList)
            {
                if (info.IsSelected == false && IsAll == false)
                    continue;

                string destFilename = dialog.FolderName + "\\" + Path.GetFileNameWithoutExtension(info.FileName) + Path.GetExtension(info.FileName);
                File.Move(info.FileName, destFilename);
                if (File.Exists(destFilename))
                {
                    info.FileName = destFilename;
                }
            }
        }

        private void BatchCutSelectedExecutedCommand(object? obj) => BatchCut(false);
        private void BatchCutAllExecutedCommand(object? obj) => BatchCut(true);

        private bool BatchCutCanExecuteCommand(object? obj)
        {
            if (data.FFmpegFile.Length == 0 || BatchSaveDirectory.Length == 0) return false;
            return true;
        }

        private readonly List<string> inputs = [];
        private readonly List<string> outputs = [];
        private readonly List<long> durations = [];  // milliseconds
        private void BatchCut(bool IsAll)
        {
            if (!Directory.Exists(BatchSaveDirectory))
            {
                Directory.CreateDirectory(BatchSaveDirectory);
            }

            inputs.Clear();
            outputs.Clear();
            durations.Clear();
            BatchProgressCurrent = 0L;
            BatchProgressMaximum = 0L;

            foreach (VideoInfo info in VideoList)
            {
                if (info.IsSelected == false && IsAll == false)
                    continue;

                if (info.SaveName.Length == 0)
                {
                    ResultText += "====== ERROR : EMPTY SaveName\n";
                    return;
                }

                inputs.Add(info.FileName);
                outputs.Add(BatchSaveDirectory + "\\" + info.SaveName);

                long duration = 0L;
                if (TimeSpan.TryParse(info.Duration, out TimeSpan ts))
                    duration = (long)(ts.TotalMilliseconds);

                if (duration == 0L)
                    duration = 1L;

                BatchProgressMaximum += duration;
                durations.Add(duration);
            }

            RecursiveBatchCut();
        }

        private static Stopwatch stopwatch = new Stopwatch();

        private void RecursiveBatchCut()
        {
            if (inputs.Count == 0 || outputs.Count == 0) return;

            if (Working == true)
            {
                ResultText += "Working...\n";
                return;
            }

            data.LoadVideo = inputs.First();
            data.SaveVideo = outputs.First();
            inputs.RemoveAt(0);
            outputs.RemoveAt(0);

            Working = true;
            stopwatch.Restart();
            ResultText += $"====== Working Start : {DateTime.Now.ToString("F")}\n";
            ResultText += $"Working File : {data.LoadVideo}\n";

            string ffmpegFile = new($"\"{data.FFmpegFile}\"");
            string arguments = GetSelectedTabArguments();

            var task = Task.Run(() => FFmpegAsync(ffmpegFile, arguments, openCmd, false)).ContinueWith((antecedent) =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Working = false;
                    stopwatch.Stop();
                    ResultText += (FFmpegResultText.Length == 0) ? "========= Success! =========\n" : FFmpegResultText;
                    ResultText += $"====== Working End : {DateTime.Now:F} ({stopwatch.ElapsedMilliseconds / 1000L} Seconds, {(double)durations.First() / stopwatch.ElapsedMilliseconds:N3}x)\n";
                    data.LoadVideo = string.Empty;
                    data.SaveVideo = string.Empty;
                    FFmpegResultText = string.Empty;
                    BatchProgressCurrent += durations.First();
                    durations.RemoveAt(0);
                    RecursiveBatchCut();
                });
            });
        }

        private void BatchReplaceNameExecutedCommand(object? obj)
        {
            if (batchReplaceNameOld.Length == 0)
                return;

            foreach (VideoInfo info in VideoList)
            {
                info.SaveName = info.SaveName.Replace(batchReplaceNameOld, batchReplaceNameNew);
            }
        }

        internal void DragAndDropBatch(string[] files)
        {
            AddVideoList(files);

            BatchGetInfo();
        }

        internal void DragAndDropSaveDirectory(string directory)
        {
            BatchSaveDirectory = directory;
        }

        private void AddVideoList(string[] files)
        {
            foreach (string file in files)
            {
                if (Directory.Exists(file))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(file);
                    foreach (FileInfo info in directoryInfo.GetFiles())
                    {
                        AddFileVideoList(info.FullName);
                    }
                    foreach (DirectoryInfo info in directoryInfo.GetDirectories())
                    {
                        AddVideoList([info.FullName]);
                    }
                }
                else if (File.Exists(file))
                {
                    AddFileVideoList(file);
                }
            }
        }

        private void AddFileVideoList(string file)
        {
            if (file.Contains("Thumbs.db", StringComparison.OrdinalIgnoreCase))
                return;

            VideoInfo info = new(file);
            VideoList.Add(info);
        }

        internal void SelectionChangedSubtitles()
        {
            if (data.SelectedSubtitle < 0 || data.Subtitles.Count == 0 || data.Subtitles.Count <= data.SelectedSubtitle)
                return;

            data.SelectedSubtitleEncoder = 0;

            string[] splitSubtitle = data.Subtitles[data.SelectedSubtitle].Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (splitSubtitle.Length == 0)
                return;

            for (int i = 0; i < data.SubtitleEncoders.Count; ++i)
            {
                foreach (string split in splitSubtitle)
                {
                    if (data.SubtitleEncoders[i].Contains(split, StringComparison.Ordinal) || data.SubtitleEncoderDescriptions[i].Contains(split, StringComparison.Ordinal))
                    {
                        data.SelectedSubtitleEncoder = i;
                        return;
                    }
                }
            }
        }

        internal void UpdateSubtitles()
        {
            if (subtitleTabControl == false || VideoList.Count == 0)
                return;

            foreach (VideoInfo info in VideoList)
            {
                if (info.IsSelected == true)
                {
                    data.Subtitles = info.Subtitles;
                    return;
                }
            }

            data.Subtitles = VideoList[0].Subtitles;
        }

        internal void UpdateStreams()
        {
            if ((fastCutTabControl == false && editTabControl == false) || VideoList.Count == 0)
                return;

            int selectedIndex = 0;
            for (int i = 0; i < VideoList.Count; ++i)
            {
                if (VideoList[i].IsSelected == true)
                {
                    selectedIndex = i;
                    break;
                }
            }

            while (data.Streams.Count > VideoList[selectedIndex].Streams.Count)
            {
                data.Streams.Remove(data.Streams[data.Streams.Count - 1]);
            }
            if (data.Streams.Count < VideoList[selectedIndex].Streams.Count)
            {
                int addCount = VideoList[selectedIndex].Streams.Count - data.Streams.Count;
                for (int i = 0; i < addCount; ++i)
                {
                    data.Streams.Add(new GumTool.Stream());
                }
            }

            for (int i = 0; i < VideoList[selectedIndex].Streams.Count; ++i)
            {
                data.Streams[i].Checked = false;
                data.Streams[i].Line = VideoList[selectedIndex].Streams[i];
            }
        }
    }
}