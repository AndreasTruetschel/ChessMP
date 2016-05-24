using System;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ChessMP.Model
{
    /// <summary>
    /// Base class for all chess pieces.
    /// </summary>
    public abstract class Piece : INotifyPropertyChanged
    {
        protected readonly Board _board;
        protected readonly PieceColor _color;

        /// <summary>
        /// Creates a new instance of the <see cref="Piece"/> type in a derived class.
        /// </summary>
        /// <param name="board">The chess board.</param>
        /// <param name="color">The piece color.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="board"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="color"/> is not a valid piece color.</exception>
        protected Piece(Board board, PieceColor color)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            if (!Enum.IsDefined(typeof(PieceColor), color))
                throw new ArgumentException("The piece color is invalid.", nameof(color));

            _board = board;
            _color = color;
        }

        /// <summary>
        /// Gets the chess board.
        /// </summary>
        public Board Board
        {
            get { return _board; }
        }

        /// <summary>
        /// Gets the piece color.
        /// </summary>
        public PieceColor Color
        {
            get { return _color; }
        }

        /// <summary>
        /// Gets the X-Component of the piece coordinates.
        /// </summary>
        public int PositionX
        {
            get
            {
                for (int i = 0; i <= 7; i++)
                {
                    for (int j = 0; j <= 7; j++)
                    {
                        if (Board[i, j] == this)
                            return i;
                    }
                }

                return -1;
            }
        }

        /// <summary>
        /// Gets the Y-Component of the piece coordinates.
        /// </summary>
        public int PositionY
        {
            get
            {
                for (int i = 0; i <= 7; i++)
                {
                    for (int j = 0; j <= 7; j++)
                    {
                        if (Board[i, j] == this)
                            return j;
                    }
                }

                return -1;
            }
        }

        public bool Selectable
        {
            get
            {
                if (Board.Game.NetStream == null)
                    return false;

                if (Color == Board.Game.NetStream.PlayersColour && Board.Game.NetStream.MyTurn && this != null)
                    return true;

                else
                    return false;
            }
        }

        /// <summary>
        /// Checks if a piece is capturable at the specified coordinates.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Capturable
        {
            get
            {
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (Board[j, i] != null && Board[j, i].CanMoveTo(PositionX, PositionY))
                            return true;
                    }
                }

                return false;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract Piece Clone();

        /// <summary>
        /// Returns a boolean value in a derived class indicating whether 
        /// the piece can be moved to the specified target coordiantes.
        /// </summary>
        /// <param name="x">The x-component of the target coordinates.</param>
        /// <param name="y">The y-component of the target coordinates.</param>
        /// <returns>True if the piece can be moved to the target coordinates, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either <paramref name="x"/> or <paramref name="y"/> is out of range.</exception>
        public abstract bool CanMoveTo(int x, int y);

        /// <summary>
        /// Moves the piece to the specified target coordinate if this is possible,
        /// returning if the move was performed.
        /// </summary>
        /// <param name="x">The x-component of the target coordinates.</param>
        /// <param name="y">The y-component of the target coordinates.</param>
        /// <returns>True if the piece can be moved to the target coordinates, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either <paramref name="x"/> or <paramref name="y"/> is out of range.</exception>
        public virtual bool MoveTo(int x, int y)
        {
            // Move the piece to the specified position, after checking if this move is legal.
            // If a piece is placed on the target position, notify it with a call to Capture().

            int xOld = PositionX;
            int yOld = PositionY;
            Piece pieceOnTargetPosition = Board[x, y];

            if (CanMoveTo(x, y))
            {
                Board[PositionX, PositionY] = null;
                Board[x, y] = this;
                RaisePropertyChanged();

                if (pieceOnTargetPosition != null)
                    Capture(pieceOnTargetPosition);

                Board.TerminateDraw(xOld, yOld, x, y);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called whenever a piece is captured.
        /// </summary>
        public void Capture(Piece capturedPiece)
        {
            // Called when a piece is captured.
            // Use this to modify any state (f.e. properties) and also publish this (PropertyChanged).
            if (capturedPiece == null)
                throw new ArgumentNullException(nameof(capturedPiece));

            
        }

        protected bool CanMoveStraight(int x, int y, Board Board)
        {
            if (x < 0 || x > 7 || y < 0 || y > 7)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y > 7)
                throw new ArgumentOutOfRangeException(nameof(y));

            // not current spot
            if (x == PositionX && y == PositionY)
                return false;

            // up or down
            if (x == PositionX)
            {
                int end = Math.Max(y, PositionY) - 1;
                for (int curr = Math.Min(y, PositionY) + 1; curr <= end; curr++)
                {
                    if (Board[x, curr] != null)
                        return false;
                }
                return SpotEmptyOrPieceOfDifferentColor(x, y, Board);
            }

            // right or left
            if (y == PositionY)
            {
                int end = Math.Max(x, PositionX) - 1;
                for (int curr = Math.Min(x, PositionX) + 1; curr <= end; curr++)
                {
                    if (Board[curr, y] != null)
                        return false;
                }

                return SpotEmptyOrPieceOfDifferentColor(x, y, Board);
            }

            return false;
        }

        protected bool CanMoveDiagonal(int x, int y, Board Board)
        {
            if (x < 0 || x > 7)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y > 7)
                throw new ArgumentOutOfRangeException(nameof(y));

            // not current spot
            if (x == PositionX && y == PositionY)
                return false;

            // diagonal
            if (Math.Abs(x - PositionX) == Math.Abs(y - PositionY))
            {
                // down left to up right
                if ((x < PositionX && y > PositionY) || (x > PositionX && y < PositionY))
                {
                    int startX = Math.Min(x, PositionX) + 1;
                    int startY = Math.Max(y, PositionY) - 1;
                    for (int i = 0; i < Math.Abs(x - PositionX) - 1; i++)
                    {
                        if (Board[startX + i, startY - i] != null)
                            return false;
                    }

                    return SpotEmptyOrPieceOfDifferentColor(x, y, Board);
                }

                // up left to down right
                if ((x < PositionX && y < PositionY) || (x > PositionX && y > PositionY))
                {
                    int startX = Math.Min(x, PositionX) + 1;
                    int startY = Math.Min(y, PositionY) + 1;
                    for (int i = 0; i < Math.Abs(x - PositionX) - 1; i++)
                    {
                        if (Board[startX + i, startY + i] != null)
                            return false;
                    }

                    return SpotEmptyOrPieceOfDifferentColor(x, y, Board);
                }
            }

            return false;
        }

        protected bool SpotEmptyOrPieceOfDifferentColor(int x, int y, Board Board)
        {
            if (x < 0 || x > 7)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y > 7)
                throw new ArgumentOutOfRangeException(nameof(y));

            return Board[x, y] == null || Board[x, y].Color != this.Color;
        }

        protected bool SpotEmpty(int x, int y, Board Board)
        {
            return Board[x, y] == null;
        }

        /// <summary>
        /// Checks if the own king will be capturable if the piece would do this move.
        /// </summary>
        /// <param name="xCurrent"></param>
        /// <param name="yCurrent"></param>
        /// <param name="xNew"></param>
        /// <param name="yNew"></param>
        /// <returns></returns>
        public bool WouldBeOwnKingCapturable(int xCurrent, int yCurrent, int xNew, int yNew)
        {
            if (xCurrent < 0 || xCurrent > 7)
                throw new ArgumentOutOfRangeException(nameof(xCurrent));
            if (yCurrent < 0 || yCurrent > 7)
                throw new ArgumentOutOfRangeException(nameof(yCurrent));
            if (xNew < 0 || xNew > 7)
                throw new ArgumentOutOfRangeException(nameof(xNew));
            if (yNew < 0 || yNew > 7)
                throw new ArgumentOutOfRangeException(nameof(yNew));
            
            if (!(Board[xCurrent, yCurrent].CanMoveTo(xNew, yNew)))
                return false;


            int xKing = -1;
            int yKing = -1;
            Piece tmp = Board[xNew, yNew];

            

            Board[xNew, yNew] = Board[xCurrent, yCurrent];
            Board[xCurrent, yCurrent] = null;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (Board[j, i] is King && Board[j, i].Color == Color)
                    {
                        xKing = j;
                        yKing = i;
                    }
                }
            }

            bool result = Board[xKing, yKing].Capturable;
            Board[xCurrent, yCurrent] = Board[xNew, yNew];
            Board[xNew, yNew] = tmp;
            return result;
        }

        /// <summary>
        /// Fired whenever a property of the instance is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called to fire the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected void RaisePropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Represents possible values for the piece color.
    /// </summary>
    public enum PieceColor
    {
        /// <summary>Black piece color.</summary>
        Black,

        /// <summary>White piece color.</summary>
        White
    }

    /// <summary>
    /// Represents a piece of type 'king'.
    /// </summary>
    public sealed class King : Piece // König
    {
        /// <summary>
        /// Creates a new instance of the <see cref="King"/> type in a derived class.
        /// </summary>
        /// <param name="board">The chess board.</param>
        /// <param name="color">The piece color.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="board"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="color"/> is not a valid piece color.</exception>
        public King(Board board, PieceColor color) : base(board, color) { }

        /// <summary>
        /// Returns a boolean value indicating whether 
        /// the piece can be moved to the specified target coordiantes.
        /// </summary>
        /// <param name="x">The x-component of the target coordinates.</param>
        /// <param name="y">The y-component of the target coordinates.</param>
        /// <returns>True if the piece can be moved to the target coordinates, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either <paramref name="x"/> or <paramref name="y"/> is out of range.</exception>
        public override bool CanMoveTo(int x, int y)
        {
            if (x < 0 || x > 7)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y > 7)
                throw new ArgumentOutOfRangeException(nameof(y));

            // 1 field in any direction
            if (((Math.Abs(x - PositionX) == 1 && Math.Abs(y - PositionY) == 1) || Math.Abs(x - PositionX) == 1 && y - PositionY == 0 || Math.Abs(y - PositionY) == 1 && x - PositionX == 0))
                return SpotEmptyOrPieceOfDifferentColor(x, y, Board);

            //// 1 field left or right
            //if (Math.Abs(x - PositionX) == 1 && y - PositionY == 0)
            //    return SpotEmptyOrPieceOfDifferentColor(x, y);

            //// 1 field up or down
            //if (Math.Abs(y - PositionY) == 1 && x - PositionX == 0)
            //    return SpotEmptyOrPieceOfDifferentColor(x, y);

            return false;
        }

        public override Piece Clone()
        {
            return new King(Board, Color);
        }
    }

    /// <summary>
    /// Represents a piece of type 'queen'.
    /// </summary>
    public sealed class Queen : Piece // Dame
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Queen"/> type in a derived class.
        /// </summary>
        /// <param name="board">The chess board.</param>
        /// <param name="color">The piece color.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="board"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="color"/> is not a valid piece color.</exception>
        public Queen(Board board, PieceColor color) : base(board, color) { }

        /// <summary>
        /// Returns a boolean value indicating whether 
        /// the piece can be moved to the specified target coordiantes.
        /// </summary>
        /// <param name="x">The x-component of the target coordinates.</param>
        /// <param name="y">The y-component of the target coordinates.</param>
        /// <returns>True if the piece can be moved to the target coordinates, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either <paramref name="x"/> or <paramref name="y"/> is out of range.</exception>
        public override bool CanMoveTo(int x, int y)
        {
            return CanMoveDiagonal(x, y, Board) || CanMoveStraight(x, y, Board);
        }

        public override Piece Clone()
        {
            return new Queen(Board, Color);
        }
    }

    /// <summary>
    /// Represents a piece of type 'bishop'.
    /// </summary>
    public sealed class Bishop : Piece // Läufer
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Bishop"/> type in a derived class.
        /// </summary>
        /// <param name="board">The chess board.</param>
        /// <param name="color">The piece color.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="board"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="color"/> is not a valid piece color.</exception>
        public Bishop(Board board, PieceColor color) : base(board, color) { }

        /// <summary>
        /// Returns a boolean value indicating whether 
        /// the piece can be moved to the specified target coordiantes.
        /// </summary>
        /// <param name="x">The x-component of the target coordinates.</param>
        /// <param name="y">The y-component of the target coordinates.</param>
        /// <returns>True if the piece can be moved to the target coordinates, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either <paramref name="x"/> or <paramref name="y"/> is out of range.</exception>
        public override bool CanMoveTo(int x, int y)
        {
            return CanMoveDiagonal(x, y, Board);
        }

        public override Piece Clone()
        {
            return new Bishop(Board, Color);
        }
    }

    /// <summary>
    /// Represents a piece of type 'knight'.
    /// </summary>
    public sealed class Knight : Piece // Springer
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Knight"/> type in a derived class.
        /// </summary>
        /// <param name="board">The chess board.</param>
        /// <param name="color">The piece color.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="board"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="color"/> is not a valid piece color.</exception>
        public Knight(Board board, PieceColor color) : base(board, color) { }

        /// <summary>
        /// Returns a boolean value indicating whether 
        /// the piece can be moved to the specified target coordiantes.
        /// </summary>
        /// <param name="x">The x-component of the target coordinates.</param>
        /// <param name="y">The y-component of the target coordinates.</param>
        /// <returns>True if the piece can be moved to the target coordinates, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either <paramref name="x"/> or <paramref name="y"/> is out of range.</exception>
        public override bool CanMoveTo(int x, int y)
        {
            if (x < 0 || x > 7)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y > 7)
                throw new ArgumentOutOfRangeException(nameof(x));

            // not current spot
            if (x == PositionX && y == PositionY)
                return false;

            // jump 2+1
            if ((Math.Abs(x - PositionX) == 2 && Math.Abs(y - PositionY) == 1) || (Math.Abs(x - PositionX) == 1 && Math.Abs(y - PositionY) == 2))
                return SpotEmptyOrPieceOfDifferentColor(x, y, Board);

            return false;
        }

        public override Piece Clone()
        {
            return new Knight(Board, Color);
        }
    }

    /// <summary>
    /// Represents a piece of type 'pawn'.
    /// </summary>
    public sealed class Pawn : Piece // Bauer
    {
        private int _moves = 0;
        /// <summary>
        /// Creates a new instance of the <see cref="Pawn"/> type in a derived class.
        /// </summary>
        /// <param name="board">The chess board.</param>
        /// <param name="color">The piece color.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="board"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="color"/> is not a valid piece color.</exception>
        public Pawn(Board board, PieceColor color) : base(board, color) { }


        /// <summary>
        /// Returns a boolean value indicating whether 
        /// the piece can be moved to the specified target coordiantes.
        /// </summary>
        /// <param name="x">The x-component of the target coordinates.</param>
        /// <param name="y">The y-component of the target coordinates.</param>
        /// <returns>True if the piece can be moved to the target coordinates, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either <paramref name="x"/> or <paramref name="y"/> is out of range.</exception>
        public override bool CanMoveTo(int x, int y)
        {
            if (x < 0 || x > 7)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y > 7)
                throw new ArgumentOutOfRangeException(nameof(y));

            // not current spot
            if (x == PositionX && y == PositionY)
                return false;

            if (Color == PieceColor.White)
            {
                if (_moves == 0)
                {
                    if (y - PositionY == 1 && x == PositionX)
                        return SpotEmpty(x, y, Board);

                    if (y - PositionY == 2 && x == PositionX)
                        return SpotEmpty(x, y, Board) && SpotEmpty(x, y - 1, Board);
                }

                if (_moves > 0)
                {
                    if (y - PositionY == 1 && x == PositionX)
                        return SpotEmpty(x, y, Board);

                }

                if (Math.Abs(x - PositionX) == 1 && (y - PositionY == 1) && Board[x, y] != null && Board[x, y].Color != this.Color)
                    return true;
            }

            if (Color == PieceColor.Black)
            {
                if (_moves == 0)
                {
                    if (y - PositionY == -1 && x == PositionX)
                        return SpotEmpty(x, y, Board);

                    if (y - PositionY == -2 && x == PositionX)
                        return SpotEmpty(x, y, Board) && SpotEmpty(x, y + 1, Board);
                }

                if (_moves > 0)
                {
                    if (y - PositionY == -1 && x == PositionX)
                        return SpotEmpty(x, y, Board);
                }

                if (Math.Abs(x - PositionX) == 1 && (y - PositionY == -1) && Board[x, y] != null && Board[x, y].Color != this.Color)
                    return true;
            }

            return false;
        }

        public override Piece Clone()
        {
            return new Pawn(Board, Color);
        }

        public override bool MoveTo(int x, int y)
        {
            // Move the piece to the specified position, after checking if this move is legal.
            // If a piece is placed on the target position, notify it with a call to Capture().

            if (base.MoveTo(x, y))
            {
                _moves++;
                return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Represents a piece of type 'rook'.
    /// </summary>
    public sealed class Rook : Piece // Turm
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Rook"/> type in a derived class.
        /// </summary>
        /// <param name="board">The chess board.</param>
        /// <param name="color">The piece color.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="board"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="color"/> is not a valid piece color.</exception>
        public Rook(Board board, PieceColor color) : base(board, color) { }

        /// <summary>
        /// Returns a boolean value indicating whether 
        /// the piece can be moved to the specified target coordiantes.
        /// </summary>
        /// <param name="x">The x-component of the target coordinates.</param>
        /// <param name="y">The y-component of the target coordinates.</param>
        /// <returns>True if the piece can be moved to the target coordinates, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if either <paramref name="x"/> or <paramref name="y"/> is out of range.</exception>
        public override bool CanMoveTo(int x, int y)
        {
            return CanMoveStraight(x, y, Board);
        }

        public override Piece Clone()
        {
            return new Rook(Board, Color);
        }
    }
}
