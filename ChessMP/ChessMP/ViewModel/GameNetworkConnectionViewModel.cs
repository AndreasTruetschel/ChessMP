using System;
using System.Net;
using ChessMP.Model;

namespace ChessMP.ViewModel
{
    public sealed class GameNetworkConnectionViewModel : ViewModel
    {
        private readonly GameNetworkConnection _model;
        private bool _isCreated = false;

        public GameNetworkConnectionViewModel() : this(new GameNetworkConnection()) { }

        public GameNetworkConnectionViewModel(GameNetworkConnection model)
        {
            _model = model;
        }

        [Command]
        public void CreateHost()
        {
            if(_isCreated)
            {
                return;
            }

            _model.CreateHost();

            _isCreated = true;
        }

        [Command]
        public void CreateClient()
        {
            if (_isCreated)
            {
                return;
            }

            if (IP == null)
                throw new InvalidOperationException();

            _model.CreateClient(IP);

            _isCreated = true;
        }

        public IPEndPoint IP
        {
            get;
            set;
        }

        public GameNetworkConnection Model
        {
            get { return _model; }
        }
    }
}
