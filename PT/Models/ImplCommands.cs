using System;
using System.Windows.Input;


namespace PT.Models {
    class ImplCommands : ICommand {
        private Action<object> _execute;
        private Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public ImplCommands(Predicate<object> canExec, Action<object> exec) {
            _execute = exec;
            _canExecute = canExec;
        }

        public ImplCommands(Action<object> exec) : this(null, exec) {
        }

        public bool CanExecute(object parameter) {
            if (_canExecute == null) {
                return true;
            }
            else {
                return false;
            }
        }

        public void Execute(object parameter) {
            _execute(parameter);
        }
    }
}
