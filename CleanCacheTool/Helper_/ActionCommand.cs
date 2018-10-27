using System;
using System.Windows.Input;
using CleanCacheTool.Annotations;

namespace CleanCacheTool
{
    public class ActionCommand : ICommand
    {
        private Action _execute;

        private Predicate<object> _canExecute;

        public ActionCommand([NotNull]Action execute) : this(execute, DefaultCanExecute)
        {
        }

        public ActionCommand([NotNull]Action execute, Predicate<object> canExecute)
        {
            this._execute = execute ?? throw new ArgumentNullException("execute");
            this._canExecute = canExecute ?? throw new ArgumentNullException("canExecute");
        }
        public bool CanExecute(object parameter)
        {
            return this._canExecute != null && this._canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this._execute();
        }
        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
                this.CanExecuteChangedInternal += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
                this.CanExecuteChangedInternal -= value;
            }
        }

        private event EventHandler CanExecuteChangedInternal;

        public void OnCanExecuteChanged()
        {
            EventHandler handler = this.CanExecuteChangedInternal;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private static bool DefaultCanExecute(object parameter)
        {
            return true;
        }
    }
}
