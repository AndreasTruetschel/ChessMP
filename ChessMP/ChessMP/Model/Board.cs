using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

//xxxx

namespace ChessMP.Model
{
    /// <summary>
    /// Represents a chess board.
    /// </summary>
    public class Board : INotifyPropertyChanged
    {
        private Piece[] _data = new Piece[8 * 8];
        private Game _game;

        /// <summary>
        /// Creates a new instance of the <see cref="Board"/> class.s
        /// </summary>
        public Board(Game game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            _game = game;

            Task receive = Task.Run(() =>
            {
                while (true)
                {
                    if (Game.NetStream != null && Game.NetStream.IsConnected == true)
                    {
                        int[] a = { -1, -1, -1, -1 };
                        try
                        {
                            a = Game.NetStream.ReceiveData();
                            if (a != null && a[0] != -1 && a[1] != -1 && a[2] != -1 && a[3] != -1 && this[a[0], a[1]] != null)
                            {
                                this[a[2], a[3]] = this[a[0], a[1]];
                                RaisePropertyChanged($"Index[{a[2]}, {a[3]}]");
                                this[a[0], a[1]] = null;
                                RaisePropertyChanged($"Index[{a[0]}, {a[1]}]");
                                Game.NetStream.MyTurn = true;
                                CheckOrCheckmate();
                            }
                        }

                        catch (IOException)
                        {
                            MessageBox.Show("Connection lost. Close game and start a new one.");
                            break;
                        }

                    }
                }
            });
        }

        public Game Game
        {
            get { return _game; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Piece"/> that is currently placed on the board at the specified coordinates.
        /// </summary>
        /// <param name="x">The x coordinate in the range 0 to 7.</param>
        /// <param name="y">The y coordinate in the range 0 to 7.</param>
        /// <returns>The piece at the specified coordinates or null if no piece is located at the coordinates.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either <paramref name="x"/> or <paramref name="y"/> is out of range.</exception>
        [IndexerName("Index")]
        public Piece this[int x, int y]
        {
            get
            {
                if (x < 0 || x > 7)
                    throw new ArgumentOutOfRangeException(nameof(x));

                if (y < 0 || y > 7)
                    throw new ArgumentOutOfRangeException(nameof(y));

                return _data[y * 8 + x];
            }
            internal set
            {
                if (x < 0 || x > 7)
                    throw new ArgumentOutOfRangeException(nameof(x));

                if (y < 0 || y > 7)
                    throw new ArgumentOutOfRangeException(nameof(y));

                if (_data[y * 8 + x] == value)
                    return;

                _data[y * 8 + x] = value;

                //RaisePropertyChanged($"Index[{x}, {y}]");
            }
        }

        //[IndexerName("Index")]
        //public Piece this[BoardPosition pos]
        //{
        //    get
        //    {
        //        return this[pos.X, pos.Y];
        //    }
        //    set
        //    {
        //        this[pos.X, pos.Y] = value;
        //    }
        //}

        internal void TerminateDraw(int x1, int y1, int x2, int y2)
        {
            RaisePropertyChanged($"Index[{x1}, {y1}]");
            RaisePropertyChanged($"Index[{x2}, {y2}]");
            Game.NetStream.SendData(x1, y1, x2, y2);
            Game.NetStream.MyTurn = false;
        }

        /// <summary>
        /// Fired whenever a property of the instance is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;



        /// <summary>
        /// Called to fire the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Checks if one of the kings is in check.
        /// </summary>
        /// <returns></returns>
        public string Check()
        {
            string result = "";
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (this[j, i] is King && this[j, i].Capturable)
                    {
                        result += this[j, i].Color.ToString().ToLower();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if one of the kings is checkmate.
        /// </summary>
        /// <returns></returns>
        public string Checkmate()
        {
            string result = "";
            bool w = false;
            bool b = false;
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if ((!w || !b) && this[x, y] != null && this[x, y].CanMoveTo(j, i) && !(this[x, y].WouldBeOwnKingCapturable(x, y, j, i)))
                            {
                                if (this[x, y].Color == PieceColor.White)
                                    w = true;
                                else
                                    b = true;
                            }
                        }
                    }
                }
            }

            if (!w)
                result += "white";
            if (!b)
                result += "black";

            return result;
        }

        public void CheckOrCheckmate()
        {
            string mate = Checkmate();
            if (mate != "")
            {
                MessageBox.Show("The " + mate + " King is in checkmate.");
                return;
            }

            string check = Check();
            if (check != "")
                MessageBox.Show("The " + check + " King is in check.");
        }
    }

    //public struct BoardPosition : IEquatable<BoardPosition>
    //{
    //    private readonly int _x, _y;

    //    public BoardPosition(int x, int y)
    //    {
    //        if (x < 0 || x > 7)
    //            throw new ArgumentOutOfRangeException(nameof(x));
    //        if (y < 0 || y > 7)
    //            throw new ArgumentOutOfRangeException(nameof(y));

    //        _x = x;
    //        _y = y;
    //    }

    //    public int X { get { return _x; } }
    //    public int Y { get { return _y; } }


    //    //    $"[{_x} {_y}]"

    //    public override int GetHashCode()
    //    {
    //        return base.GetHashCode();
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        return base.Equals(obj);
    //    }
    //    public bool Equals(BoardPosition other)
    //    {
    //        if (Object.ReferenceEquals(this, other))
    //            return true;

    //        if ((object)other == null)
    //            return false;

    //        if (this.X == other.X && this.Y == other.Y)
    //            return true;
    //        else
    //            return false;
    //    }

    //    public static bool operator ==(BoardPosition a, BoardPosition b)
    //    {
    //        return a.Equals(b);
    //    }

    //    public static bool operator !=(BoardPosition a, BoardPosition b)
    //    {
    //        return !(a == b);
    //    }
    //}
}
