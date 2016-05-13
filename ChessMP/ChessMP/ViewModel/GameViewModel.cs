using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Input;
using ChessMP.Model;

namespace ChessMP.ViewModel
{
    /// <summary>
    /// Represents a game view model.
    /// </summary>
    public class GameViewModel : ViewModel
    {
        private BoardViewModel _boardViewModel;
        private Game _model;
        private GameNetworkConnectionViewModel _gameNetworkConnection;

        /// <summary>
        /// Creates a new instance of the <see cref="GameViewModel"/> class.
        /// </summary>
        public GameViewModel()
        {
            _model = new Game();

            if(_model.NetStream != null)
            {
                _gameNetworkConnection = new GameNetworkConnectionViewModel(_model.NetStream);
            }

            _boardViewModel = new BoardViewModel(_model.Board);           
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        public Game Model
        {
            get { return _model; }
        }

        /// <summary>
        /// Gets the board view model.
        /// </summary>
        public BoardViewModel BoardViewModel
        {
            get { return _boardViewModel; }
        }

        public GameNetworkConnectionViewModel NetStream
        {
            get { return _gameNetworkConnection; }
            set
            {
                if (_gameNetworkConnection == value)
                    return;

                _gameNetworkConnection = value;

                _model.NetStream = value.Model;
            }
        }

        /// <summary>
        /// Called whenever the game shall be started.
        /// </summary>
        [Command]
        public void Start()
        {
            _model.Start();
        }

        /// <summary>
        /// Called whenever the game shall be stopped.
        /// </summary>
        [Command]
        public void Stop()
        {
            _model.Stop();
        }
    }
}
