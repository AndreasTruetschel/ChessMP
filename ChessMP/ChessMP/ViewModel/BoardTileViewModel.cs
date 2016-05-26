using System;
using System.ComponentModel;
using ChessMP.Model;

namespace ChessMP.ViewModel
{
    /// <summary>
    /// Represents a view model for a tile of the board.
    /// </summary>
    public sealed class BoardTileViewModel : ViewModel
    {
        private int _x;
        private int _y;
        private BoardViewModel _parent;
        private Piece _model;
        private BoardHoverState _hoverState = BoardHoverState.None;
        private string _piece = null;
        private Membership _membership;

        /// <summary>
        /// Initialized a new instance of the <see cref="BoardTileViewModel"/> class.
        /// </summary>
        /// <param name="parent">The parent view model.</param>
        /// <param name="x">The tiles x coordinate.</param>
        /// <param name="y">The tiles y corrdinate.</param>
        public BoardTileViewModel(BoardViewModel parent, int x, int y, Membership membership)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            if (membership == Membership.Board && (x < 0 || x > 7))
                throw new ArgumentOutOfRangeException(nameof(x));

            if (membership == Membership.Board && (y < 0 || y > 7))
                throw new ArgumentOutOfRangeException(nameof(y));

            if ((membership == Membership.CapturedBl || membership == Membership.CapturedWh) && (x < 0 || x > 1))
                throw new ArgumentOutOfRangeException(nameof(x));

            if ((membership == Membership.CapturedBl || membership == Membership.CapturedWh) && (y < 0 || y > 7))
                throw new ArgumentOutOfRangeException(nameof(y));

            _x = x;
            _y = y;
            _parent = parent;
            _membership = membership;

            UpdateModel();
            _parent.Model.PropertyChanged += UpdateModel;
        }

        /// <summary>
        /// Gets the board.
        /// </summary>
        public Board Board
        {
            get { return _parent.Model; }
        }

        /// <summary>
        /// Gets the tile color.
        /// </summary>
        public BoardTileColor Color
        {
            get { return (BoardTileColor)Math.Abs((_x - _y) % 2); }
        }

        /// <summary>
        /// Gets or sets the tile hover state.
        /// </summary>
        public BoardHoverState HoverState
        {
            get { return _hoverState; }
            set
            {
                if (_hoverState == value)
                    return;

                _hoverState = value;

                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the tile name.
        /// </summary>
        public string Name
        {
            get
            {
                char[] chars = new char[2];

                chars[0] = (char)('A' + _x);
                chars[1] = (char)('1' + _y);

                return new string(chars);
            }
        }

        /// <summary>
        /// Gets the name of the piece places on the tile.
        /// This is used for selecting the correct picture.
        /// </summary>
        public string Piece
        {
            get { return _piece; }
            set
            {
                if (_piece == value)
                    return;

                _piece = value;

                RaisePropertyChanged();
            }
        }

        public Piece Model
        {
            get { return _model; }
        }

        /// <summary>
        /// Called whenever the user clicks on the tile.
        /// </summary>
        [Command]
        public void Select()
        {
            if (_parent._selectedTile == null && Model != null && Model.Selectable)
            {
                _parent._selectedTile = this;
                SetAllHoverStates();
                return;

            }

            if (_parent._selectedTile != null)
            {
                // clicking on the same tile again
                if (_parent._selectedTile == this)
                {
                    _parent._selectedTile = null;
                    AllHoverstatesToNone();
                    return;
                }

                if (HoverState == BoardHoverState.None)
                {
                    if (Model == null)
                    {
                        _parent._selectedTile = null;
                        AllHoverstatesToNone();
                        return;
                    }

                    if (Model != null && Model.Selectable)
                    {
                        _parent._selectedTile = this;
                        AllHoverstatesToNone();
                        SetAllHoverStates();
                        return;
                    }

                    else
                    {
                        AllHoverstatesToNone();
                        _parent._selectedTile = null;
                    }


                }

                if (HoverState == BoardHoverState.Movement || HoverState == BoardHoverState.Capture)
                {
                    int xOld = _parent._selectedTile._x;
                    int yOld = _parent._selectedTile._y;
                    _parent._selectedTile._model.MoveTo(_x, _y);

                    _parent.Model.CheckOrCheckmate();
                    UpdateModel();
                    _parent._selectedTile.UpdateModel();
                    _parent._selectedTile = null;
                    AllHoverstatesToNone();

                    return;
                }
            }
        }

        private void Board_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != $"Index[{_x}, {_y}]")
                return;
            UpdateModel();
        }

        /// <summary>
        /// Updates the tile information from its model.
        /// </summary>
        private void UpdateModel()
        {
            if (_membership == Membership.Board)            
                _model = Board[_x, _y];

            if (_membership == Membership.CapturedBl)
                _model = Board.GetCapturedBlack(_x, _y);

            if (_membership == Membership.CapturedWh)
                _model = Board.GetCapturedWhite(_x, _y);

            if (_model != null)
            {
                HoverState = BoardHoverState.None;
                Piece = _model.GetType().Name.ToLower() + "_" + _model.Color.ToString().ToLower();
            }
            else
            {
                HoverState = BoardHoverState.None;
                Piece = null;
            }
        }

        private void UpdateModel(Object sender, PropertyChangedEventArgs e)
        {
            UpdateModel();
        }

        private void AllHoverstatesToNone()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    _parent.Tiles[j + i * 8].HoverState = BoardHoverState.None;
                }
            }
        }

        private void SetAllHoverStates()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (_model.CanMoveTo(j, i) && !(_model.WouldBeOwnKingCapturable(_x, _y, j, i)))
                    {
                        if (_parent.Tiles[j + i * 8]._model == null)
                            _parent.Tiles[j + i * 8].HoverState = BoardHoverState.Movement;
                        else
                            _parent.Tiles[j + i * 8].HoverState = BoardHoverState.Capture;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Represents possible values for the tile color.
    /// </summary>
    public enum BoardTileColor
    {
        /// <summary>Black tile color.</summary>
        Black,

        /// <summary>White tile color.</summary>
        White
    }

    /// <summary>
    /// Represents possible values for the hover state.
    /// </summary>
    public enum BoardHoverState
    {
        /// <summary>No hover state. Tile is display as usual.</summary>
        None,

        /// <summary>Movemenet hover state. Tile is displayed green.</summary>
        Movement,

        /// <summary>Capture hover state. Tile is displayed red.</summary>
        Capture
    }

    /// <summary>
    /// Specifies whether the tile is part of the Board, CapturedBlTiles or CapturedWhTiles
    /// </summary>
    public enum Membership
    {
        Board,

        CapturedBl,

        CapturedWh
    }
}
