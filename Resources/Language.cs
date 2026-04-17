using System;
using System.Collections.Generic;

namespace ChessGame.Resources
{
    public static class Language
    {
        private static Dictionary<string, string> messages = new Dictionary<string, string>();
        
        static Language()
        {
            InitializeItalianMessages();
        }
        
        private static void InitializeItalianMessages()
        {
            // Game messages
            messages["WhiteTurn"] = "Turno del Bianco";
            messages["BlackTurn"] = "Turno del Nero";
            messages["WhiteInCheck"] = "Il Bianco è sotto SCACCO!";
            messages["BlackInCheck"] = "Il Nero è sotto SCACCO!";
            messages["Checkmate"] = "SCACCO MATTO! {0} vince!";
            messages["Stalemate"] = "STALLO! La partita è pareggiata!";
            messages["InvalidMove"] = "Mossa non valida!";
            messages["GameOver"] = "Partita terminata!";
            messages["WhiteWins"] = "Bianco";
            messages["BlackWins"] = "Nero";
            messages["Draw"] = "Pareggio!";
            
            // AI messages
            messages["AIThinking"] = "L'IA sta pensando...";
            messages["AITimeout"] = "L'IA ha impiegato troppo tempo!";
            messages["AICheckmateTimeout"] = "L'IA ha impiegato troppo tempo! SCACCO MATTO dichiarato!";
            messages["AIStalemateTimeout"] = "L'IA ha impiegato troppo tempo! STALLO dichiarato!";
            messages["AIVictory"] = "L'IA ha vinto!";
            messages["PlayerVictory"] = "Complimenti! Hai battuto l'IA!";
            messages["AILevelUp"] = "Il livello dell'IA è aumentato a {0}!";
            
            // Difficulty levels
            messages["DifficultyNewbie"] = "Principiante";
            messages["DifficultyIntermediate"] = "Intermedio";
            messages["DifficultyGood"] = "Buono";
            messages["DifficultyMaster"] = "Maestro";
            messages["DifficultyLabel"] = "Difficoltà IA:";
            
            // Button texts
            messages["UndoButton"] = "Annulla (Ctrl+Z)";
            messages["RedoButton"] = "Ripristina (Ctrl+Y)";
            messages["ResetButton"] = "Nuova Partita";
            messages["SaveButton"] = "Salva Partita";
            messages["LoadButton"] = "Carica Partita";
            messages["AIModeOff"] = "Modalità IA: OFF";
            messages["AIModeOn"] = "Modalità IA: ON";
            
            // Save/Load messages
            messages["SaveSuccess"] = "Partita salvata con successo!";
            messages["SaveError"] = "Errore durante il salvataggio: {0}";
            messages["LoadSuccess"] = "Partita caricata con successo!";
            messages["LoadError"] = "Errore durante il caricamento: {0}";
            messages["SaveTitle"] = "Salva Partita";
            messages["LoadTitle"] = "Carica Partita";
            messages["FileFilter"] = "File di Salvataggio Scacchi (*.json)|*.json";
            
            // Undo/Redo messages
            messages["NoMovesToUndo"] = "Nessuna mossa da annullare!";
            messages["NoMovesToRedo"] = "Nessuna mossa da ripristinare!";
            messages["GameReset"] = "Partita resettata! Turno del Bianco";
            
            // Score
            messages["ScoreLabel"] = "Bianco: {0} | Nero: {1}";
            
            // Move history
            messages["WhitePrefix"] = "Bianco";
            messages["BlackPrefix"] = "Nero";
            messages["CaptureSymbol"] = " ✗";
            messages["CastlingSymbol"] = " (0-0)";
            messages["PromotionSymbol"] = " (=D)";
            messages["EnPassantSymbol"] = " (a.p.)";
            
            // Promotion
            messages["PromotionQuestion"] = "Promozione pedone! Scegli il pezzo (D/T/A/C): ";
            messages["PromotionQueen"] = "D";
            messages["PromotionRook"] = "T";
            messages["PromotionBishop"] = "A";
            messages["PromotionKnight"] = "C";
            messages["PromotionDefault"] = "D";
        }
        
        public static string Get(string key, params object[] args)
        {
            if (messages.ContainsKey(key))
            {
                string message = messages[key];
                if (args != null && args.Length > 0)
                {
                    try
                    {
                        return string.Format(message, args);
                    }
                    catch
                    {
                        return message;
                    }
                }
                return message;
            }
            return key;
        }
        
        public static string GetTurnText(PieceColor color, bool inCheck = false)
        {
            if (inCheck)
            {
                return color == PieceColor.White ? Get("WhiteInCheck") : Get("BlackInCheck");
            }
            return color == PieceColor.White ? Get("WhiteTurn") : Get("BlackTurn");
        }
        
        public static string GetDifficultyName(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Newbie => Get("DifficultyNewbie"),
                DifficultyLevel.Intermediate => Get("DifficultyIntermediate"),
                DifficultyLevel.Good => Get("DifficultyGood"),
                DifficultyLevel.Master => Get("DifficultyMaster"),
                _ => Get("DifficultyNewbie")
            };
        }
    }
}