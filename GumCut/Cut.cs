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
        private bool openCmd;
        private string resultText = string.Empty;
        private VideoCollection videoList = [];
        private string batchSaveDirectory = string.Empty;
        private int batchProgressCurrent;
        private int batchProgressMaximum;
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

        private enum SelectedTab
        {
            FastCutTab,
            EditTab,
            ImageTab
        }
        private SelectedTab CurrentSelectedTab = SelectedTab.FastCutTab;

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
                CurrentSelectedTab = SelectedTab.FastCutTab;
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
                CurrentSelectedTab = SelectedTab.EditTab;
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
                CurrentSelectedTab = SelectedTab.ImageTab;
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
        public int BatchProgressCurrent
        {
            get => batchProgressCurrent; set
            {
                batchProgressCurrent = value;
                OnPropertyChanged(nameof(BatchProgressCurrent));
            }
        }
        public int BatchProgressMaximum
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
        }

        private void SplitEncoderInfo(string ffmpegResult)
        {
            data.VideoEncoders.Clear();
            data.AudioEncoders.Clear();

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
                Filter = "FFmpeg file (.exe)|ffmpeg.exe"
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
            string arguments = string.Empty;

            switch (CurrentSelectedTab)
            {
                case SelectedTab.FastCutTab:
                    arguments = FFmpegArguments.FastCut(data, !openCmd);
                    break;
                case SelectedTab.EditTab:
                    arguments = FFmpegArguments.Edit(data, !openCmd);
                    break;
                case SelectedTab.ImageTab:
                    arguments = FFmpegArguments.Image(data, !openCmd);
                    break;
                default:
                    break;
            }

            return arguments;
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
                if (SplitVideoInfo(FFmpegResultText) == false)
                {
                    ResultText += "Fail =========\n";
                    ResultText += FFmpegResultText;
                }

                FFmpegResultText = string.Empty;
                GetVideoInfo();
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
                if (info.Encoder.Length == 0)
                {
                    filename = info.FileName;
                    break;
                }
            }

            if (filename.Length == 0)
                return;
            
            Working = true;
            var task = Task.Run(() => FFmpegAsync("\"" + data.FFmpegFile + "\"", "-hide_banner -i \"" + filename + "\"", false, false)).ContinueWith((antecedent) =>
            {
                VideoInfo temp = BatchSplitInfo(FFmpegResultText, filename);
                if (temp.Encoder.Length == 0)
                    temp.Encoder = "Not Found";
                foreach (VideoInfo info in VideoList)
                {
                    if (info.Encoder.Length != 0 || info.FileName.Equals(temp.FileName) == false)
                        continue;

                    info.Encoder = temp.Encoder;
                    info.Resolution = temp.Resolution;
                    info.Duration = temp.Duration;
                    info.FPS = temp.FPS;
                    info.Bitrate = temp.Bitrate;
                    info.SaveName = Path.GetFileNameWithoutExtension(info.FileName) + "_cut" + Path.GetExtension(info.FileName);
                    break;
                }

                FFmpegResultText = string.Empty;
                Working = false;
                BatchGetInfo();
            });
        }

        private VideoInfo BatchSplitInfo(string ffmpegResult, string filename)
        {
            VideoInfo info = new(filename);
            string[]? splitResult = ffmpegResult?.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (splitResult == null || splitResult.Length == 0) return info;

            char[] delimiterChars = { ' ', ',', '.', ':' };
            foreach (string line in splitResult)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.Contains("Stream", StringComparison.Ordinal) && line.Contains("Video", StringComparison.Ordinal) && !line.Contains("attached pic", StringComparison.Ordinal))
                {
                    string[] splitLine = line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (string split in splitLine)
                    {
                        if (split.Contains("Video", StringComparison.Ordinal))
                        {
                            string[] encoder = split.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            for (int i = 0; i < encoder.Length; ++i)
                            {
                                if (encoder[i].Equals("Video:") && encoder.Length >= i + 2)
                                {
                                    info.Encoder = encoder[i + 1];
                                    break;
                                }
                            }
                        }
                        else if (split.Contains("fps", StringComparison.Ordinal))
                        {
                            string[] fps = split.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            if (fps.Length >= 2)
                            {
                                info.FPS = double.Parse(fps[0]);
                            }
                        }
                        else if (split.Contains('x', StringComparison.Ordinal))
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
                            }
                        }
                        else if (split.Contains("bitrate", StringComparison.Ordinal))
                        {
                            string[] bitrate = split.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            if (bitrate.Length >= 2)
                            {
                                info.Bitrate = bitrate[1];
                            }
                        }
                    }
                }
            }

            return info;
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
            for (int i = VideoList.Count -1; i >= 0; --i)
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

        private List<string> inputs = new();
        private List<string> outputs = new();
        private void BatchCut(bool IsAll)
        {
            inputs.Clear();
            outputs.Clear();

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
            }

            BatchProgressCurrent = 0;
            BatchProgressMaximum = inputs.Count;

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
                Working = false;
                stopwatch.Stop();
                ResultText += (FFmpegResultText.Length == 0) ? "========= Success! =========\n" : FFmpegResultText;
                ResultText += $"====== Working End : {DateTime.Now.ToString("F")} ({stopwatch.ElapsedMilliseconds / 1000L} Seconds)\n";
                data.LoadVideo = string.Empty;
                data.SaveVideo = string.Empty;
                FFmpegResultText = string.Empty;
                ++BatchProgressCurrent;
                RecursiveBatchCut();
            });
        }

        private void BatchReplaceNameExecutedCommand(object? obj)
        {
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
    }
}