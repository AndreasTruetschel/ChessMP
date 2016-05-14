using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;

namespace ChessMP.Model
{
    /// <summary>
    /// Represents a chess game.
    /// </summary>
    public class Game
    {
        private Board _board;
        private GameNetworkConnection _networkStream;

        /// <summary>
        /// Creates a new instance of the <see cref="Game"/> class.
        /// </summary>
        public Game()
        {
            _board = new Board(this);
            Stop();
        }

        public GameNetworkConnection NetStream
        {
            get { return _networkStream; }
            set { _networkStream = value; }
        }

        /// <summary>
        /// Gets the chess board.
        /// </summary>
        public Board Board
        {
            get { return _board; }
        }

        /// <summary>
        /// Starts the game.
        /// </summary>
        public void Start()
        {
            // Currently nothing to do here
        }

        /// <summary>
        /// Stops the game and resets the pieces.
        /// </summary>
        public void Stop()
        {
            // Set the white pieces.

            Board[0, 0] = new Rook(Board, Model.PieceColor.White);
            Board[1, 0] = new Knight(Board, Model.PieceColor.White);
            Board[2, 0] = new Bishop(Board, Model.PieceColor.White);
            Board[3, 0] = new Queen(Board, Model.PieceColor.White);
            Board[4, 0] = new King(Board, Model.PieceColor.White);
            Board[5, 0] = new Bishop(Board, Model.PieceColor.White);
            Board[6, 0] = new Knight(Board, Model.PieceColor.White);
            Board[7, 0] = new Rook(Board, Model.PieceColor.White);

            for (int i = 0; i <= 7; i++)
                Board[i, 1] = new Pawn(Board, Model.PieceColor.White);

            //Set the black pieces.

            for (int i = 0; i <= 7; i++)
                Board[i, 6] = new Pawn(Board, PieceColor.Black);

            Board[0, 7] = new Rook(Board, Model.PieceColor.Black);
            Board[1, 7] = new Knight(Board, Model.PieceColor.Black);
            Board[2, 7] = new Bishop(Board, Model.PieceColor.Black);
            Board[3, 7] = new Queen(Board, Model.PieceColor.Black);
            Board[4, 7] = new King(Board, Model.PieceColor.Black);
            Board[5, 7] = new Bishop(Board, Model.PieceColor.Black);
            Board[6, 7] = new Knight(Board, Model.PieceColor.Black);
            Board[7, 7] = new Rook(Board, Model.PieceColor.Black);

            // Clears all other positions.

            for (int i = 2; i <= 5; i++)
                for (int j = 0; j <= 7; j++)
                    Board[j, i] = null;           
        }
    }
}
