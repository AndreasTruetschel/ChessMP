using System.Windows.Controls;
using System.Windows.Input;

namespace ChessMP
{
    public class GameNetworkHelper : UserControl
    {
        public GameNetworkHelper()
        {
            Loaded += (o, e) =>
            {
                ShowGameNetworkWindowHandler(null);
            };
        }

        private Command _showGameNetworkWindow;

        public ICommand ShowGameNetworkWindow
        {
            get
            {
                if (_showGameNetworkWindow == null)
                {
                    _showGameNetworkWindow = new Command(ShowGameNetworkWindowHandler);
                }

                return _showGameNetworkWindow;
            }
        }

        public void ShowGameNetworkWindowHandler(object param)
        {
            GameNetworkWindow window = new GameNetworkWindow();

            bool? result = window.ShowDialog();

            if (result == true)
            {
                DataContext = window.DataContext;
            }
        }
    }
}
