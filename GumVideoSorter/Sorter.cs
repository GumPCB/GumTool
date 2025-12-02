using GumTool;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace GumVideoSorter
{
    internal class Sorter : INotifyPropertyChanged
    {
        private string DefaultThumbnailImage = "\\ini\\GumSorter.png";
        private string transparencyImage = "\\ini\\Transparency.png";

        private InputData data = new();
        private VideoCollection videoList = [];
        private VideoCollection deleteList = [];
        private int selectedVideoIndex = 0;
        private bool working;
        private bool openCmd;
        private long currentWorked = 0;
        private long currentWorkedDeleteList = 0;
        private long encodeProgressCurrent = 0;
        private long encodeProgressMaximum = 0;
        private string tempDirectory = "temp";
        private string saveDirectory = "save";
        private string encodeCommand = "-map 0 -qp 20 -movflags +faststart -c:v hevc_nvenc -c:a copy";
        private string resultText = string.Empty;
        private uint thumbnailCount = 10;
        private string thumbnailImage;
        private List<string> thumbnailImages = [];
        private int currentThumbnailIndex = 1;
        private bool isClosingDeleteThumbnail = false;
        private bool isVisibleThumbnailName = false;
        private List<string> createdthumbnailDirectorys = [];
        private ReplaceCollection replaceList = [];
        private ReplaceInfo replaceName = new();

        public Command FFmpegOpenButton { get; set; }
        public Command TempDirectoryButton { get; set; }
        public Command SaveDirectoryButton { get; set; }
        public Command VideoListOpenFileButton { get; set; }
        public Command VideoListOpenDirectoryButton { get; set; }
        public Command VideoListMoveSelectedButton { get; set; }
        public Command VideoListMoveAllButton { get; set; }
        public Command VideoListRemoveSelectedButton { get; set; }
        public Command VideoListRemoveAllButton { get; set; }
        public Command VideoListDeleteSelectedButton { get; set; }
        public Command VideoListDeleteAllButton { get; set; }
        public Command DeleteListMoveSelectedButton { get; set; }
        public Command DeleteListMoveAllButton { get; set; }
        public Command DeleteListRemoveSelectedButton { get; set; }
        public Command DeleteListRemoveAllButton { get; set; }
        public Command DeleteListDeleteSelectedButton { get; set; }
        public Command DeleteListDeleteAllButton { get; set; }
        public Command ReplaceListDeleteButton { get; set; }
        public Command ReplaceNameButton { get; set; }
        public Command ToDirectoryNameSelectedButton { get; set; }
        public Command ToDirectoryNameAllButton { get; set; }
        public Command ApplyRenameSelectedButton { get; set; }
        public Command ApplyRenameAllButton { get; set; }
        public Command EncodeSelectedButton { get; set; }
        public Command EncodeAllButton { get; set; }

        public InputData Data
        {
            get => data; set
            {
                data = value;
                OnPropertyChanged(nameof(Data));
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
        public VideoCollection DeleteList
        {
            get => deleteList; set
            {
                deleteList = value;
                OnPropertyChanged(nameof(DeleteList));
            }
        }
        public int SelectedVideoIndex
        {
            get => selectedVideoIndex; set
            {
                selectedVideoIndex = value;
                OnPropertyChanged(nameof(SelectedVideoIndex));
                OnPropertyChanged(nameof(SelectedVideoIndexCount));
            }
        }
        public int SelectedVideoIndexCount
        {
            get => selectedVideoIndex + 1;
        }
        public bool Working
        {
            get => working; set
            {
                working = value;
                OnPropertyChanged(nameof(Working));
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
        public long CurrentWorked
        {
            get => currentWorked; set
            {
                currentWorked = value;
                OnPropertyChanged(nameof(CurrentWorked));
            }
        }
        public long CurrentWorkedDeleteList
        {
            get => currentWorkedDeleteList; set
            {
                currentWorkedDeleteList = value;
                OnPropertyChanged(nameof(CurrentWorkedDeleteList));
            }
        }
        public long EncodeProgressCurrent
        {
            get => encodeProgressCurrent; set
            {
                encodeProgressCurrent = value;
                OnPropertyChanged(nameof(EncodeProgressCurrent));
            }
        }
        public long EncodeProgressMaximum
        {
            get => encodeProgressMaximum; set
            {
                encodeProgressMaximum = value;
                OnPropertyChanged(nameof(EncodeProgressMaximum));
            }
        }
        public string TempDirectory
        {
            get => tempDirectory; set
            {
                tempDirectory = value;
                OnPropertyChanged(nameof(TempDirectory));
                SetupfileSave();
            }
        }
        public string SaveDirectory
        {
            get => saveDirectory; set
            {
                saveDirectory = value;
                OnPropertyChanged(nameof(SaveDirectory));
                SetupfileSave();
            }
        }
        public string EncodeCommand
        {
            get => encodeCommand; set
            {
                encodeCommand = value;
                OnPropertyChanged(nameof(EncodeCommand));
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
        public uint ThumbnailCount
        {
            get => thumbnailCount; set
            {
                thumbnailCount = value;
                OnPropertyChanged(nameof(ThumbnailCount));
                SetupfileSave();
            }
        }
        public string ThumbnailImage
        {
            get => thumbnailImage; set
            {
                thumbnailImage = value;
                OnPropertyChanged(nameof(ThumbnailImage));
            }
        }
        public List<string> ThumbnailImages
        {
            get => thumbnailImages; set
            {
                thumbnailImages = value;
                OnPropertyChanged(nameof(ThumbnailImages));
            }
        }
        public int CurrentThumbnailIndex
        {
            get => currentThumbnailIndex; set
            {
                currentThumbnailIndex = value;
                OnPropertyChanged(nameof(CurrentThumbnailIndex));
            }
        }
        public bool IsClosingDeleteThumbnail
        {
            get => isClosingDeleteThumbnail; set
            {
                isClosingDeleteThumbnail = value;
                OnPropertyChanged(nameof(IsClosingDeleteThumbnail));
                SetupfileSave();
            }
        }
        public bool IsVisibleThumbnailName
        {
            get => isVisibleThumbnailName; set
            {
                isVisibleThumbnailName = value;
                OnPropertyChanged(nameof(IsVisibleThumbnailName));
                SetupfileSave();
            }
        }
        public ReplaceCollection ReplaceList
        {
            get => replaceList; set
            {
                replaceList = value;
                OnPropertyChanged(nameof(ReplaceList));
            }
        }
        public ReplaceInfo ReplaceName
        {
            get => replaceName; set
            {
                replaceName = value;
                OnPropertyChanged(nameof(ReplaceName));
            }
        }
        public string TransparencyImage
        {
            get => transparencyImage; set
            {
                transparencyImage = value;
                OnPropertyChanged(nameof(TransparencyImage));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Sorter()
        {
            FFmpegOpenButton = new(FFmpegOpenExecutedCommand, EmptyCanExecuteCommand);
            TempDirectoryButton = new(TempDirectoryExecutedCommand, EmptyCanExecuteCommand);
            SaveDirectoryButton = new(SaveDirectoryExecutedCommand, EmptyCanExecuteCommand);
            VideoListOpenFileButton = new(VideoListOpenFileExecutedCommand, EmptyCanExecuteCommand);
            VideoListOpenDirectoryButton = new(VideoListOpenDirectoryExecutedCommand, EmptyCanExecuteCommand);
            VideoListMoveSelectedButton = new(VideoListMoveSelectedExecutedCommand, EmptyCanExecuteCommand);
            VideoListMoveAllButton = new(VideoListMoveAllExecutedCommand, EmptyCanExecuteCommand);
            VideoListRemoveSelectedButton = new(VideoListRemoveSelectedExecutedCommand, EmptyCanExecuteCommand);
            VideoListRemoveAllButton = new(VideoListRemoveAllExecutedCommand, EmptyCanExecuteCommand);
            VideoListDeleteSelectedButton = new(VideoListDeleteSelectedExecutedCommand, EmptyCanExecuteCommand);
            VideoListDeleteAllButton = new(VideoListDeleteAllExecutedCommand, EmptyCanExecuteCommand);
            DeleteListMoveSelectedButton = new(DeleteListMoveSelectedExecutedCommand, EmptyCanExecuteCommand);
            DeleteListMoveAllButton = new(DeleteListMoveAllExecutedCommand, EmptyCanExecuteCommand);
            DeleteListRemoveSelectedButton = new(DeleteListRemoveSelectedExecutedCommand, EmptyCanExecuteCommand);
            DeleteListRemoveAllButton = new(DeleteListRemoveAllExecutedCommand, EmptyCanExecuteCommand);
            DeleteListDeleteSelectedButton = new(DeleteListDeleteSelectedExecutedCommand, EmptyCanExecuteCommand);
            DeleteListDeleteAllButton = new(DeleteListDeleteAllExecutedCommand, EmptyCanExecuteCommand);
            ReplaceListDeleteButton = new(ReplaceListDeleteExecutedCommand, EmptyCanExecuteCommand);
            ReplaceNameButton = new(ReplaceNameExecutedCommand, EmptyCanExecuteCommand);
            ToDirectoryNameSelectedButton = new(ToDirectoryNameSelectedExecutedCommand, EmptyCanExecuteCommand);
            ToDirectoryNameAllButton = new(ToDirectoryNameAllExecutedCommand, EmptyCanExecuteCommand);
            ApplyRenameSelectedButton = new(ApplyRenameSelectedExecutedCommand, EmptyCanExecuteCommand);
            ApplyRenameAllButton = new(ApplyRenameAllExecutedCommand, EmptyCanExecuteCommand);
            EncodeSelectedButton = new(EncodeSelectedExecutedCommand, EmptyCanExecuteCommand);
            EncodeAllButton = new(EncodeAllExecutedCommand, EmptyCanExecuteCommand);

            SetupfileLoad();
            IniReplaceLoad();
            data.ImageFormat = 2; // JPG

            DefaultThumbnailImage = Path.GetDirectoryName(Environment.ProcessPath) + DefaultThumbnailImage;
            thumbnailImage = DefaultThumbnailImage;
            TransparencyImage = Path.GetDirectoryName(Environment.ProcessPath) + transparencyImage;
        }

        private const string SetupFile = "ini\\SorterSetup.ini";
        private void SetupfileLoad()
        {
            if (!File.Exists(SetupFile))
                return;

            using StreamReader sr = new(SetupFile);
            string? ffmpeg = sr.ReadLine();
            string? tempDirectory = sr.ReadLine();
            string? thumbnail = sr.ReadLine();
            string? isDelete = sr.ReadLine();
            string? isVisible = sr.ReadLine();
            string? saveDirectory = sr.ReadLine();
            string? encodeCommand = sr.ReadLine();
            string? opencmd = sr.ReadLine();
            sr.Close();

            if (ffmpeg != null)
            {
                data.FFmpegFile = ffmpeg;
            }

            if (tempDirectory != null)
            {
                TempDirectory = tempDirectory;
            }

            if (thumbnail != null && uint.TryParse(thumbnail, out uint thumb))
            {
                ThumbnailCount = thumb;
            }

            if (isDelete != null)
            {
                IsClosingDeleteThumbnail = isDelete.Equals("1");
            }

            if (isVisible != null)
            {
                IsVisibleThumbnailName = isVisible.Equals("1");
            }

            if (saveDirectory != null)
            {
                SaveDirectory = saveDirectory;
            }

            if (encodeCommand != null)
            {
                EncodeCommand = encodeCommand;
            }

            if (opencmd != null)
            {
                OpenCmd = opencmd.Equals("1");
            }
        }
        private const string ReplaceFile = "ini\\replace.ini";
        private void IniReplaceLoad()
        {
            if (File.Exists(ReplaceFile))
            {
                using StreamReader sr = new(ReplaceFile);
                string? line;
                while (sr.EndOfStream == false)
                {
                    line = sr.ReadLine();
                    if (line == null)
                            continue;

                    string[] split = line.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (split.Length != 2)
                        continue;

                    ReplaceList.Add(new ReplaceInfo()
                    {
                        Before = split[0],
                        After = split[1]
                    });
                }
                sr.Close();
            }
        }

        private void IniReplaceSave()
        {
            FileStream fs = new(ReplaceFile, FileMode.Create);
            using StreamWriter sw = new(fs);
            foreach (ReplaceInfo replace in ReplaceList)
            {
                sw.WriteLine($"{replace.Before}/{replace.After}");
            }
            sw.Close();
        }

        private void SetupfileSave()
        {
            FileStream fs = new(SetupFile, FileMode.Create);
            using StreamWriter sw = new(fs);
            sw.WriteLine(data.FFmpegFile);
            sw.WriteLine(TempDirectory);
            sw.WriteLine(ThumbnailCount.ToString());
            sw.WriteLine(IsClosingDeleteThumbnail ? '1' : '0');
            sw.WriteLine(IsVisibleThumbnailName ? '1' : '0');
            sw.WriteLine(SaveDirectory);
            sw.WriteLine(EncodeCommand);
            sw.WriteLine(OpenCmd ? '1' : '0');
            sw.Close();
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
            VideoListGetInfo();
        }

        private void TempDirectoryExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            TempDirectory = dialog.FolderName;
        }
        private void SaveDirectoryExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            SaveDirectory = dialog.FolderName;
        }
        
        private void VideoListOpenFileExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            AddVideoList(dialog.FileNames);
            VideoListGetInfo();
        }

        private void VideoListOpenDirectoryExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            AddVideoList(dialog.FolderNames);
            VideoListGetInfo();
        }

        private void VideoListMoveSelectedExecutedCommand(object? obj) => VideoListMove(false);
        private void VideoListMoveAllExecutedCommand(object? obj) => VideoListMove(true);

        private void VideoListMove(bool IsAll)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            CurrentWorked = 0;
            var task = Task.Run(() =>
            {
                foreach (VideoInfo info in VideoList)
                {
                    if (info.IsSelected == false && IsAll == false)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            CurrentWorked++;
                        });
                        continue;
                    }

                    string destFilename = dialog.FolderName + "\\" + Path.GetFileNameWithoutExtension(info.FileName) + Path.GetExtension(info.FileName);
                    File.Move(info.FileName, destFilename);
                    if (File.Exists(destFilename))
                    {
                        info.FileName = destFilename;
                    }
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        CurrentWorked++;
                    });
                }
            });
        }

        private void VideoListRemoveSelectedExecutedCommand(object? obj)
        {
            for (int i = VideoList.Count - 1; i >= 0; --i)
            {
                if (VideoList[i].IsSelected == false)
                    continue;

                VideoList.RemoveAt(i);
            }
        }

        private void VideoListRemoveAllExecutedCommand(object? obj)
        {
            VideoList.Clear();
        }

        private void VideoListDeleteSelectedExecutedCommand(object? obj)
        {
            for (int i = VideoList.Count - 1; i >= 0; --i)
            {
                if (VideoList[i].IsSelected == false)
                    continue;

                AddDeleteList(VideoList[i]);
                VideoList.RemoveAt(i);
            }
        }

        private void VideoListDeleteAllExecutedCommand(object? obj)
        {
            foreach (VideoInfo info in VideoList)
            {
                AddDeleteList(info);
            }
            VideoList.Clear();
        }

        private void DeleteListMoveSelectedExecutedCommand(object? obj) => DeleteListMove(false);
        private void DeleteListMoveAllExecutedCommand(object? obj) => DeleteListMove(true);

        private void DeleteListMove(bool IsAll)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            CurrentWorkedDeleteList = 0;
            var task = Task.Run(() =>
            {
                foreach (VideoInfo info in DeleteList)
                {
                    if (info.IsSelected == false && IsAll == false)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            CurrentWorkedDeleteList++;
                        });
                        continue;
                    }

                    string destFilename = dialog.FolderName + "\\" + Path.GetFileNameWithoutExtension(info.FileName) + Path.GetExtension(info.FileName);
                    File.Move(info.FileName, destFilename);
                    if (File.Exists(destFilename))
                    {
                        info.FileName = destFilename;
                    }
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        CurrentWorkedDeleteList++;
                    });
                }
            });
        }

        private void DeleteListRemoveSelectedExecutedCommand(object? obj)
        {
            for (int i = DeleteList.Count - 1; i >= 0; --i)
            {
                if (DeleteList[i].IsSelected)
                {
                    DeleteList.RemoveAt(i);
                }
            }
        }

        private void DeleteListRemoveAllExecutedCommand(object? obj)
        {
            DeleteList.Clear();
        }

        private void DeleteListDeleteSelectedExecutedCommand(object? obj) => DeleteListDeleteFile(false);

        private void DeleteListDeleteAllExecutedCommand(object? obj) => DeleteListDeleteFile(true);
        private void DeleteListDeleteFile(bool IsAll)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete file?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
                return;

            CurrentWorkedDeleteList = 0;
            var task = Task.Run(() =>
            {
                for (int i = DeleteList.Count - 1; i >= 0; --i)
                {
                    if (DeleteList[i].IsSelected == false && IsAll == false)
                    {
                        App.Current.Dispatcher.Invoke((Action)delegate
                        {
                            CurrentWorkedDeleteList++;
                        });
                        continue;
                    }

                    if (File.Exists(DeleteList[i].FileName))
                    {
                        try
                        {
                            File.Delete(DeleteList[i].FileName);
                        }
                        catch (IOException)
                        {
                        }
                    }
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        DeleteList.RemoveAt(i);
                        CurrentWorkedDeleteList++;
                    });
                }
            });
        }

        private void ReplaceListDeleteExecutedCommand(object? obj)
        {
            for (int i = ReplaceList.Count - 1; i >= 0; --i)
            {
                if (ReplaceList[i].Before.Equals(replaceName.Before, StringComparison.OrdinalIgnoreCase)
                    && ReplaceList[i].After.Equals(replaceName.After, StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceList.RemoveAt(i);
                }
            }
            ReplaceName.Before = string.Empty;
            ReplaceName.After = string.Empty;
            IniReplaceSave();
        }

        private void ReplaceNameExecutedCommand(object? obj)
        {
            if (replaceName.Before.Length == 0)
                return;

            foreach (VideoInfo info in VideoList)
            {
                info.SaveName = info.SaveName.Replace(replaceName.Before, replaceName.After);
            }

            for (int i = ReplaceList.Count - 1; i >= 0; --i)
            {
                if (ReplaceList[i].Before.Equals(replaceName.Before, StringComparison.OrdinalIgnoreCase)
                    && ReplaceList[i].After.Equals(replaceName.After, StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceList.RemoveAt(i);
                }
            }
            ReplaceList.Insert(0, new ReplaceInfo()
            {
                Before = replaceName.Before,
                After = replaceName.After
            });
            IniReplaceSave();
        }

        private void ToDirectoryNameSelectedExecutedCommand(object? obj) => ToDirectoryName(false);
        private void ToDirectoryNameAllExecutedCommand(object? obj) => ToDirectoryName(true);
        private void ToDirectoryName(bool IsAll)
        {
            foreach (VideoInfo info in VideoList)
            {
                if (info.IsSelected == false && IsAll == false)
                    continue;

                string directoryName = Path.GetDirectoryName(info.FileName) ?? string.Empty;
                info.SaveName = directoryName.Length > 0 ? Path.GetFileName(directoryName) + Path.GetExtension(info.FileName) : info.SaveName;
            }
        }

        private void ApplyRenameSelectedExecutedCommand(object? obj) => ApplyRename(false);
        private void ApplyRenameAllExecutedCommand(object? obj) => ApplyRename(true);
        private void ApplyRename(bool IsAll)
        {
            foreach (VideoInfo info in VideoList)
            {
                if (info.IsSelected == false && IsAll == false)
                    continue;

                if (info.SaveName.Equals(Path.GetFileName(info.FileName), StringComparison.OrdinalIgnoreCase))
                    continue;

                string destFilename = Path.GetDirectoryName(info.FileName) + "\\" + info.SaveName;
                File.Move(info.FileName, destFilename);
                if (File.Exists(destFilename))
                {
                    info.FileName = destFilename;
                }
            }
        }

        private void EncodeSelectedExecutedCommand(object? obj) => EncodeVideos(false);
        private void EncodeAllExecutedCommand(object? obj) => EncodeVideos(true);
        private readonly List<string> encodeCommands = [];
        private readonly List<string> encodeFileNames = [];
        private readonly List<long> encodeDurations = [];  // milliseconds
        private long encodeCurrent = 0;
        private long encodeMax = 0;
        private void EncodeVideos(bool IsAll)
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
            }

            EncodeProgressCurrent = 0;
            encodeCurrent = 0;
            encodeCommands.Clear();
            encodeFileNames.Clear();
            encodeDurations.Clear();
            long sumDuration = 0L;
            string hideBanner = openCmd ? "" : "-hide_banner -loglevel warning ";
            foreach (VideoInfo info in VideoList)
            {
                if (info.IsSelected == false && IsAll == false)
                    continue;

                encodeCommands.Add($"{hideBanner}-i \"{info.FileName}\" {EncodeCommand.Trim()} -y \"{SaveDirectory}\\{info.SaveName}\"");
                encodeFileNames.Add(info.SaveName);

                long duration = 0L;
                if (TimeSpan.TryParse(info.Duration, out TimeSpan ts))
                    duration = (long)(ts.TotalMilliseconds);

                if (duration == 0L)
                    duration = 1L;

                sumDuration += duration;
                encodeDurations.Add(duration);
            }
            EncodeProgressMaximum = sumDuration;
            encodeMax = encodeCommands.Count;

            RecursiveEncode();
        }
        private static Stopwatch stopwatch = new Stopwatch();
        private void RecursiveEncode()
        {
            if (encodeCommands.Count == 0)
                return;

            string command = encodeCommands[0];
            encodeCommands.RemoveAt(0);
            stopwatch.Restart();
            ResultText += $"= Working Start : {encodeCurrent + 1}/{encodeMax} - {encodeFileNames.First()}\n";
            encodeFileNames.RemoveAt(0);

            var task = Task.Run(() => FFmpegAsync("\"" + data.FFmpegFile + "\"", command, openCmd, false)).ContinueWith((antecedent) =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    stopwatch.Stop();
                    encodeCurrent++;
                    ResultText += (FFmpegResultText.Length == 0) ? "========= Success! =========\n" : FFmpegResultText;
                    ResultText += $"= Working End : {encodeCurrent}/{encodeMax} ({stopwatch.ElapsedMilliseconds / 1000L} Seconds, {(double)encodeDurations.First() / stopwatch.ElapsedMilliseconds:N3}x)\n";
                    EncodeProgressCurrent += encodeDurations.First();
                    encodeDurations.RemoveAt(0);
                    FFmpegResultText = string.Empty;
                    RecursiveEncode();
                });
            });
        }

        private bool EmptyCanExecuteCommand(object? obj)
        {
            return true;
        }

        internal void VideoList_DragAndDrop(string[] files)
        {
            AddVideoList(files);

            VideoListGetInfo();
        }
        internal void DragAndDropTempDirectory(string directory)
        {
            TempDirectory = directory;
        }
        internal void DragAndDropSaveDirectory(string directory)
        {
            SaveDirectory = directory;
        }

        private void AddVideoList(string[] files)
        {
            foreach (string file in files)
            {
                if (Directory.Exists(file))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(file);
                    foreach (FileInfo info in directoryInfo.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        AddFileVideoList(info);
                    }
                }
                else if (File.Exists(file))
                {
                    AddFileVideoList(new FileInfo(file));
                }
            }
        }

        private void AddFileVideoList(FileInfo fi)
        {
            if (fi.FullName.Contains("Thumbs.db", StringComparison.OrdinalIgnoreCase))
                return;

            if (VideoList.Any(v => v.FileName.Equals(fi.FullName, StringComparison.OrdinalIgnoreCase))
                || DeleteList.Any(v => v.FileName.Equals(fi.FullName, StringComparison.OrdinalIgnoreCase)))
                return;

            VideoInfo info = new(fi.FullName);
            info.FileSize = fi.Length;
            VideoList.Add(info);
            if (VideoList.Count == 1)
                SelectedVideoIndex = 0;
        }

        internal void VideoSelectionChanged(VideoInfo videoInfo)
        {
            ThumbnailImages = videoInfo.Thumbnails;
            NextThumbnailImage();
        }
        internal void AddDeleteList(VideoInfo deleted)
        {
            if (deleted == null || DeleteList.Any(v => v.FileName.Equals(deleted.FileName, StringComparison.OrdinalIgnoreCase)))
                return;

            deleted.SaveName = Path.GetFileNameWithoutExtension(deleted.FileName) + Path.GetExtension(deleted.FileName);
            deleted.IsSelected = false;
            DeleteList.Add(deleted);
        }

        private string GetTempDirectory(string filename) => $"{tempDirectory}\\{Path.GetFileNameWithoutExtension(filename)}".TrimEnd();
        private string GetVideoInfoFile(string filename) => $"{GetTempDirectory(filename)}\\VideoInfo.txt";
        private void VideoListGetInfo()
        {
            if (Working == true)
                return;

            long current = 0;
            string filename = string.Empty;
            foreach (VideoInfo info in VideoList)
            {
                current++;
                if (info.vCodec.Length == 0)
                {
                    filename = info.FileName;
                    break;
                }
            }

            if (current > (CurrentWorked + 1) || VideoList.Count < currentWorked)
            {
                CurrentWorked = current - 1;
            }

            if (filename.Length == 0)
            {
                return;
            }

            Working = true;

            if (File.Exists(GetVideoInfoFile(filename)))
            {
                foreach (VideoInfo info in VideoList)
                {
                    if (info.vCodec.Length != 0 || info.FileName.Equals(filename) == false)
                        continue;

                    info.FileLoad(GetVideoInfoFile(filename));
                    ThumbnailCreate(info);
                    break;
                }
                return;
            }

            var task = Task.Run(() => FFmpegAsync("\"" + data.FFmpegFile + "\"", "-hide_banner -i \"" + filename + "\"", false, false)).ContinueWith((antecedent) =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    foreach (VideoInfo info in VideoList)
                    {
                        if (info.vCodec.Length != 0 || info.FileName.Equals(filename) == false)
                            continue;

                        info.SplitInfo(FFmpegResultText);
                        info.SaveName = Path.GetFileNameWithoutExtension(info.FileName) + Path.GetExtension(info.FileName);
                        Directory.CreateDirectory(GetTempDirectory(info.FileName));
                        info.FileSave(GetVideoInfoFile(info.FileName));

                        FFmpegResultText = string.Empty;
                        ThumbnailCreate(info);
                        break;
                    }
                });
            });
        }

        private const string JpegSearchPattern = "*.jpg";
        internal void ThumbnailCreate(VideoInfo temp)
        {
            if (temp == null || temp.FileName.Length == 0)
            {
                CurrentWorked++;
                Working = false;
                VideoListGetInfo();
                return;
            }

            string filename = temp.FileName;
            string tempDir = GetTempDirectory(filename);
            createdthumbnailDirectorys.Add(tempDir);
            if (Directory.Exists(tempDir))
            {
                DirectoryInfo dirInfo = new(tempDir);
                List<string> thumbnails = [];
                foreach (FileInfo fileInfo in dirInfo.GetFiles(JpegSearchPattern))
                    thumbnails.Add(fileInfo.FullName);
                
                if (thumbnails.Count >= thumbnailCount)
                {
                    foreach (VideoInfo info in VideoList)
                    {
                        if (info.FileName.Equals(filename) == false)
                            continue;

                        foreach (string thumb in thumbnails)
                            info.Thumbnails.Add(thumb);

                        break;
                    }
                    if (thumbnailImage == DefaultThumbnailImage && VideoList.Count > 0 && VideoList[selectedVideoIndex].FileName.Equals(filename))
                    {
                        ThumbnailImages = VideoList[selectedVideoIndex].Thumbnails;
                        NextThumbnailImage();
                    }

                    CurrentWorked++;
                    Working = false;
                    VideoListGetInfo();
                    return;
                }
            }

            double totalSeconds = 0.0;
            string[] splitLine = temp.Duration.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (string split in splitLine)
            {
                if (double.TryParse(split, out double value))
                    totalSeconds = totalSeconds * 60.0 + value;
            }
            if (totalSeconds < 0.05)
            {
                temp.Thumbnails.Add(DefaultThumbnailImage);

                CurrentWorked++;
                Working = false;
                VideoListGetInfo();
                return;
            }

            data.Fps = thumbnailCount / totalSeconds;

            data.LoadVideo = filename;
            data.SaveVideo = $"{tempDir}\\{Path.GetFileNameWithoutExtension(filename)}_cut{Path.GetExtension(filename)}";

            Working = true;

            var task = Task.Run(() => FFmpegAsync("\"" + data.FFmpegFile + "\"", FFmpegArguments.Image(data, true), false, false)).ContinueWith((antecedent) =>
            {
                data.LoadVideo = string.Empty;
                data.SaveVideo = string.Empty;
                FFmpegResultText = string.Empty;

                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    foreach (VideoInfo info in VideoList)
                    {
                        if (info.FileName.Equals(filename) == false)
                            continue;

                        DirectoryInfo dirInfo = new(tempDir);
                        foreach (FileInfo fileInfo in dirInfo.GetFiles(JpegSearchPattern))
                            info.Thumbnails.Add(fileInfo.FullName);

                        if (info.Thumbnails.Count == 0)
                            info.Thumbnails.Add(DefaultThumbnailImage);
                    }

                    if (thumbnailImage == DefaultThumbnailImage && VideoList.Count > 0 && VideoList[selectedVideoIndex].FileName.Equals(filename))
                    {
                        ThumbnailImages = VideoList[selectedVideoIndex].Thumbnails;
                        NextThumbnailImage();
                    }

                    CurrentWorked++;
                    Working = false;
                    VideoListGetInfo();
                });
            });
        }

        private static string FFmpegResultText = string.Empty;
        private static async Task FFmpegAsync(string ffmpegFile, string arguments, bool openWindow, bool isStandardOutput)
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

        internal void DeleteThumbnailDirectorys()
        {
            if (IsClosingDeleteThumbnail == false)
                return;

            foreach (string dir in createdthumbnailDirectorys)
            {
                try
                {
                    Directory.Delete(dir, true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to delete directory '{dir}': {ex}");
                }
            }
            createdthumbnailDirectorys.Clear();
        }

        internal void NextThumbnailImage()
        {
            if (thumbnailImages.Count == 0)
            {
                if (ThumbnailImage != DefaultThumbnailImage)
                {
                    ThumbnailImage = DefaultThumbnailImage;
                    CurrentThumbnailIndex = 1;
                }
                return;
            }

            int currentIndex = thumbnailImages.IndexOf(thumbnailImage);
            currentIndex++;
            if (currentIndex >= thumbnailImages.Count)
            {
                currentIndex = 0;
            }
            ThumbnailImage = thumbnailImages[currentIndex];
            CurrentThumbnailIndex = currentIndex + 1;
        }

        internal void PreviousThumbnailImage()
        {
            if (thumbnailImages.Count == 0)
            {
                if (ThumbnailImage != DefaultThumbnailImage)
                {
                    ThumbnailImage = DefaultThumbnailImage;
                    CurrentThumbnailIndex = 1;
                }
                return;
            }

            int currentIndex = thumbnailImages.IndexOf(ThumbnailImage);
            currentIndex--;
            if (currentIndex < 0)
            {
                currentIndex = thumbnailImages.Count - 1;
            }
            ThumbnailImage = thumbnailImages[currentIndex];
            CurrentThumbnailIndex = currentIndex + 1;
        }

        internal void ClearThumbnailImage()
        {
            ThumbnailImages = [];
            ThumbnailImage = DefaultThumbnailImage;
            CurrentThumbnailIndex = 1;
        }
    }
}
