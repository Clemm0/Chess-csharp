using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessGame
{
    public class AIOpponent
    {
        private Random random = new Random();
        private Dictionary<string, int> positionHistory = new Dictionary<string, int>();
        private DifficultyLevel currentDifficulty = DifficultyLevel.Newbie;
        private int consecutiveWins = 0;

        private const int PawnValue   = 100;
        private const int KnightValue = 320;
        private const int BishopValue = 330;
        private const int RookValue   = 500;
        private const int QueenValue  = 900;
        private const int KingValue   = 20000;

        private static readonly int[,] PawnTable = {
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            { 50, 50, 50, 50, 50, 50, 50, 50 },
            { 10, 10, 20, 30, 30, 20, 10, 10 },
            {  5,  5, 10, 25, 25, 10,  5,  5 },
            {  0,  0,  0, 20, 20,  0,  0,  0 },
            {  5, -5,-10,  0,  0,-10, -5,  5 },
            {  5, 10, 10,-20,-20, 10, 10,  5 },
            {  0,  0,  0,  0,  0,  0,  0,  0 }
        };
        private static readonly int[,] KnightTable = {
            {-50,-40,-30,-30,-30,-30,-40,-50 },
            {-40,-20,  0,  0,  0,  0,-20,-40 },
            {-30,  0, 10, 15, 15, 10,  0,-30 },
            {-30,  5, 15, 20, 20, 15,  5,-30 },
            {-30,  0, 15, 20, 20, 15,  0,-30 },
            {-30,  5, 10, 15, 15, 10,  5,-30 },
            {-40,-20,  0,  5,  5,  0,-20,-40 },
            {-50,-40,-30,-30,-30,-30,-40,-50 }
        };
        private static readonly int[,] BishopTable = {
            {-20,-10,-10,-10,-10,-10,-10,-20 },
            {-10,  0,  0,  0,  0,  0,  0,-10 },
            {-10,  0,  5, 10, 10,  5,  0,-10 },
            {-10,  5,  5, 10, 10,  5,  5,-10 },
            {-10,  0, 10, 10, 10, 10,  0,-10 },
            {-10, 10, 10, 10, 10, 10, 10,-10 },
            {-10,  5,  0,  0,  0,  0,  5,-10 },
            {-20,-10,-10,-10,-10,-10,-10,-20 }
        };
        private static readonly int[,] RookTable = {
            {  0,  0,  0,  0,  0,  0,  0,  0 },
            {  5, 10, 10, 10, 10, 10, 10,  5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            { -5,  0,  0,  0,  0,  0,  0, -5 },
            {  0,  0,  0,  5,  5,  0,  0,  0 }
        };
        private static readonly int[,] QueenTable = {
            {-20,-10,-10, -5, -5,-10,-10,-20 },
            {-10,  0,  0,  0,  0,  0,  0,-10 },
            {-10,  0,  5,  5,  5,  5,  0,-10 },
            { -5,  0,  5,  5,  5,  5,  0, -5 },
            {  0,  0,  5,  5,  5,  5,  0, -5 },
            {-10,  5,  5,  5,  5,  5,  0,-10 },
            {-10,  0,  5,  0,  0,  0,  0,-10 },
            {-20,-10,-10, -5, -5,-10,-10,-20 }
        };
        private static readonly int[,] KingTable = {
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-30,-40,-40,-50,-50,-40,-40,-30 },
            {-20,-30,-30,-40,-40,-30,-30,-20 },
            {-10,-20,-20,-20,-20,-20,-20,-10 },
            { 20, 20,  0,  0,  0,  0, 20, 20 },
            { 20, 30, 10,  0,  0, 10, 30, 20 }
        };

        public DifficultyLevel GetCurrentDifficulty() => currentDifficulty;

        public void SetDifficulty(DifficultyLevel difficulty)
        {
            currentDifficulty = difficulty;
            consecutiveWins = 0;
        }

        public void RecordWin()
        {
            consecutiveWins++;
            if (consecutiveWins >= 3 && currentDifficulty < DifficultyLevel.Master)
            {
                currentDifficulty++;
                consecutiveWins = 0;
            }
        }

        public void RecordLoss() => consecutiveWins = 0;

        public (int startRow, int startCol, int endRow, int endCol, PieceType promotionPiece)? GetBestMove(Board board, PieceColor aiColor)
        {
            var validMoves = GetAllValidMoves(board, aiColor);
            if (validMoves.Count == 0) return null;

            if (currentDifficulty == DifficultyLevel.Newbie)
            {
                var m = validMoves[random.Next(validMoves.Count)];
                return (m.startRow, m.startCol, m.endRow, m.endCol, m.promotionPiece);
            }

            int depth = GetDepthFromDifficulty();
            var result = Minimax(board, depth, int.MinValue, int.MaxValue, true, aiColor, aiColor);

            if (result.bestMove.HasValue)
            {
                string hash = GetBoardHash(board);
                positionHistory[hash] = positionHistory.GetValueOrDefault(hash, 0) + 1;
                return result.bestMove;
            }

            var fallback = validMoves[0];
            return (fallback.startRow, fallback.startCol, fallback.endRow, fallback.endCol, fallback.promotionPiece);
        }

        private int GetDepthFromDifficulty()
        {
            return currentDifficulty switch
            {
                DifficultyLevel.Intermediate => 2,
                DifficultyLevel.Good         => 3,
                DifficultyLevel.Master       => 4,
                _ => 2
            };
        }

        private ((int startRow, int startCol, int endRow, int endCol, PieceType promotionPiece)? bestMove, int bestScore) Minimax(
            Board board, int depth, int alpha, int beta, bool isMaximizing, PieceColor aiColor, PieceColor currentColor)
        {
            if (depth == 0)
                return (null, EvaluateBoard(board, aiColor));

            var moves = GetAllValidMoves(board, currentColor);
            if (moves.Count == 0)
            {
                var v = new MoveValidator(board);
                if (v.IsKingInCheck(currentColor))
                    return (null, isMaximizing ? -50000 - depth * 100 : 50000 + depth * 100);
                return (null, 0); 
            }

            var ordered = OrderMoves(board, moves);

            if (isMaximizing)
            {
                int maxScore = int.MinValue;
                (int, int, int, int, PieceType)? bestMove = null;

                foreach (var move in ordered)
                {
                    Board nb = new Board(board);
                    ApplyMoveToBoard(nb, move.startRow, move.startCol, move.endRow, move.endCol, move.promotionPiece, currentColor);
                    int score = Minimax(nb, depth - 1, alpha, beta, false, aiColor, GetOpponentColor(currentColor)).bestScore;

                    if (score > maxScore) { maxScore = score; bestMove = (move.startRow, move.startCol, move.endRow, move.endCol, move.promotionPiece); }
                    alpha = Math.Max(alpha, maxScore);
                    if (beta <= alpha) break;
                }
                return (bestMove, maxScore);
            }
            else
            {
                int minScore = int.MaxValue;
                (int, int, int, int, PieceType)? bestMove = null;

                foreach (var move in ordered)
                {
                    Board nb = new Board(board);
                    ApplyMoveToBoard(nb, move.startRow, move.startCol, move.endRow, move.endCol, move.promotionPiece, currentColor);
                    int score = Minimax(nb, depth - 1, alpha, beta, true, aiColor, GetOpponentColor(currentColor)).bestScore;

                    if (score < minScore) { minScore = score; bestMove = (move.startRow, move.startCol, move.endRow, move.endCol, move.promotionPiece); }
                    beta = Math.Min(beta, minScore);
                    if (beta <= alpha) break;
                }
                return (bestMove, minScore);
            }
        }

        private List<(int startRow, int startCol, int endRow, int endCol, PieceType promotionPiece, Piece movedPiece)> OrderMoves(
            Board board,
            List<(int startRow, int startCol, int endRow, int endCol, PieceType promotionPiece, Piece movedPiece)> moves)
        {
            return moves.OrderByDescending(m =>
            {
                int score = 0;
                Piece victim = board.Squares[m.endRow, m.endCol];
                if (victim != null)
                    score += GetPieceValue(victim.Type) * 10 - GetPieceValue(m.movedPiece.Type);
                if (m.promotionPiece != PieceType.None)
                    score += GetPieceValue(m.promotionPiece);
                return score;
            }).ToList();
        }

        private int EvaluateBoard(Board board, PieceColor aiColor)
        {
            int score = 0;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board.Squares[row, col];
                    if (piece == null) continue;

                    int value = GetPieceValue(piece.Type) + GetPositionBonus(piece.Type, piece.Color, row, col);
                    score += piece.Color == aiColor ? value : -value;
                }
            }

            string hash = GetBoardHash(board);
            if (positionHistory.TryGetValue(hash, out int visits))
                score -= visits * 30;

            return score;
        }

        private int GetPositionBonus(PieceType type, PieceColor color, int row, int col)
        {
            int r = color == PieceColor.White ? row : 7 - row;  
            return type switch
            {
                PieceType.Pawn   => PawnTable[r, col],
                PieceType.Knight => KnightTable[r, col],
                PieceType.Bishop => BishopTable[r, col],
                PieceType.Rook   => RookTable[r, col],
                PieceType.Queen  => QueenTable[r, col],
                PieceType.King   => KingTable[r, col],
                _ => 0
            };
        }

        private void ApplyMoveToBoard(Board nb, int startRow, int startCol, int endRow, int endCol, PieceType promotionPiece, PieceColor color)
        {
            Piece movedPiece = nb.Squares[startRow, startCol];

            if (movedPiece.Type == PieceType.Pawn && Math.Abs(endCol - startCol) == 1 && nb.Squares[endRow, endCol] == null)
            {
                if (nb.EnPassantRow.HasValue && nb.EnPassantCol.HasValue &&
                    endRow == nb.EnPassantRow.Value && endCol == nb.EnPassantCol.Value)
                {
                    int dir = color == PieceColor.White ? -1 : 1;
                    nb.Squares[endRow - dir, endCol] = null;
                }
            }

            nb.Squares[endRow, endCol] = movedPiece;
            nb.Squares[startRow, startCol] = null;
            movedPiece.HasMoved = true;

            if (movedPiece.Type == PieceType.King && Math.Abs(endCol - startCol) == 2)
            {
                int rSrc = endCol > startCol ? 7 : 0;
                int rDst = endCol > startCol ? 5 : 3;
                Piece rook = nb.Squares[startRow, rSrc];
                if (rook != null)
                {
                    nb.Squares[startRow, rDst] = rook;
                    nb.Squares[startRow, rSrc] = null;
                    rook.HasMoved = true;
                }
            }

            if (promotionPiece != PieceType.None)
                nb.Squares[endRow, endCol] = new Piece(promotionPiece, color);

            if (movedPiece.Type == PieceType.Pawn && Math.Abs(endRow - startRow) == 2)
            {
                int dir = color == PieceColor.White ? -1 : 1;
                nb.SetEnPassant(startRow + dir, startCol, color);
            }
            else
            {
                nb.ClearEnPassant();
            }
        }

        private List<(int startRow, int startCol, int endRow, int endCol, PieceType promotionPiece, Piece movedPiece)> GetAllValidMoves(Board board, PieceColor color)
        {
            var moves = new List<(int, int, int, int, PieceType, Piece)>();
            var validator = new MoveValidator(board);

            for (int sr = 0; sr < 8; sr++)
                for (int sc = 0; sc < 8; sc++)
                {
                    Piece piece = board.Squares[sr, sc];
                    if (piece == null || piece.Color != color) continue;

                    for (int er = 0; er < 8; er++)
                        for (int ec = 0; ec < 8; ec++)
                            if (validator.IsValidMove(sr, sc, er, ec, color, out PieceType promo))
                                moves.Add((sr, sc, er, ec, promo, piece));
                }

            return moves;
        }

        private int GetPieceValue(PieceType type) => type switch
        {
            PieceType.Pawn   => PawnValue,
            PieceType.Knight => KnightValue,
            PieceType.Bishop => BishopValue,
            PieceType.Rook   => RookValue,
            PieceType.Queen  => QueenValue,
            PieceType.King   => KingValue,
            _ => 0
        };

        private string GetBoardHash(Board board)
        {
            var sb = new System.Text.StringBuilder(128);
            for (int r = 0; r < 8; r++)
                for (int c = 0; c < 8; c++)
                {
                    Piece p = board.Squares[r, c];
                    sb.Append(p == null ? '.' : (char)((int)p.Type * 2 + (int)p.Color));
                }
            return sb.ToString();
        }

        private PieceColor GetOpponentColor(PieceColor color) =>
            color == PieceColor.White ? PieceColor.Black : PieceColor.White;
    }
}