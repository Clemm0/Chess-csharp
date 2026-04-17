using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace ChessGame.Models
{
    [Serializable]
    public class SaveData
    {
        public PieceSerializable[,] Board { get; set; }
        public int? EnPassantTargetRow { get; set; }
        public int? EnPassantTargetCol { get; set; }
        public string CurrentPlayer { get; set; }
        public bool IsAIMode { get; set; }
        public int WhiteScore { get; set; }
        public int BlackScore { get; set; }
        public List<MoveSerializable> MoveHistory { get; set; }
        public DateTime SavedDate { get; set; }
    }

    [Serializable]
    public class PieceSerializable
    {
        public string Type { get; set; }
        public string Color { get; set; }
        public bool HasMoved { get; set; }
    }

    [Serializable]
    public class MoveSerializable
    {
        public int StartRow { get; set; }
        public int StartCol { get; set; }
        public int EndRow { get; set; }
        public int EndCol { get; set; }
        public PieceSerializable MovedPiece { get; set; }
        public PieceSerializable CapturedPiece { get; set; }
        public string PromotionPiece { get; set; }
        public bool IsCastling { get; set; }
        public int RookStartRow { get; set; }
        public int RookStartCol { get; set; }
        public int RookEndRow { get; set; }
        public int RookEndCol { get; set; }
        public bool IsEnPassant { get; set; }
        public int EnPassantPawnRow { get; set; }
        public int EnPassantPawnCol { get; set; }
    }
}