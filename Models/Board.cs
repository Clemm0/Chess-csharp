using System;

namespace ChessGame
{
    public class Board
    {
        public Piece[,] Squares { get; private set; }
        
        public int? EnPassantRow { get; set; }
        public int? EnPassantCol { get; set; }
        public PieceColor? EnPassantColor { get; set; }
        public bool IsEnPassantAvailable { get; set; }

        public Board()
        {
            Squares = new Piece[8, 8];
            EnPassantRow = null;
            EnPassantCol = null;
            EnPassantColor = null;
            IsEnPassantAvailable = false;
            InitializeBoard();
        }

        public Board(Board other)
        {
            Squares = new Piece[8, 8];
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if (other.Squares[i, j] != null)
                        Squares[i, j] = new Piece(other.Squares[i, j].Type, other.Squares[i, j].Color)
                        {
                            HasMoved = other.Squares[i, j].HasMoved
                        };
            
            EnPassantRow = other.EnPassantRow;
            EnPassantCol = other.EnPassantCol;
            EnPassantColor = other.EnPassantColor;
            IsEnPassantAvailable = other.IsEnPassantAvailable;
        }

        private void InitializeBoard()
        {
            for (int col = 0; col < 8; col++)
            {
                Squares[1, col] = new Piece(PieceType.Pawn, PieceColor.Black);
                Squares[6, col] = new Piece(PieceType.Pawn, PieceColor.White);
            }

            Squares[0, 0] = new Piece(PieceType.Rook, PieceColor.Black);
            Squares[0, 1] = new Piece(PieceType.Knight, PieceColor.Black);
            Squares[0, 2] = new Piece(PieceType.Bishop, PieceColor.Black);
            Squares[0, 3] = new Piece(PieceType.Queen, PieceColor.Black);
            Squares[0, 4] = new Piece(PieceType.King, PieceColor.Black);
            Squares[0, 5] = new Piece(PieceType.Bishop, PieceColor.Black);
            Squares[0, 6] = new Piece(PieceType.Knight, PieceColor.Black);
            Squares[0, 7] = new Piece(PieceType.Rook, PieceColor.Black);

            Squares[7, 0] = new Piece(PieceType.Rook, PieceColor.White);
            Squares[7, 1] = new Piece(PieceType.Knight, PieceColor.White);
            Squares[7, 2] = new Piece(PieceType.Bishop, PieceColor.White);
            Squares[7, 3] = new Piece(PieceType.Queen, PieceColor.White);
            Squares[7, 4] = new Piece(PieceType.King, PieceColor.White);
            Squares[7, 5] = new Piece(PieceType.Bishop, PieceColor.White);
            Squares[7, 6] = new Piece(PieceType.Knight, PieceColor.White);
            Squares[7, 7] = new Piece(PieceType.Rook, PieceColor.White);
        }
        
        public void SetEnPassant(int row, int col, PieceColor color)
        {
            EnPassantRow = row;
            EnPassantCol = col;
            EnPassantColor = color;
            IsEnPassantAvailable = true;
        }

        public void ClearEnPassant()
        {
            EnPassantRow = null;
            EnPassantCol = null;
            EnPassantColor = null;
            IsEnPassantAvailable = false;
        }
    }
}