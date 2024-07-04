using System.Windows.Input;

namespace GumCut
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
}
