using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace GumTool
{
    public class Command : ICommand
    {
        private Action<object?> ExecuteMethod;
        private Func<object?, bool> CanExecuteMethod;

        public Command(Action<object?> execute_method, Func<object?, bool> canexecute_method)
        {
            ExecuteMethod = execute_method;
            CanExecuteMethod = canexecute_method;
        }

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return CanExecuteMethod(parameter);
        }

        public void Execute(object? parameter)
        {
            ExecuteMethod(parameter);
        }
    }

    public static class WindowHelper
    {
        public static string[]? IsFilesOrDirectorys(DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var files = args.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (files != null)
                {
                    foreach (var file in files)
                    {
                        if (File.Exists(file) == false && Directory.Exists(file) == false)
                            return null;
                    }
                    return files;
                }
            }
            return null;
        }

        public static string? IsDirectory(DragEventArgs args)
        {
            if (args.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                var directorys = args.Data.GetData(DataFormats.FileDrop, true) as string[];
                if (directorys != null)
                {
                    foreach (var directory in directorys)
                    {
                        if (Directory.Exists(directory) == true)
                            return directory;
                    }
                    return null;
                }
            }
            return null;
        }

        public static async Task OpenUrlAsync(string url)
        {
            ProcessStartInfo cmd = new(url);

            cmd.CreateNoWindow = false;
            cmd.UseShellExecute = true;
            cmd.RedirectStandardInput = false;
            cmd.RedirectStandardOutput = false;
            cmd.RedirectStandardError = false;

            Process process = new();
            process.EnableRaisingEvents = false;
            process.StartInfo = cmd;
            process.Start();

            await process.WaitForExitAsync();
            process.Close();
        }
    }
}
