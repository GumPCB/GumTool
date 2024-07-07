﻿using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Controls;

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
        public Command FFmpegOpenButton { get; set; }
        public Command LoadVideoButton { get; set; }
        public Command SaveVideoButton { get; set; }
        public Command CutButton { get; set; }
        public Command CmdButton { get; set; }
        public Command EraseButton { get; set; }

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
            LoadVideoButton = new(LoadVideoExecutedCommand, EmptyCanExecuteCommand);
            SaveVideoButton = new(SaveVideoExecutedCommand, SaveVideoCanExecuteCommand);
            CutButton = new(CutExecutedCommand, CutCanExecuteCommand);
            CmdButton = new(CmdExecutedCommand, EmptyCanExecuteCommand);
            EraseButton = new(EraseExecutedCommand, EmptyCanExecuteCommand);
            
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
            GetEncoderInfo();
        }

        private void SetupfileSave()
        {
            FileStream fs = new(SetupfileName, FileMode.Create);
            using StreamWriter sw = new(fs);
            sw.WriteLine(data.FFmpegFile);
            sw.Close();

            GetEncoderInfo();
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

        private void GetLoadVideoInfo()
        {
            ResultText += "===== Get Video Info : \n";
            var task = Task.Run(() => FFmpegAsync("\"" + data.FFmpegFile + "\"", "-hide_banner -i \"" + data.LoadVideo + "\"", false, false)).ContinueWith((antecedent) =>
            {
                if (SplitLoadVideoInfo(FFmpegResultText) == false)
                {
                    ResultText += "Fail =========\n";
                    ResultText += FFmpegResultText;
                }

                FFmpegResultText = string.Empty;
            });
        }

        private bool SplitLoadVideoInfo(string ffmpegResult)
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

        private void LoadVideoExecutedCommand(object? obj)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();

            bool? result = dialog.ShowDialog();

            if (result != true)
                return;

            string filename = dialog.FileName;
            data.LoadVideo = filename;
            GetLoadVideoInfo();

            data.SaveVideo = $"{Path.GetDirectoryName(filename)}\\{Path.GetFileNameWithoutExtension(filename)}_cut{Path.GetExtension(filename)}";
        }

        private void SaveVideoExecutedCommand(object? obj)
        {
            string dotExrension = $"{Path.GetExtension(data.LoadVideo)}";
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = Path.GetFileNameWithoutExtension(data.SaveVideo),
                DefaultExt = dotExrension,
                Filter = $""
            };

            bool? result = dialog.ShowDialog();

            if (result != true)
                return;

            data.SaveVideo = dialog.FileName;
        }

        private bool SaveVideoCanExecuteCommand(object? obj)
        {
            if (data.LoadVideo.Length == 0) return false;
            return true;
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
            ResultText += $"====== Working Start : {DateTime.Now.ToString("F")}\n";

            string ffmpegFile = new($"\"{data.FFmpegFile}\"");
            string arguments = GetSelectedTabArguments();

            var task = Task.Run(() => FFmpegAsync(ffmpegFile, arguments, openCmd, false)).ContinueWith((antecedent) => 
            {
                Working = false;
                stopwatch.Stop();
                ResultText += (FFmpegResultText.Length == 0) ? "========= Success! =========\n" : FFmpegResultText;
                ResultText += $"====== Working End : {DateTime.Now.ToString("F")} ({stopwatch.ElapsedMilliseconds / 1000L} Seconds)\n";
                FFmpegResultText = string.Empty;
            });
        }

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

        private bool CutCanExecuteCommand(object? obj)
        {
            if (data.FFmpegFile.Length == 0) return false;
            if (data.LoadVideo.Length == 0) return false;
            if (data.SaveVideo.Length == 0) return false;
            if (data.Start.Checked == false && data.End.Checked == false && data.End.IsZero == false && data.Start > data.End) return false;

            return true;
        }

        private void CmdExecutedCommand(object? obj)
        {
            ResultText += $"\"{data.FFmpegFile}\" {GetSelectedTabArguments()}\n";
        }

        private void EraseExecutedCommand(object? obj)
        {
            ResultText = string.Empty;
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
            else if (extension.Length != 0)
            {
                data.LoadVideo = fileName;
                GetLoadVideoInfo();
                data.SaveVideo = $"{Path.GetDirectoryName(fileName)}\\{Path.GetFileNameWithoutExtension(fileName)}_cut{Path.GetExtension(fileName)}";
                return;
            }
            else
            {
                if (data.SaveVideo.Length != 0)
                {
                    data.SaveVideo = $"{fileName}\\{Path.GetFileNameWithoutExtension(data.SaveVideo)}{Path.GetExtension(data.SaveVideo)}";
                }
                else if (data.LoadVideo.Length != 0)
                {
                    data.SaveVideo = $"{fileName}\\{Path.GetFileNameWithoutExtension(data.LoadVideo)}_cut{Path.GetExtension(data.LoadVideo)}";
                }
                else
                {
                    data.SaveVideo = fileName;
                }
            }
        }
    }
}