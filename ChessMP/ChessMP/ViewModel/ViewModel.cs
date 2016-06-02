using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ChessMP.ViewModel
{
    /// <summary>
    /// Base class for view models.
    /// </summary>
    public abstract class ViewModel : INotifyPropertyChanged
    {
        private CommandManager _commands;

        /// <summary>
        /// Creates a new instance of the <see cref="ViewModel"/> type in a derived class.
        /// </summary>
        protected ViewModel()
        {
            _commands = new CommandManager(this);
        }

        /// <summary>
        /// Gets the command manager.
        /// </summary>
        public CommandManager Commands
        {
            get { return _commands; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            //if (propertyName == null)
            //    throw new ArgumentNullException(nameof(propertyName));
            //if(PropertyChanged != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));            
        }
    }
}
