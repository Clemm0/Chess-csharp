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

        public DifficultyLevel GetCurrentDifficulty() => currentDifficulty;

        public void RecordWin()
        {
            consecutiveWins++;
            if (consecutiveWins >= 3 && currentDifficulty < DifficultyLevel.Master)
            {
                currentDifficulty++;
                consecutiveWins = 0;
            }
        }

        public void RecordLoss()
        {
            consecutiveWins = 0;
        }

        public (int startRow, int startCol, int endRow, int endCol, PieceType promotionPiece)? GetBestMove(Board board, PieceColor aiColor)
        {
            var validMoves = GetAllValidMoves(board, aiColor);
            if (validMoves.Count == 0)
                return null;

            int searchDepth = GetDepthFromDifficulty();
            var result = Minimax(board, searchDepth, int.MinValue, int.MaxValue, true, aiColor, aiColor);

            if (result.bestMove.HasValue)
            {
                string boardHash = GetBoardHash(board);
                if (positionHistory.ContainsKey(boardHash))
                    positionHistory[boardHash] = positionHistory[boardHash] + (result.bestScore > 0 ? 1 : -1);
                else
                    positionHistory[boardHash] = 0;

                return result.bestMove;
            }

            return null;
        }

        private int GetDepthFromDifficulty()
        {
            return currentDifficulty switch
            {
                DifficultyLevel.Newbie => 1,
                DifficultyLevel.Intermediate => 2,
                DifficultyLevel.Good => 3,
                DifficultyLevel.Master => 4,
                _ => 2
            };
        }

        private ((int startRow, int startCol, int endRow, int endCol, PieceType promotionPiece)? bestMove, int bestScore) Minimax(Board board, int depth, int alpha, int beta, bool isMaximizing, PieceColor aiColor, PieceColor currentColor)
        {
            if (depth == 0)
                return (null, EvaluateBoard(board, aiColor));

            var moves = GetAllValidMoves(board, currentColor);
            if (moves.Count == 0)
                return (null, isMaximizing ? int.MinValue : int.MaxValue);

            if (currentDifficulty == DifficultyLevel.Newbie && random.Next(100) < 30)
            {
                var randomMove = moves[random.Next(moves.Count)];
                return ((randomMove.startRow, randomMove.startCol, randomMove.endRow, randomMove.endCol, randomMove.promotionPiece), 0);
            }

            if (isMaximizing)
            {
                int maxScore = int.MinValue;
                (int, int, int, int, PieceType)? bestMove = null;

                foreach (var move in moves)
                {
                    Board newBoard = new Board(board);
                    Piece movedPiece = newBoard.Squares[move.startRow, move.startCol];

                    newBoard.Squares[move.endRow, move.endCol] = movedPiece;
                    newBoard.Squares[move.startRow, move.startCol] = null;
                    movedPiece.HasMoved = true;

                    if (move.promotionPiece != PieceType.None)
                        newBoard.Squares[move.endRow, move.endCol] = new Piece(move.promotionPiece, currentColor);

                    var result = Minimax(newBoard, depth - 1, alpha, beta, false, aiColor, GetOpponentColor(currentColor));
                    int score = result.bestScore;

                    if (score > maxScore)
                    {
                        maxScore = score;
                        bestMove = (move.startRow, move.startCol, move.endRow, move.endCol, move.promotionPiece);
                    }

                    alpha = Math.Max(alpha, score);
                    if (beta <= alpha)
                        break;
                }

                return (bestMove, maxScore);
            }
            else
            {
                int minScore = int.MaxValue;
                (int, int, int, int, PieceType)? bestMove = null;

                foreach (var move in moves)
                {
                    Board newBoard = new Board(board);
                    Piece movedPiece = newBoard.Squares[move.startRow, move.startCol];

                    newBoard.Squares[move.endRow, move.endCol] = movedPiece;
                    newBoard.Squares[move.startRow, move.startCol] = null;
                    movedPiece.HasMoved = true;

                    if (move.promotionPiece != PieceType.None)
                        newBoard.Squares[move.endRow, move.endCol] = new Piece(move.promotionPiece, currentColor);

                    var result = Minimax(newBoard, depth - 1, alpha, beta, true, aiColor, GetOpponentColor(currentColor));
                    int score = result.bestScore;

                    if (score < minScore)
                    {
                        minScore = score;
                        bestMove = (move.startRow, move.startCol, move.endRow, move.endCol, move.promotionPiece);
                    }

                    beta = Math.Min(beta, score);
                    if (beta <= alpha)
                        break;
                }

                return (bestMove, minScore);
            }
        }

        private int EvaluateBoard(Board board, PieceColor aiColor)
        {
            int score = 0;

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board.Squares[row, col];
                    if (piece != null)
                    {
                        int pieceValue = GetPieceValue(piece.Type);
                        int positionBonus = GetPositionBonus(board, piece.Type, piece.Color, row, col);

                        if (piece.Color == aiColor)
                            score += pieceValue + positionBonus;
                        else
                            score -= pieceValue + positionBonus;
                    }
                }
            }

            var validator = new MoveValidator(board);
            if (validator.IsKingInCheck(aiColor) && !validator.HasAnyLegalMove(aiColor))
                score -= 1000;
            if (validator.IsKingInCheck(GetOpponentColor(aiColor)) && !validator.HasAnyLegalMove(GetOpponentColor(aiColor)))
                score += 1000;

            string boardHash = GetBoardHash(board);
            if (positionHistory.ContainsKey(boardHash))
                score += positionHistory[boardHash];

            return score;
        }

        private int GetPositionBonus(Board board, PieceType type, PieceColor color, int row, int col)
        {
            int centerBonus = 0;
            if ((row == 3 || row == 4) && (col == 3 || col == 4))
                centerBonus = 10;
            else if ((row >= 2 && row <= 5) && (col >= 2 && col <= 5))
                centerBonus = 5;

            int pawnBonus = 0;
            if (type == PieceType.Pawn)
            {
                int advance = color == PieceColor.White ? 7 - row : row;
                pawnBonus = advance * 2;
            }

            int developmentBonus = 0;
            if ((type == PieceType.Knight || type == PieceType.Bishop))
            {
                if ((color == PieceColor.White && row < 7) || (color == PieceColor.Black && row > 0))
                    developmentBonus = 5;
                else
                    developmentBonus = -2;
            }

            int rookBonus = 0;
            if (type == PieceType.Rook)
            {
                bool hasPawnsInColumn = false;
                for (int r = 0; r < 8; r++)
                {
                    Piece p = board.Squares[r, col];
                    if (p != null && p.Type == PieceType.Pawn && p.Color == color)
                    {
                        hasPawnsInColumn = true;
                        break;
                    }
                }
                if (!hasPawnsInColumn)
                    rookBonus = 10;
            }

            return centerBonus + pawnBonus + developmentBonus + rookBonus;
        }

        private List<(int startRow, int startCol, int endRow, int endCol, PieceType promotionPiece, Piece movedPiece)> GetAllValidMoves(Board board, PieceColor color)
        {
            var moves = new List<(int, int, int, int, PieceType, Piece)>();
            var validator = new MoveValidator(board);

            for (int startRow = 0; startRow < 8; startRow++)
            {
                for (int startCol = 0; startCol < 8; startCol++)
                {
                    Piece piece = board.Squares[startRow, startCol];
                    if (piece != null && piece.Color == color)
                    {
                        for (int endRow = 0; endRow < 8; endRow++)
                        {
                            for (int endCol = 0; endCol < 8; endCol++)
                            {
                                if (validator.IsValidMove(startRow, startCol, endRow, endCol, color, out PieceType promotionPiece))
                                {
                                    moves.Add((startRow, startCol, endRow, endCol, promotionPiece, piece));
                                }
                            }
                        }
                    }
                }
            }
            return moves;
        }

        private string GetBoardHash(Board board)
        {
            string hash = "";
            for (int row = 0; row < 8; row++)
                for (int col = 0; col < 8; col++)
                {
                    Piece p = board.Squares[row, col];
                    hash += p == null ? "." : $"{p.Type}{p.Color}";
                }
            return hash;
        }

        private PieceColor GetOpponentColor(PieceColor color)
        {
            return color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        }

        private int GetPieceValue(PieceType type)
        {
            return type switch
            {
                PieceType.Pawn => 10,
                PieceType.Knight => 30,
                PieceType.Bishop => 30,
                PieceType.Rook => 50,
                PieceType.Queen => 90,
                PieceType.King => 900,
                _ => 0
            };
        }

        public void SetDifficulty(DifficultyLevel difficulty)
        {
            currentDifficulty = difficulty;
            consecutiveWins = 0; 
        }
    }
}