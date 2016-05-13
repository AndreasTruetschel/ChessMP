using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Input;

namespace ChessMP
{
    public sealed class Command : ICommand
    {
        private ExecuteHandler _execute;
        private CanExecuteHandler _canExecute;

        public Command(ExecuteHandler execute) : this(execute, null) { }

        public Command(ExecuteHandler execute, CanExecuteHandler canExecute)
        {
            // Null is a valid value for name.

            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute;
        }
            
        // Currently not implemented
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;

            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }
    }

    public delegate void ExecuteHandler(object parameter);
    public delegate bool CanExecuteHandler(object parameter);

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class CommandAttribute : Attribute
    {
        private string _name;

        public CommandAttribute() { }

        public CommandAttribute(string name)
        {
            // Null is a valid value for name.

            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }

    public sealed class CommandManager
    {
        private Dictionary<string, Command> _data = new Dictionary<string, Command>();
        private object _owner;

        public CommandManager(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            _owner = obj;

            InitData();
        }

        public Command this[string name]
        {
            get
            {
                if (String.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("The argument must neither be null nor empty nor whitespace only.", nameof(name));

                if (_data.ContainsKey(name))
                    return _data[name];

                return null;
            }
        }

        public void InitData()
        {
            Type type = _owner.GetType();

            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);

            foreach (var method in methods)
            {
                CommandAttribute cmdAttr = method.GetCustomAttribute<CommandAttribute>(true);

                if (cmdAttr == null)
                    continue;

                if (method.ReturnType != typeof(void))
                    throw new InvalidOperationException("Commands must have a return type of 'void'.");

                ParameterInfo[] pars = method.GetParameters();

                Command cmd;

                // No parameters
                if (pars.Length == 0)
                {
                    cmd = new Command(p => method.Invoke(_owner, null));
                }
                // One parameter of type 'object'
                else if (pars.Length == 1 && pars[0].ParameterType == typeof(object) && !pars[0].IsOut && !pars[0].IsIn)
                {
                    cmd = new Command(p => method.Invoke(_owner, new[] { p }));
                }
                // Wrong number of arguments or invalid type
                else
                {
                    throw new InvalidOperationException("Commands must accept zero or one argument of type 'object'.");
                }

                string name = cmdAttr.Name;

                if(string.IsNullOrWhiteSpace(name))
                {
                    name = method.Name;
                }

                _data[name] = cmd;
            }
        }
    }
}
