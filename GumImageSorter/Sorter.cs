using GumTool;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GumImageSorter
{
    internal class Sorter : INotifyPropertyChanged
    {
        private ImageCollection imageList = [];
        private ImageCollection deleteList = [];
        private int selectedImageIndex = 0;
        private bool working;
        private long progressCurrent = 0;
        private long progressMaximum = 0;
        private BitmapImage defaultThumbnailImage = new();
        private ImageSource transparencyImage = new BitmapImage();
        private ImageSource thumbnailImage = new BitmapImage();
        private ReplaceCollection replaceList = [];
        private ReplaceInfo replaceName = new();

        public Command ImageListOpenFileButton { get; set; }
        public Command ImageListOpenDirectoryButton { get; set; }
        public Command ImageListMoveSelectedButton { get; set; }
        public Command ImageListMoveAllButton { get; set; }
        public Command ImageListRemoveSelectedButton { get; set; }
        public Command ImageListRemoveAllButton { get; set; }
        public Command ImageListDeleteSelectedButton { get; set; }
        public Command ImageListDeleteAllButton { get; set; }
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

        public ImageCollection ImageList
        {
            get => imageList; set
            {
                imageList = value;
                OnPropertyChanged(nameof(ImageList));
            }
        }
        public ImageCollection DeleteList
        {
            get => deleteList; set
            {
                deleteList = value;
                OnPropertyChanged(nameof(DeleteList));
            }
        }
        public int SelectedImageIndex
        {
            get => selectedImageIndex; set
            {
                selectedImageIndex = value;
                OnPropertyChanged(nameof(SelectedImageIndex));
                OnPropertyChanged(nameof(SelectedImageIndexCount));
            }
        }
        public int SelectedImageIndexCount
        {
            get => selectedImageIndex + 1;
        }
        public bool Working
        {
            get => working; set
            {
                working = value;
                OnPropertyChanged(nameof(Working));
            }
        }
        public long ProgressCurrent
        {
            get => progressCurrent; set
            {
                progressCurrent = value;
                OnPropertyChanged(nameof(ProgressCurrent));
            }
        }
        public long ProgressMaximum
        {
            get => progressMaximum; set
            {
                progressMaximum = value;
                OnPropertyChanged(nameof(ProgressMaximum));
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
        public ImageSource TransparencyImage
        {
            get => transparencyImage; set
            {
                transparencyImage = value;
                OnPropertyChanged(nameof(TransparencyImage));
            }
        }
        public ImageSource ThumbnailImage
        {
            get => thumbnailImage; set
            {
                thumbnailImage = value;
                OnPropertyChanged(nameof(ThumbnailImage));
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Sorter()
        {
            ImageListOpenFileButton = new(ImageListOpenFileExecutedCommand, EmptyCanExecuteCommand);
            ImageListOpenDirectoryButton = new(ImageListOpenDirectoryExecutedCommand, EmptyCanExecuteCommand);
            ImageListMoveSelectedButton = new(ImageListMoveSelectedExecutedCommand, WorkingCanExecuteCommand);
            ImageListMoveAllButton = new(ImageListMoveAllExecutedCommand, WorkingCanExecuteCommand);
            ImageListRemoveSelectedButton = new(ImageListRemoveSelectedExecutedCommand, EmptyCanExecuteCommand);
            ImageListRemoveAllButton = new(ImageListRemoveAllExecutedCommand, EmptyCanExecuteCommand);
            ImageListDeleteSelectedButton = new(ImageListDeleteSelectedExecutedCommand, EmptyCanExecuteCommand);
            ImageListDeleteAllButton = new(ImageListDeleteAllExecutedCommand, EmptyCanExecuteCommand);
            DeleteListMoveSelectedButton = new(DeleteListMoveSelectedExecutedCommand, WorkingCanExecuteCommand);
            DeleteListMoveAllButton = new(DeleteListMoveAllExecutedCommand, WorkingCanExecuteCommand);
            DeleteListRemoveSelectedButton = new(DeleteListRemoveSelectedExecutedCommand, EmptyCanExecuteCommand);
            DeleteListRemoveAllButton = new(DeleteListRemoveAllExecutedCommand, EmptyCanExecuteCommand);
            DeleteListDeleteSelectedButton = new(DeleteListDeleteSelectedExecutedCommand, WorkingCanExecuteCommand);
            DeleteListDeleteAllButton = new(DeleteListDeleteAllExecutedCommand, WorkingCanExecuteCommand);
            ReplaceListDeleteButton = new(ReplaceListDeleteExecutedCommand, EmptyCanExecuteCommand);
            ReplaceNameButton = new(ReplaceNameExecutedCommand, WorkingCanExecuteCommand);
            ToDirectoryNameSelectedButton = new(ToDirectoryNameSelectedExecutedCommand, EmptyCanExecuteCommand);
            ToDirectoryNameAllButton = new(ToDirectoryNameAllExecutedCommand, EmptyCanExecuteCommand);
            ApplyRenameSelectedButton = new(ApplyRenameSelectedExecutedCommand, WorkingCanExecuteCommand);
            ApplyRenameAllButton = new(ApplyRenameAllExecutedCommand, WorkingCanExecuteCommand);

            IniReplaceLoad();
            LoadBitmapImage();
        }

        private void LoadBitmapImage()
        {
            {
                string path = Path.GetDirectoryName(Environment.ProcessPath) + "\\ini\\GumImageSorter.png";
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                defaultThumbnailImage = bitmap;
                thumbnailImage = defaultThumbnailImage;
            }

            {
                string path = Path.GetDirectoryName(Environment.ProcessPath) + "\\ini\\Transparency.png";
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                TransparencyImage = bitmap;
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
                
        private void ImageListOpenFileExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            AddImageList(dialog.FileNames);
            ImageListGetInfo();
        }

        private void ImageListOpenDirectoryExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            AddImageList(dialog.FolderNames);
            ImageListGetInfo();
        }

        private void ImageListMoveSelectedExecutedCommand(object? obj) => ImageListMove(false);
        private void ImageListMoveAllExecutedCommand(object? obj) => ImageListMove(true);

        private void ImageListMove(bool IsAll)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            ProgressCurrent = 0;
            ProgressMaximum = ImageList.Count;
            Working = true;

            var task = Task.Run(() =>
            {
                foreach (ImageInfo info in ImageList)
                {
                    if (info.IsSelected == false && IsAll == false)
                    {
                        progressCurrent++;
                        continue;
                    }

                    string destFilename = dialog.FolderName + "\\" + Path.GetFileNameWithoutExtension(info.FileName) + Path.GetExtension(info.FileName);
                    try
                    {
                        File.Move(info.FileName, destFilename);
                    }
                    catch (IOException)
                    {
                    }
                    if (File.Exists(destFilename))
                    {
                        info.FileName = destFilename;
                    }
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        ProgressCurrent++;
                    });
                }

                App.Current.Dispatcher.Invoke(delegate
                {
                    Working = false;
                });
            });
        }

        private void ImageListRemoveSelectedExecutedCommand(object? obj)
        {
            for (int i = ImageList.Count - 1; i >= 0; --i)
            {
                if (ImageList[i].IsSelected == false)
                    continue;

                ImageList.RemoveAt(i);
            }
        }

        private void ImageListRemoveAllExecutedCommand(object? obj)
        {
            ImageList.Clear();
        }

        private void ImageListDeleteSelectedExecutedCommand(object? obj)
        {
            for (int i = ImageList.Count - 1; i >= 0; --i)
            {
                if (ImageList[i].IsSelected == false)
                    continue;

                AddDeleteList(ImageList[i]);
                ImageList.RemoveAt(i);
            }
        }

        private void ImageListDeleteAllExecutedCommand(object? obj)
        {
            foreach (ImageInfo info in ImageList)
            {
                AddDeleteList(info);
            }
            ImageList.Clear();
        }

        private void DeleteListMoveSelectedExecutedCommand(object? obj) => DeleteListMove(false);
        private void DeleteListMoveAllExecutedCommand(object? obj) => DeleteListMove(true);

        private void DeleteListMove(bool IsAll)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            bool? result = dialog.ShowDialog();

            if (result == null || result != true)
                return;

            ProgressCurrent = 0;
            ProgressMaximum = DeleteList.Count;
            Working = true;

            var task = Task.Run(() =>
            {
                foreach (ImageInfo info in DeleteList)
                {
                    if (info.IsSelected == false && IsAll == false)
                    {
                        progressCurrent++;
                        continue;
                    }

                    string destFilename = dialog.FolderName + "\\" + Path.GetFileNameWithoutExtension(info.FileName) + Path.GetExtension(info.FileName);
                    try
                    {
                        File.Move(info.FileName, destFilename);
                    }
                    catch (IOException)
                    {
                    }
                    if (File.Exists(destFilename))
                    {
                        info.FileName = destFilename;
                    }
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        ProgressCurrent++;
                    });
                }

                App.Current.Dispatcher.Invoke(delegate
                {
                    Working = false;
                });
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

            ProgressCurrent = 0;
            ProgressMaximum = DeleteList.Count;
            Working = true;

            var task = Task.Run(() =>
            {
                for (int i = DeleteList.Count - 1; i >= 0; --i)
                {
                    if (DeleteList[i].IsSelected == false && IsAll == false)
                    {
                        progressCurrent++;
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
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        DeleteList.RemoveAt(i);
                        ProgressCurrent++;
                    });
                }

                App.Current.Dispatcher.Invoke(delegate
                {
                    Working = false;
                });
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

            foreach (ImageInfo info in ImageList)
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
            foreach (ImageInfo info in ImageList)
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
            ProgressCurrent = 0;
            ProgressMaximum = ImageList.Count;
            Working = true;

            var task = Task.Run(() =>
            {
                foreach (ImageInfo info in ImageList)
                {
                    if (info.IsSelected == false && IsAll == false)
                    {
                        progressCurrent++;
                        continue;
                    }

                    if (info.SaveName.Equals(Path.GetFileName(info.FileName), StringComparison.OrdinalIgnoreCase))
                    {
                        progressCurrent++;
                        continue;
                    }

                    string destFilename = Path.GetDirectoryName(info.FileName) + "\\" + info.SaveName;
                    try
                    {
                        File.Move(info.FileName, destFilename);
                    }
                    catch (IOException)
                    {
                    }
                    if (File.Exists(destFilename))
                    {
                        info.FileName = destFilename;
                    }
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        ProgressCurrent++;
                    });
                }

                App.Current.Dispatcher.Invoke(delegate
                {
                    Working = false;
                });
            });
        }

        private bool EmptyCanExecuteCommand(object? obj)
        {
            return true;
        }
        private bool WorkingCanExecuteCommand(object? obj)
        {
            return !Working;
        }

        internal void ImageList_DragAndDrop(string[] files)
        {
            AddImageList(files);

            ImageListGetInfo();
        }

        private void AddImageList(string[] files)
        {
            foreach (string file in files)
            {
                if (Directory.Exists(file))
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(file);
                    foreach (FileInfo info in directoryInfo.GetFiles("*.*", SearchOption.AllDirectories))
                    {
                        AddFileImageList(info);
                    }
                }
                else if (File.Exists(file))
                {
                    AddFileImageList(new FileInfo(file));
                }
            }
        }

        private void AddFileImageList(FileInfo fi)
        {
            if (fi.FullName.Contains("Thumbs.db", StringComparison.OrdinalIgnoreCase))
                return;

            if (ImageList.Any(v => v.FileName.Equals(fi.FullName, StringComparison.OrdinalIgnoreCase))
                || DeleteList.Any(v => v.FileName.Equals(fi.FullName, StringComparison.OrdinalIgnoreCase)))
                return;

            ImageInfo info = new(fi.FullName);
            info.FileSize = fi.Length;
            ImageList.Add(info);
            if (ImageList.Count == 1)
                SelectedImageIndex = 0;
        }

        internal void ImageSelectionChanged(ImageInfo imageInfo)
        {
            if (imageInfo.Format.Length == 0)
            {
                ThumbnailImage = defaultThumbnailImage;
                return;
            }

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(imageInfo.FileName, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            ThumbnailImage = bitmap;
        }
        internal void AddDeleteList(ImageInfo deleted)
        {
            if (deleted == null || DeleteList.Any(v => v.FileName.Equals(deleted.FileName, StringComparison.OrdinalIgnoreCase)))
                return;

            deleted.SaveName = Path.GetFileNameWithoutExtension(deleted.FileName) + Path.GetExtension(deleted.FileName);
            deleted.IsSelected = false;
            DeleteList.Add(deleted);
        }

        private void ImageListGetInfo()
        {
            if (Working == true)
                return;

            ProgressCurrent = 0;
            ProgressMaximum = ImageList.Count;
            Working = true;
            var task = Task.Run(() =>
            {
                foreach (ImageInfo info in ImageList)
                {
                    if (info.SaveName.Length != 0)
                    {
                        progressCurrent++;
                        continue;
                    }
                    info.SaveName = Path.GetFileNameWithoutExtension(info.FileName) + Path.GetExtension(info.FileName);

                    try
                    {
                        using (System.IO.Stream fs = File.OpenRead(info.FileName))
                        {
                            BitmapDecoder? decoder = BitmapDecoder.Create(fs, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
                            if (decoder != null && decoder.Frames.Count != 0)
                            {
                                info.Width = decoder.Frames[0].PixelWidth;
                                info.Height = decoder.Frames[0].PixelHeight;
                                info.Format = decoder.CodecInfo.FriendlyName.Replace(" Decoder", string.Empty);
                                info.Pixel = decoder.Frames[0].Format.ToString();
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                    App.Current.Dispatcher.Invoke(delegate
                    {
                        ProgressCurrent++;
                        if (thumbnailImage == defaultThumbnailImage && ImageList.Count > 0 && ImageList[selectedImageIndex].FileName.Equals(info.FileName) && info.Format.Length != 0)
                        {
                            ImageSelectionChanged(info);
                        }
                    });
                }

                App.Current.Dispatcher.Invoke(delegate
                {
                    Working = false;
                });
            });
        }
    }
}
