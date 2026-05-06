using System;
using System.Collections.Generic;

namespace ChessGame
{
    public class MoveValidator
    {
        private Board board;

        public MoveValidator(Board board)
        {
            this.board = board;
        }

        public bool IsValidMove(int startRow, int startCol, int endRow, int endCol, PieceColor currentPlayer, out PieceType promotionPiece)
        {
            promotionPiece = PieceType.None;
            Piece piece = board.Squares[startRow, startCol];
            if (piece == null || piece.Color != currentPlayer)
                return false;

            Piece targetPiece = board.Squares[endRow, endCol];
            if (targetPiece != null && targetPiece.Color == currentPlayer)
                return false;

            bool basicMoveValid = piece.Type switch
            {
                PieceType.Pawn => IsValidPawnMove(startRow, startCol, endRow, endCol, piece.Color, targetPiece, out promotionPiece),
                PieceType.Knight => IsValidKnightMove(startRow, startCol, endRow, endCol),
                PieceType.Bishop => IsValidBishopMove(startRow, startCol, endRow, endCol),
                PieceType.Rook => IsValidRookMove(startRow, startCol, endRow, endCol),
                PieceType.Queen => IsValidQueenMove(startRow, startCol, endRow, endCol),
                PieceType.King => IsValidKingMove(startRow, startCol, endRow, endCol, currentPlayer),
                _ => false
            };

            if (!basicMoveValid)
                return false;

            return !WouldBeInCheck(startRow, startCol, endRow, endCol, currentPlayer);
        }

        // ==================== METODI PER I PEDONI ====================

