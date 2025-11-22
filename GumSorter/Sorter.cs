using GumTool;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace GumSorter
{
    internal class Sorter : INotifyPropertyChanged
    {
        private string DefaultThumbnailImage = "\\ini\\GumSorter.png";

        private InputData data = new();
        private VideoCollection videoList = [];
        private VideoCollection deleteList = [];
        private int selectedVideoIndex = 0;
        private bool working;
        private long currentWorked = 0;
        private string tempDirectory = "temp";
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
        public Command ApplyChangeSelectedButton { get; set; }
        public Command ApplyChangeAllButton { get; set; }

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
        public long CurrentWorked
        {
            get => currentWorked; set
            {
                currentWorked = value;
                OnPropertyChanged(nameof(CurrentWorked));
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
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Sorter()
        {
            FFmpegOpenButton = new(FFmpegOpenExecutedCommand, EmptyCanExecuteCommand);
            TempDirectoryButton = new(TempDirectoryExecutedCommand, EmptyCanExecuteCommand);
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
            ApplyChangeSelectedButton = new(ApplyChangeSelectedExecutedCommand, EmptyCanExecuteCommand);
            ApplyChangeAllButton = new(ApplyChangeAllExecutedCommand, EmptyCanExecuteCommand);

            SetupfileLoad();
            IniReplaceLoad();
            data.ImageFormat = 2; // JPG

            DefaultThumbnailImage = Path.GetDirectoryName(Environment.ProcessPath) + DefaultThumbnailImage;
            thumbnailImage = DefaultThumbnailImage;
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
                        Original = split[0],
                        Replace = split[1]
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
                sw.WriteLine($"{replace.Original}/{replace.Replace}");
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

        private void VideoListRemoveSelectedExecutedCommand(object? obj)
        {
            if (selectedVideoIndex < 0 || selectedVideoIndex >= VideoList.Count) return;

            VideoList.RemoveAt(selectedVideoIndex);
        }

        private void VideoListRemoveAllExecutedCommand(object? obj)
        {
            VideoList.Clear();
        }

        private void VideoListDeleteSelectedExecutedCommand(object? obj)
        {
            if (selectedVideoIndex < 0 || selectedVideoIndex >= VideoList.Count) return;

            AddDeleteList(VideoList[selectedVideoIndex]);
            VideoList.RemoveAt(selectedVideoIndex);
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

            foreach (VideoInfo info in DeleteList)
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

        private void DeleteListDeleteSelectedExecutedCommand(object? obj)
        {
            for (int i = DeleteList.Count - 1; i >= 0; --i)
            {
                if (DeleteList[i].IsSelected == false || File.Exists(DeleteList[i].FileName) == false)
                    continue;
                
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete this file?\n{DeleteList[i].FileName}", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                    continue;

                try
                {
                    File.Delete(DeleteList[i].FileName);
                }
                catch (IOException)
                {
                }
                DeleteList.RemoveAt(i);
            }
        }

        private void DeleteListDeleteAllExecutedCommand(object? obj)
        {
            MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete all file?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
                return;

            foreach (VideoInfo info in DeleteList)
            {
                if (File.Exists(info.FileName))
                {
                    try
                    {
                        File.Delete(info.FileName);
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            DeleteList.Clear();
        }

        private void ReplaceListDeleteExecutedCommand(object? obj)
        {
            for (int i = ReplaceList.Count - 1; i >= 0; --i)
            {
                if (ReplaceList[i].Original.Equals(replaceName.Original, StringComparison.OrdinalIgnoreCase)
                    && ReplaceList[i].Replace.Equals(replaceName.Replace, StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceList.RemoveAt(i);
                }
            }
            ReplaceName.Original = string.Empty;
            ReplaceName.Replace = string.Empty;
            IniReplaceSave();
        }

        private void ReplaceNameExecutedCommand(object? obj)
        {
            if (replaceName.Original.Length == 0)
                return;

            foreach (VideoInfo info in VideoList)
            {
                info.SaveName = info.SaveName.Replace(replaceName.Original, replaceName.Replace);
            }

            for (int i = ReplaceList.Count - 1; i >= 0; --i)
            {
                if (ReplaceList[i].Original.Equals(replaceName.Original, StringComparison.OrdinalIgnoreCase)
                    && ReplaceList[i].Replace.Equals(replaceName.Replace, StringComparison.OrdinalIgnoreCase))
                {
                    ReplaceList.RemoveAt(i);
                }
            }
            ReplaceList.Insert(0, new ReplaceInfo()
            {
                Original = replaceName.Original,
                Replace = replaceName.Replace
            });
            IniReplaceSave();
        }

        private void ToDirectoryNameSelectedExecutedCommand(object? obj)
        {
            if (selectedVideoIndex < 0 || selectedVideoIndex >= VideoList.Count) return;

            string directoryName = Path.GetDirectoryName(VideoList[selectedVideoIndex].FileName) ?? string.Empty;
            VideoList[selectedVideoIndex].SaveName = directoryName.Length > 0 ? Path.GetFileName(directoryName) + Path.GetExtension(VideoList[selectedVideoIndex].FileName) : VideoList[selectedVideoIndex].SaveName;
        }
        private void ToDirectoryNameAllExecutedCommand(object? obj)
        {
            foreach (VideoInfo info in VideoList)
            {
                string directoryName = Path.GetDirectoryName(info.FileName) ?? string.Empty;
                info.SaveName = directoryName.Length > 0 ? Path.GetFileName(directoryName) + Path.GetExtension(info.FileName) : info.SaveName;
            }
        }

        private void ApplyChangeSelectedExecutedCommand(object? obj)
        {
            if (selectedVideoIndex < 0 || selectedVideoIndex >= VideoList.Count) return;

            string destFilename = Path.GetDirectoryName(VideoList[selectedVideoIndex].FileName) + "\\" + VideoList[selectedVideoIndex].SaveName;
            File.Move(VideoList[selectedVideoIndex].FileName, destFilename);
            if (File.Exists(destFilename))
            {
                VideoList[selectedVideoIndex].FileName = destFilename;
            }
        }
        private void ApplyChangeAllExecutedCommand(object? obj)
        {
            foreach (VideoInfo info in VideoList)
            {
                string destFilename = Path.GetDirectoryName(info.FileName) + "\\" + info.SaveName;
                File.Move(info.FileName, destFilename);
                if (File.Exists(destFilename))
                {
                    info.FileName = destFilename;
                }
            }
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

            if (VideoList.Any(v => v.FileName.Equals(file, StringComparison.OrdinalIgnoreCase))
                || DeleteList.Any(v => v.FileName.Equals(file, StringComparison.OrdinalIgnoreCase)))
                return;

            VideoInfo info = new(file);
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

            if (current > CurrentWorked || videoList.Count < currentWorked)
            {
                CurrentWorked = current;
            }

            if (filename.Length == 0)
            {
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
                        info.SaveName = Path.GetFileNameWithoutExtension(info.FileName) + Path.GetExtension(info.FileName);
                        info.Streams = temp.Streams;
                        info.Subtitles = temp.Subtitles;
                        break;
                    }

                    FFmpegResultText = string.Empty;
                    ThumbnailCreate(temp);
                });
            });
        }

        internal void ThumbnailCreate(VideoInfo temp)
        {
            if (temp == null || temp.FileName.Length == 0)
            {
                foreach (VideoInfo info in VideoList)
                {
                    if (info.Thumbnails.Count >= thumbnailCount || info.Duration.Length == 0)
                        continue;

                    temp = info;
                    break;
                }
                if (temp == null || temp.FileName.Length == 0)
                {
                    Working = false;
                    VideoListGetInfo();
                    return;
                }
            }

            string tempDir = string.Empty;
            tempDir = $"{tempDirectory}\\{Path.GetFileNameWithoutExtension(temp.FileName)}";
            if (Directory.Exists(tempDir))
            {
                createdthumbnailDirectorys.Add(tempDir);


                DirectoryInfo dirInfo = new(tempDir);
                foreach (FileInfo fileInfo in dirInfo.GetFiles())
                    temp.Thumbnails.Add(fileInfo.FullName);
                
                if (temp.Thumbnails.Count >= thumbnailCount)
                {
                    foreach (VideoInfo info in VideoList)
                    {
                        if (info.FileName.Equals(temp.FileName) == false)
                            continue;

                        foreach (string thumb in temp.Thumbnails)
                            info.Thumbnails.Add(thumb);
                    }
                    if (thumbnailImage == DefaultThumbnailImage && VideoList.Count > 0 && VideoList[SelectedVideoIndex].FileName.Equals(temp.FileName))
                    {
                        ThumbnailImages = VideoList[SelectedVideoIndex].Thumbnails;
                        NextThumbnailImage();
                    }
                    CurrentWorked++;
                    Working = false;
                    VideoListGetInfo();
                    return;
                }
            }
            else
            {
                Directory.CreateDirectory(tempDir);
                createdthumbnailDirectorys.Add(tempDir);
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

            data.LoadVideo = temp.FileName;
            data.SaveVideo = $"{tempDir}\\{Path.GetFileNameWithoutExtension(temp.FileName)}_cut{Path.GetExtension(temp.FileName)}";

            var task = Task.Run(() => FFmpegAsync("\"" + data.FFmpegFile + "\"", FFmpegArguments.Image(data, true), false, false)).ContinueWith((antecedent) =>
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {

                    foreach (VideoInfo info in VideoList)
                    {
                        if (info.FileName.Equals(data.LoadVideo) == false)
                            continue;

                        DirectoryInfo dirInfo = new(tempDir);
                        foreach (FileInfo fileInfo in dirInfo.GetFiles())
                            info.Thumbnails.Add(fileInfo.FullName);

                        if (info.Thumbnails.Count == 0)
                            info.Thumbnails.Add(DefaultThumbnailImage);
                    }

                    if (thumbnailImage == DefaultThumbnailImage && VideoList.Count > 0 && VideoList[SelectedVideoIndex].FileName.Equals(data.LoadVideo))
                    {
                        ThumbnailImages = VideoList[SelectedVideoIndex].Thumbnails;
                        NextThumbnailImage();
                    }

                    CurrentWorked++;
                    data.LoadVideo = string.Empty;
                    data.SaveVideo = string.Empty;
                    FFmpegResultText = string.Empty;
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
