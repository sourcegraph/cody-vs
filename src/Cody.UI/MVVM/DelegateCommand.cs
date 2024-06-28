using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Cody.UI.MVVM
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _executeMethod;
        private readonly Func<bool> _canExecute;

        public DelegateCommand(Action execute) : this(execute, null)
        {
        }

        public DelegateCommand(Action execute, Func<bool> canExecute)
        {
            _canExecute = canExecute;
            _executeMethod = execute;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute();
        }

        public bool CanExecute()
        {
            if (_canExecute == null) return true;
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            if (_executeMethod == null) return;
            _executeMethod();
        }

        protected virtual void OnCanExecuteChanged(object sender, EventArgs e)
        {
            CanExecuteChanged(sender, e);
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(this, EventArgs.Empty);
        }
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _executeMethod;
        private readonly Func<T, bool> _canExecute;

        public DelegateCommand(Action<T> execute) : this(execute, null)
        {
        }

        public DelegateCommand(Action<T> execute, Func<T, bool> canExecute)
        {
            _canExecute = canExecute;
            _executeMethod = execute;
        }

        public event EventHandler CanExecuteChanged = delegate { };

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(ConvertParameter(parameter));
        }
        private static T ConvertParameter(object parameter)
        {
            if (parameter != null)
            {
                if (parameter is T)
                    return (T)parameter;

                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter.IsValid(parameter))
                    return (T)converter.ConvertFrom(parameter);
            }

            return default(T);
        }

        public bool CanExecute(T parameter)
        {
            if (_canExecute == null) return true;
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            if (_executeMethod == null) return;
            _executeMethod(ConvertParameter(parameter));
        }

        protected virtual void OnCanExecuteChanged(object sender, EventArgs e)
        {
            CanExecuteChanged(sender, e);
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(this, EventArgs.Empty);
        }
    }
}