        private bool IsValidPawnMove(int startRow, int startCol, int endRow, int endCol, PieceColor color, Piece target, out PieceType promotionPiece)
        {
            promotionPiece = PieceType.None;
            int direction = color == PieceColor.White ? -1 : 1;
            int startRowBase = color == PieceColor.White ? 6 : 1;

            // 1. AVANTI DI 1
            if (startCol == endCol && endRow == startRow + direction && target == null)
            {
                CheckPromotion(endRow, color, ref promotionPiece);
                return true;
            }

            // 2. AVANTI DI 2
            // NOTE: En passant target is set in ChessForm.ExecuteMove after the move is confirmed,
            // NOT here. Setting it here corrupts the state because IsValidPawnMove is called many
            // times during highlighting, AI evaluation, and check detection.
            if (startCol == endCol && endRow == startRow + 2 * direction && target == null && startRow == startRowBase)
            {
                int middleRow = startRow + direction;
                if (board.Squares[middleRow, startCol] == null)
                    return true;
                return false;
            }

            // 3. CATTURA DIAGONALE NORMALE
            if (Math.Abs(endCol - startCol) == 1 && endRow == startRow + direction && target != null && target.Color != color)
            {
                CheckPromotion(endRow, color, ref promotionPiece);
                return true;
            }

            // 4. EN PASSANT - CATTURA
            if (Math.Abs(endCol - startCol) == 1 && endRow == startRow + direction && target == null)
            {
                // Verifica se c'è un en passant disponibile
                if (board.EnPassantRow.HasValue && board.EnPassantCol.HasValue)
                {
                    // Il pedone deve muoversi sulla casella dove c'è l'en passant
                    if (endRow == board.EnPassantRow.Value && endCol == board.EnPassantCol.Value)
                    {
                        // Verifica che ci sia un pedone avversario nella casella dietro
                        int opponentRow = endRow - direction;
                        if (opponentRow >= 0 && opponentRow < 8)
                        {
                            Piece opponentPawn = board.Squares[opponentRow, endCol];
                            if (opponentPawn != null && opponentPawn.Type == PieceType.Pawn && opponentPawn.Color != color)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private void CheckPromotion(int endRow, PieceColor color, ref PieceType promotionPiece)
        {
            if ((color == PieceColor.White && endRow == 0) || (color == PieceColor.Black && endRow == 7))
            {
                promotionPiece = PieceType.Queen; // Default, poi l'utente sceglie
            }
        }

        public List<(int row, int col)> GetValidPawnMoves(int startRow, int startCol, PieceColor color)
        {
            var validMoves = new List<(int, int)>();
            int direction = color == PieceColor.White ? -1 : 1;
            int startRowBase = color == PieceColor.White ? 6 : 1;

            // Avanti di 1
            int newRow = startRow + direction;
            if (newRow >= 0 && newRow < 8 && board.Squares[newRow, startCol] == null)
            {
                validMoves.Add((newRow, startCol));

                // Avanti di 2
                int newRow2 = startRow + 2 * direction;
                if (startRow == startRowBase && newRow2 >= 0 && newRow2 < 8 && board.Squares[newRow2, startCol] == null)
                {
                    validMoves.Add((newRow2, startCol));
                }
            }

            // Catture diagonali
            for (int colOffset = -1; colOffset <= 1; colOffset += 2)
            {
                int newCol = startCol + colOffset;
                if (newCol >= 0 && newCol < 8)
                {
                    newRow = startRow + direction;
                    if (newRow >= 0 && newRow < 8)
                    {
                        Piece target = board.Squares[newRow, newCol];
                        if (target != null && target.Color != color)
                        {
                            validMoves.Add((newRow, newCol));
                        }
                    }
                }
            }

            // EN PASSANT - Aggiungi come mossa valida
            if (board.EnPassantRow.HasValue && board.EnPassantCol.HasValue)
            {
                int enRow = board.EnPassantRow.Value;
                int enCol = board.EnPassantCol.Value;

                // Il pedone deve essere sulla riga giusta e colonna adiacente
                if (Math.Abs(enCol - startCol) == 1 && enRow == startRow + direction)
                {
                    validMoves.Add((enRow, enCol));
                }
            }

            return validMoves;
        }

        // ==================== METODI PER GLI ALTRI PEZZI ====================

        private bool IsValidKnightMove(int startRow, int startCol, int endRow, int endCol)
        {
            int deltaRow = Math.Abs(endRow - startRow);
            int deltaCol = Math.Abs(endCol - startCol);
            return (deltaRow == 2 && deltaCol == 1) || (deltaRow == 1 && deltaCol == 2);
        }

        private bool IsValidBishopMove(int startRow, int startCol, int endRow, int endCol)
        {
            if (Math.Abs(endRow - startRow) != Math.Abs(endCol - startCol))
                return false;
            return IsClearDiagonal(startRow, startCol, endRow, endCol);
        }

        private bool IsValidRookMove(int startRow, int startCol, int endRow, int endCol)
        {
            if (startRow != endRow && startCol != endCol)
                return false;
            return IsClearStraight(startRow, startCol, endRow, endCol);
        }

        private bool IsValidQueenMove(int startRow, int startCol, int endRow, int endCol)
        {
            return IsValidBishopMove(startRow, startCol, endRow, endCol) ||
                   IsValidRookMove(startRow, startCol, endRow, endCol);
        }

        private bool IsValidKingMove(int startRow, int startCol, int endRow, int endCol, PieceColor color)
        {
            int deltaRow = Math.Abs(endRow - startRow);
            int deltaCol = Math.Abs(endCol - startCol);

            // Movimento normale del re
            if (deltaRow <= 1 && deltaCol <= 1)
                return true;

            // ARROCCO
            if (deltaRow == 0 && deltaCol == 2 && !board.Squares[startRow, startCol].HasMoved)
            {
                int rookCol = endCol > startCol ? 7 : 0;
                Piece rook = board.Squares[startRow, rookCol];
                if (rook != null && rook.Type == PieceType.Rook && !rook.HasMoved)
                {
                    int step = endCol > startCol ? 1 : -1;
                    for (int col = startCol + step; col != rookCol; col += step)
                    {
                        if (board.Squares[startRow, col] != null)
                            return false;
                    }

                    if (IsSquareUnderAttack(startRow, startCol, color) ||
                        IsSquareUnderAttack(startRow, startCol + step, color))
                        return false;

                    return true;
                }
            }

            return false;
        }

        // ==================== METODI DI UTILITÀ ====================

        private bool IsClearDiagonal(int startRow, int startCol, int endRow, int endCol)
        {
            int rowStep = Math.Sign(endRow - startRow);
            int colStep = Math.Sign(endCol - startCol);

            int currentRow = startRow + rowStep;
            int currentCol = startCol + colStep;

            while (currentRow != endRow || currentCol != endCol)
            {
                if (board.Squares[currentRow, currentCol] != null)
                    return false;
                currentRow += rowStep;
                currentCol += colStep;
            }
            return true;
        }

        private bool IsClearStraight(int startRow, int startCol, int endRow, int endCol)
        {
            if (startRow == endRow)
            {
                int step = Math.Sign(endCol - startCol);
                for (int col = startCol + step; col != endCol; col += step)
                {
                    if (board.Squares[startRow, col] != null)
                        return false;
                }
            }
            else
            {
                int step = Math.Sign(endRow - startRow);
                for (int row = startRow + step; row != endRow; row += step)
                {
                    if (board.Squares[row, startCol] != null)
                        return false;
                }
            }
            return true;
        }

        private bool WouldBeInCheck(int startRow, int startCol, int endRow, int endCol, PieceColor color)
        {
            Board tempBoard = new Board(board);
            Piece movedPiece = tempBoard.Squares[startRow, startCol];
            Piece capturedPiece = tempBoard.Squares[endRow, endCol];

            tempBoard.Squares[endRow, endCol] = movedPiece;
            tempBoard.Squares[startRow, startCol] = null;

            // Gestione en passant nella simulazione - VERSIONE CORRETTA
            if (movedPiece.Type == PieceType.Pawn && Math.Abs(endCol - startCol) == 1 && capturedPiece == null)
            {
                if (board.IsEnPassantAvailable && board.EnPassantRow.HasValue && board.EnPassantCol.HasValue)
                {
                    if (endRow == board.EnPassantRow.Value && endCol == board.EnPassantCol.Value)
                    {
                        int direction = color == PieceColor.White ? -1 : 1;
                        int capturedPawnRow = endRow - direction;
                        if (capturedPawnRow >= 0 && capturedPawnRow < 8)
                        {
                            tempBoard.Squares[capturedPawnRow, endCol] = null;
                        }
                    }
                }
            }

            // Gestione arrocco nella simulazione
            if (movedPiece.Type == PieceType.King && Math.Abs(endCol - startCol) == 2)
            {
                int rookStartCol = endCol > startCol ? 7 : 0;
                int rookEndCol = endCol > startCol ? 5 : 3;
                Piece rook = tempBoard.Squares[startRow, rookStartCol];
                if (rook != null)
                {
                    tempBoard.Squares[startRow, rookEndCol] = rook;
                    tempBoard.Squares[startRow, rookStartCol] = null;
                }
            }

            MoveValidator tempValidator = new MoveValidator(tempBoard);
            return tempValidator.IsKingInCheck(color);
        }
        public bool IsKingInCheck(PieceColor color)
        {
            int kingRow = -1, kingCol = -1;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board.Squares[row, col];
                    if (piece != null && piece.Type == PieceType.King && piece.Color == color)
                    {
                        kingRow = row;
                        kingCol = col;
                        break;
                    }
                }
            }

            PieceColor opponent = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board.Squares[row, col];
                    if (piece != null && piece.Color == opponent)
                    {
                        if (IsValidMoveWithoutCheck(row, col, kingRow, kingCol, opponent))
                            return true;
                    }
                }
            }
            return false;
        }

        private bool IsSquareUnderAttack(int row, int col, PieceColor defendingColor)
        {
            PieceColor attackingColor = defendingColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    Piece piece = board.Squares[r, c];
                    if (piece != null && piece.Color == attackingColor)
                    {
                        if (IsValidMoveWithoutCheck(r, c, row, col, attackingColor))
                            return true;
                    }
                }
            }
            return false;
        }

