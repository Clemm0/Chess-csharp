namespace ChessGame
{
    public class Move
    {
        public int StartRow { get; set; }
        public int StartCol { get; set; }
        public int EndRow { get; set; }
        public int EndCol { get; set; }
        public Piece MovedPiece { get; set; }
        public Piece CapturedPiece { get; set; }
        public PieceType PromotionPiece { get; set; }
        public bool IsCastling { get; set; }
        public int RookStartRow { get; set; }
        public int RookStartCol { get; set; }
        public int RookEndRow { get; set; }
        public int RookEndCol { get; set; }
        public bool IsEnPassant { get; set; }
        public int EnPassantPawnRow { get; set; }
        public int EnPassantPawnCol { get; set; }
        public bool IsUndo { get; set; }

        public Move(int startRow, int startCol, int endRow, int endCol, Piece movedPiece, Piece capturedPiece)
        {
            StartRow = startRow;
            StartCol = startCol;
            EndRow = endRow;
            EndCol = endCol;
            MovedPiece = movedPiece;
            CapturedPiece = capturedPiece;
            PromotionPiece = PieceType.None;
            IsCastling = false;
            IsEnPassant = false;
            IsUndo = false;
        }
    }
}