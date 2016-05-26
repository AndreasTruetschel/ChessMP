using System;
using ChessMP.Model;

namespace ChessMP.ViewModel
{
    /// <summary>
    /// Represents the view model for a chess board.
    /// </summary>
    public class BoardViewModel : ViewModel
    {
        private BoardTileViewModel[] _tiles;
        private BoardTileViewModel[] _capturedBlTiles;
        private BoardTileViewModel[] _capturedWhTiles;
        private Board _model;

        internal BoardTileViewModel _selectedTile = null; // Tip: Can save the last clicked tile, checked in BoardTileViewModel.Select()

        /// <summary>
        /// Creates a new instance of the <see cref="BoardViewModel"/> class.
        /// </summary>
        /// <param name="board">The board.</param>
        public BoardViewModel(Board board)
        {
            if (board == null)
                throw new ArgumentNullException(nameof(board));

            _model = board;

            CreateTiles();
        }

        /// <summary>
        /// Gets the board tiles.
        /// </summary>
        public BoardTileViewModel[] Tiles // TODO: Should be read-only - No your task - Just a thought
        {
            get { return _tiles; }
        }

        /// <summary>
        /// Gets the tiles for the captured black pieces.
        /// </summary>
        public BoardTileViewModel[] CapturedBlTiles
        {
            get { return _capturedBlTiles; }
        }

        /// <summary>
        /// Gets the tiles for the captured white pieces.
        /// </summary>
        public BoardTileViewModel[] CapturedWhTiles
        {
            get { return _capturedWhTiles; }
        }


        /// <summary>
        /// Gets the model.
        /// </summary>
        public Board Model
        {
            get { return _model; }
        }

        /// <summary>
        /// Inits <see cref="Tiles"/>.
        /// </summary>
        private void CreateTiles()
        {
            _tiles = new BoardTileViewModel[8 * 8];
            _capturedBlTiles = new BoardTileViewModel[2 * 8];
            _capturedWhTiles = new BoardTileViewModel[2 * 8];

            for (int i = 0; i < 8 * 8; i++)
            {
                int x = i % 8;
                int y = i / 8;

                // Init Tile              
                // Save to list
                _tiles[x + y * 8] = new BoardTileViewModel(this, x, y, Membership.Board);
            }

            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    // Init Tile              
                    // Save to list
                    _capturedBlTiles[x + y * 2] = new BoardTileViewModel(this, x, y, Membership.CapturedBl);
                    _capturedWhTiles[x + y * 2] = new BoardTileViewModel(this, x, y, Membership.CapturedWh);
                }

            }
        }
    }
}
