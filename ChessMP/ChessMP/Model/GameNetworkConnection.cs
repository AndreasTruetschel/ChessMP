using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;

namespace ChessMP.Model
{
    // Du musst folgedes tun: In der Game bringst du alle Figuren auf Ausgangsposition. In der Piece musst du noch Funktionalität einbauen,
    // die überprüft, ob du verbunden bist. Wenn nicht, ist auch kein Zug möglich.
    // Kein listener.AcceptSocket() darf den Hauptthread blockieren, sodass dein Fenster bedienbar bleibt und das GameNetworkConnection Objekt
    // An die game klasse weitergegeben werden kann. Kannst du asynchron oder parallel machen, wobei asynchron sauberer ist.
    // Ein netts gimmick wäre, wenn du den server per zufallsgenerator auswürfeln lässt, wer welche farbe bekommt. Dann müsstest du aber auch eine Art Protokoll einführen.
    // Der Client müsste als erstes warten (asynchron/parallel), welche farbe er vom server erhält.

    public sealed class GameNetworkConnection : INotifyPropertyChanged
    {
        public GameNetworkConnection() { }

        private NetworkStream _netStream;
        private bool _isConnected = false;
        private bool _isServer = false;
        private bool _isClient = false;
        private PieceColor _playersColour;
        private bool _myTurn = false;
        private Socket _socket;
        private TcpClient _client;

        public event PropertyChangedEventHandler PropertyChanged;

        public NetworkStream NetStream { get { return _netStream; } }

        public bool IsServer { get { return _isServer; } }
        public bool IsClient { get { return _isClient; } }
        public PieceColor PlayersColour { get { return _playersColour; } }
        public Socket Socket { get { return _socket; } }
        public TcpClient Client { get { return _client; } }
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged("IsConnected");
            }
        }


        public bool MyTurn
        {
            get
            { return _myTurn; }
            set
            {
                _myTurn = value;
                RaisePropertyChanged("MyTurn");
            }
        }

        // async void ist Mist. Besser immer async Task, wobei die Lösung jetzt auch nicht das gelbe vom Ei ist, aber akzeptabel.


        //public async void CreateHost()
        //{
        //    TcpListener listener = new TcpListener(IPAddress.Any, 9000);
        //    listener.Start();
        //    Socket socket = await listener.AcceptSocketAsync();
        //    if (socket.Connected)
        //    {
        //        _netStream = new NetworkStream(socket);
        //        _isConnected = true;
        //        _isServer = true;
        //        _playersColour = PieceColor.White;
        //        _myTurn = true;
        //        MessageBox.Show("Server connected.");
        //    }
        //}

        private Task _connectionTask = null;

        public void CreateHost()
        {
            _connectionTask = CreateHostAsync();
        }

        private async Task CreateHostAsync()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, 9000);
            listener.Start();
            Socket socket = await listener.AcceptSocketAsync();
            if (socket.Connected)
            {
                _socket = socket;
                _netStream = new NetworkStream(socket);
                IsConnected = true;
                _isServer = true;
                _playersColour = PieceColor.White;
                MyTurn = true;
                MessageBox.Show("Server connected.");
            }
        }

        //public async void CreateClient(IPEndPoint ipEndPoint)
        //{
        //    TcpClient client = new TcpClient();
        //    int n = 0;
        //    while (n < 5)
        //    {
        //        try
        //        {
        //            await client.ConnectAsync(ipEndPoint.Address, 9000);
        //            if (client.Connected)
        //            {
        //                _netStream = client.GetStream();
        //                _isConnected = true;
        //                _isClient = true;
        //                _playersColour = PieceColor.Black;
        //                MessageBox.Show("Client connected with host");

        //                break;
        //            }
        //            n++;
        //        }

        //        catch (SocketException)
        //        {
        //            n++;
        //            MessageBox.Show("Connection failed, try again");
        //        }
        //    }
        //}

        public void CreateClient(IPEndPoint ipEndPoint)
        {
            _connectionTask = CreateClientAsync(ipEndPoint);
        }

        private async Task CreateClientAsync(IPEndPoint ipEndPoint)
        {
            TcpClient client = new TcpClient();
            int n = 0;
            while (n < 5)
            {
                try
                {
                    await client.ConnectAsync(ipEndPoint.Address, 9000);
                    if (client.Connected)
                    {
                        _client = client;
                        _netStream = client.GetStream();
                        IsConnected = true;
                        _isClient = true;
                        _playersColour = PieceColor.Black;
                        MessageBox.Show("Client connected with host");

                        break;
                    }
                    n++;
                }

                catch (SocketException)
                {
                    n++;
                    MessageBox.Show("Connection failed, try again");
                }
            }
        }

        public void SendData(int xOld, int yOld, int xNew, int yNew)
        {
            int[] array = { xOld, yOld, xNew, yNew };
            BinaryFormatter bf = new BinaryFormatter();
            if (NetStream == null)
                throw new InvalidOperationException(nameof(NetStream));

            try
            {
                bf.Serialize(NetStream, array);
                NetStream.Flush();
                MyTurn = false;
            }

            catch (IOException )
            {
                MessageBox.Show("Connection lost");
            }
        }

        public int[] ReceiveData()
        {
            BinaryFormatter bf = new BinaryFormatter();
            if (NetStream == null)
                throw new InvalidOperationException(nameof(NetStream));
            return (int[])bf.Deserialize(NetStream);           
        }

        private void RaisePropertyChanged(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
