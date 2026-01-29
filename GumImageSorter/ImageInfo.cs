using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace GumImageSorter
{
    public class ImageCollection : ObservableCollection<ImageInfo>
    {
    }

    public class ImageInfo : INotifyPropertyChanged
    {
        private string fileName = string.Empty;
        private string saveName = string.Empty;
        private string format = string.Empty;
        private int width = 0;
        private int height = 0;
        private double fps;
        private string pixel = string.Empty;
        private long fileSize;
        private bool isSelected = false;
        public BitmapImage? Image = null;

        public ImageInfo(string file_name)
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
        public string Format
        {
            get => format; set
            {
                format = value;
                OnPropertyChanged(nameof(Format));
            }
        }
        public int Width
        {
            get => width; set
            {
                width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
        public int Height
        {
            get => height; set
            {
                height = value;
                OnPropertyChanged(nameof(Height));
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
        public long FileSize
        {
            get => fileSize; set
            {
                fileSize = value;
                OnPropertyChanged(nameof(FileSize));
                OnPropertyChanged(nameof(FileSizeString));
            }
        }
        public string FileSizeString
        {
            get
            {
                if (fileSize < 1024)
                    return $"{fileSize} B";
                else if (fileSize < 1048576)
                    return $"{(fileSize / 1024.0):F2} KB";
                else if (fileSize < 1073741824)
                    return $"{(fileSize / 1048576.0):F2} MB";
                else
                    return $"{(fileSize / 1073741824.0):F2} GB";
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
    }
}
