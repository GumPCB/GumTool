using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace GumCut
{
    public class Cut : INotifyPropertyChanged
    {
        private InputData data = new();
        private bool working;
        private string resultText = string.Empty;
        public Command FFmpegOpenButton { get; set; }
        public Command InputVideoButton { get; set; }
        public Command OutputVideoButton { get; set; }
        public Command CutButton { get; set; }
        public Command CmdButton { get; set; }

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
        public string ResultText
        {
            get => resultText; set
            {
                resultText = value;
                OnPropertyChanged(nameof(ResultText));
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
            InputVideoButton = new(InputVideoExecutedCommand, EmptyCanExecuteCommand);
            OutputVideoButton = new(OutputVideoExecutedCommand, OutputVideoCanExecuteCommand);
            CutButton = new(CutExecutedCommand, CutCanExecuteCommand);
            CmdButton = new(CmdExecutedCommand, EmptyCanExecuteCommand);

            SetupfileLoad();
        }

        private const string SetupfileName = "GumFFmpegCut.ini";

        private void SetupfileLoad()
        {
            if (!File.Exists(SetupfileName))
                return;

            using StreamReader sr = new(SetupfileName);
            string? line = sr.ReadLine();
            if (line == null)
                return;

            data.FFmpegFile = line;
        }

        private void SetupfileSave()
        {
            FileStream fs = new(SetupfileName, FileMode.Create);
            using StreamWriter sw = new(fs);
            sw.WriteLine(data.FFmpegFile);
            sw.Close();
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

            if (result != true)
                return;

            data.FFmpegFile = dialog.FileName;

            SetupfileSave();
        }

        private void InputVideoExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            bool? result = dialog.ShowDialog();

            if (result != true)
                return;

            string filename = dialog.FileName;
            data.InputVideo = filename;

            string outfilename = $"{Path.GetDirectoryName(filename)}\\{Path.GetFileNameWithoutExtension(filename)}_cut{Path.GetExtension(filename)}";
            data.OutputVideo = outfilename;
        }

        private void OutputVideoExecutedCommand(object? obj)
        {
            string dotExrension = $"{Path.GetExtension(data.InputVideo)}";
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = Path.GetFileNameWithoutExtension(data.OutputVideo),
                DefaultExt = dotExrension,
                Filter = $"({dotExrension})|*{dotExrension}"
            };

            bool? result = dialog.ShowDialog();

            if (result != true)
                return;

            data.OutputVideo = dialog.FileName;
        }

        private bool OutputVideoCanExecuteCommand(object? obj)
        {
            if (data.InputVideo.Length == 0) return false;
            return true;
        }

        private static string FFmpegResultText = string.Empty;
        private static Stopwatch stopwatch = new Stopwatch();

        private void CutExecutedCommand(object? obj)
        {
            if (Working == true)
            {
                ResultText += "Working...\n";
                return;
            }

            Working = true;
            stopwatch.Restart();
            ResultText = $"====== Working Start : {DateTime.Now.ToString("F")}\n";

            string ffmpegFile = new($"\"{data.FFmpegFile}\"");
            string arguments = FFmpegArguments.FastCut(data);

            var task = Task.Run(() => FFmpegAsync(ffmpegFile, arguments)).ContinueWith((antecedent) => 
            {
                Working = false;
                stopwatch.Stop();
                ResultText += (FFmpegResultText.Length > 0) ? FFmpegResultText : "========= Success! =========\n";
                ResultText += $"====== Working End : {DateTime.Now.ToString("F")} ({stopwatch.ElapsedMilliseconds / 1000L} Seconds)\n";
                FFmpegResultText = string.Empty;
            });
        }

        public static async Task FFmpegAsync(string ffmpegFile, string arguments)
        {
            ProcessStartInfo cmd = new(ffmpegFile, arguments);

            cmd.CreateNoWindow = true;
            cmd.UseShellExecute = false;
            cmd.RedirectStandardOutput = false;
            cmd.RedirectStandardInput = false;
            cmd.RedirectStandardError = true;

            Process process = new();
            process.EnableRaisingEvents = false;
            process.StartInfo = cmd;
            process.Start();

            FFmpegResultText += await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();
            process.Close();
        }

        private bool CutCanExecuteCommand(object? obj)
        {
            if (data.FFmpegFile.Length == 0) return false;
            if (data.InputVideo.Length == 0) return false;
            if (data.OutputVideo.Length == 0) return false;
            if (data.Start.Checked == false && data.End.Checked == false)
            {
                long startSeconds = (data.Start.Hour * 10000000L) + (data.Start.Minute * 100000) + (data.Start.Seconds * 1000) + data.Start.Milliseconds;
                long endSeconds = (data.End.Hour * 10000000L) + (data.End.Minute * 100000) + (data.End.Seconds * 1000) + data.End.Milliseconds;
                if (endSeconds > 0 && startSeconds > endSeconds) return false;
            }
            return true;
        }

        private void CmdExecutedCommand(object? obj)
        {
            ResultText = $"\"{data.FFmpegFile}\" {FFmpegArguments.FastCut(data)}\n";
        }

        internal void DragAndDropFile(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            
            if (extension.Equals(".exe"))
            {
                data.FFmpegFile = fileName;
                SetupfileSave();
                return;
            }
            else if (extension.Length > 0)
            {
                data.InputVideo = fileName;
                data.OutputVideo = $"{Path.GetDirectoryName(fileName)}\\{Path.GetFileNameWithoutExtension(fileName)}_cut{Path.GetExtension(fileName)}";
                return;
            }
            else if (data.InputVideo.Length > 0)
            {
                data.OutputVideo = $"{fileName}\\{Path.GetFileNameWithoutExtension(data.InputVideo)}_cut{Path.GetExtension(data.InputVideo)}";
            }
        }
    }
}