        public bool IsValidMoveWithoutCheck(int startRow, int startCol, int endRow, int endCol, PieceColor currentPlayer)
        {
            Piece piece = board.Squares[startRow, startCol];
            if (piece == null || piece.Color != currentPlayer)
                return false;

            Piece targetPiece = board.Squares[endRow, endCol];
            if (targetPiece != null && targetPiece.Color == currentPlayer)
                return false;

            return piece.Type switch
            {
                PieceType.Pawn => IsValidPawnMove(startRow, startCol, endRow, endCol, piece.Color, targetPiece, out _),
                PieceType.Knight => IsValidKnightMove(startRow, startCol, endRow, endCol),
                PieceType.Bishop => IsValidBishopMove(startRow, startCol, endRow, endCol),
                PieceType.Rook => IsValidRookMove(startRow, startCol, endRow, endCol),
                PieceType.Queen => IsValidQueenMove(startRow, startCol, endRow, endCol),
                PieceType.King => IsValidKingMove(startRow, startCol, endRow, endCol, currentPlayer),
                _ => false
            };
        }

        public List<(int row, int col)> GetValidMovesForPiece(int row, int col, PieceColor color)
        {
            var validMoves = new List<(int, int)>();
            Piece piece = board.Squares[row, col];

            if (piece == null || piece.Color != color)
                return validMoves;

            for (int endRow = 0; endRow < 8; endRow++)
            {
                for (int endCol = 0; endCol < 8; endCol++)
                {
                    if (IsValidMove(row, col, endRow, endCol, color, out _))
                    {
                        validMoves.Add((endRow, endCol));
                    }
                }
            }

            return validMoves;
        }

        public bool HasAnyLegalMove(PieceColor color)
        {
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
                                if (IsValidMove(startRow, startCol, endRow, endCol, color, out _))
                                    return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}