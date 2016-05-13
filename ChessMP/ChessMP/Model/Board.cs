using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;

namespace ChessMP.Model
{
    /// <summary>
    /// Represents a chess board.
    /// </summary>
    public class Board : INotifyPropertyChanged, ICloneable<Board>
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

                RaisePropertyChanged($"Index[{x}, {y}]");
            }
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

        public Board Clone()
        {
            Board clone = new Board(Game);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if(this[x,y] != null)
                    {
                        clone[x, y] = this[x, y].Clone(clone);
                    }
                }
            }

            return clone;
        }

        public Board Clone(Board board)
        {
            throw new NotImplementedException();
        }
    }
}
