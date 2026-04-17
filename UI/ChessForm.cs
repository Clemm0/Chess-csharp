using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using ChessGame.Resources;

namespace ChessGame
{
    public class ChessForm : Form
    {
        private Board board;
        private MoveValidator validator;
        private AIOpponent ai;
        private PieceColor currentPlayer;
        private bool gameOver;
        private bool isAIMode;
        private bool isAIThinking;
        private System.Windows.Forms.Timer aiThinkingTimer;
        private Button[,] squareButtons;
        private int? selectedRow, selectedCol;
        private List<Move> moveHistory;
        private List<Move> undoneMoves;
        private Label statusLabel;
        private Button undoButton;
        private Button redoButton;
        private Button resetButton;
        private Button aiModeButton;
        private Button saveButton;
        private Button loadButton;
        private ComboBox difficultyComboBox;
        private Panel rightPanel;
        private RichTextBox moveHistoryBox;
        private TableLayoutPanel gamePanel;
        private Label scoreLabel;
        private Label difficultyLabel;
        private int whiteScore = 0;
        private int blackScore = 0;
        private List<(int row, int col)> currentValidMoves;
        private int aiThinkingTime = 0;

        public ChessForm()
        {
            this.TopMost = false;
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            
            this.Text = "Scacchi";
            this.KeyPreview = true;
            this.KeyDown += ChessForm_KeyDown;
            
            InitializeForm();
            NewGame();
            
            // Load icon from Images folder
            LoadIconFromFile();
            
            this.Show();
            this.BringToFront();
            this.Focus();
        }

        private void LoadIconFromFile()
        {
            try
            {
                // Check multiple possible paths for the icon
                string[] possiblePaths = new string[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "icon.ico"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "icon.png"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.ico"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.png"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Images", "icon.ico"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Images", "icon.png"),
                };
                
                foreach (string iconPath in possiblePaths)
                {
                    if (File.Exists(iconPath))
                    {
                        if (iconPath.EndsWith(".ico"))
                        {
                            this.Icon = new Icon(iconPath);
                            break;
                        }
                        else if (iconPath.EndsWith(".png"))
                        {
                            using (Bitmap bmp = new Bitmap(iconPath))
                            {
                                this.Icon = Icon.FromHandle(bmp.GetHicon());
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void ChessForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Z)
            {
                UndoLastMove();
                e.Handled = true;
            }
            else if (e.Control && e.KeyCode == Keys.Y)
            {
                RedoLastMove();
                e.Handled = true;
            }
        }

        private void InitializeForm()
        {
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.MinimumSize = new Size(900, 650);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            
            gamePanel = new TableLayoutPanel
            {
                Size = new Size(640, 640),
                Location = new Point(20, 20),
                BackColor = Color.Transparent
            };
            
            rightPanel = new Panel
            {
                Location = new Point(680, 20),
                Size = new Size(380, 690),
                BackColor = Color.FromArgb(45, 45, 45),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            statusLabel = new Label
            {
                Text = "Turno del Bianco",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                Size = new Size(340, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(68, 68, 68),
                FlatStyle = FlatStyle.Flat
            };
            
            scoreLabel = new Label
            {
                Text = "Bianco: 0 | Nero: 0",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Gold,
                Location = new Point(20, 80),
                Size = new Size(340, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(68, 68, 68)
            };
            
            difficultyLabel = new Label
            {
                Text = "Difficoltà IA:",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.Cyan,
                Location = new Point(20, 125),
                Size = new Size(100, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            
            difficultyComboBox = new ComboBox
            {
                Location = new Point(120, 125),
                Size = new Size(220, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.FromArgb(68, 68, 68),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            difficultyComboBox.Items.AddRange(new string[] { "Principiante", "Intermedio", "Buono", "Maestro" });
            difficultyComboBox.SelectedIndex = 0;
            difficultyComboBox.SelectedIndexChanged += DifficultyComboBox_SelectedIndexChanged;
            
            undoButton = new Button
            {
                Text = "↩ Annulla (Ctrl+Z)",
                Location = new Point(40, 170),
                Size = new Size(140, 45),
                BackColor = Color.FromArgb(255, 107, 107),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            undoButton.FlatAppearance.BorderSize = 0;
            undoButton.Click += UndoButton_Click;
            
            redoButton = new Button
            {
                Text = "⟳ Ripristina (Ctrl+Y)",
                Location = new Point(190, 170),
                Size = new Size(150, 45),
                BackColor = Color.FromArgb(100, 181, 246),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            redoButton.FlatAppearance.BorderSize = 0;
            redoButton.Click += RedoButton_Click;
            
            resetButton = new Button
            {
                Text = "⟳ Nuova Partita",
                Location = new Point(40, 230),
                Size = new Size(140, 45),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            resetButton.FlatAppearance.BorderSize = 0;
            resetButton.Click += ResetButton_Click;
            
            saveButton = new Button
            {
                Text = "💾 Salva Partita",
                Location = new Point(190, 230),
                Size = new Size(150, 45),
                BackColor = Color.FromArgb(33, 150, 243),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.Click += SaveButton_Click;
            
            loadButton = new Button
            {
                Text = "📂 Carica Partita",
                Location = new Point(40, 290),
                Size = new Size(300, 45),
                BackColor = Color.FromArgb(156, 39, 176),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            loadButton.FlatAppearance.BorderSize = 0;
            loadButton.Click += LoadButton_Click;
            
            aiModeButton = new Button
            {
                Text = "🤖 Modalità IA: OFF",
                Location = new Point(40, 350),
                Size = new Size(300, 45),
                BackColor = Color.FromArgb(255, 152, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            aiModeButton.FlatAppearance.BorderSize = 0;
            aiModeButton.Click += AIModeButton_Click;
            
            moveHistoryBox = new RichTextBox
            {
                Location = new Point(40, 410),
                Size = new Size(300, 250),
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.FromArgb(200, 200, 200),
                BorderStyle = BorderStyle.None
            };
            
            rightPanel.Controls.AddRange(new Control[] { 
                statusLabel, scoreLabel, difficultyLabel, difficultyComboBox,
                undoButton, redoButton, resetButton, saveButton, loadButton, 
                aiModeButton, moveHistoryBox 
            });
            
            this.Controls.Add(gamePanel);
            this.Controls.Add(rightPanel);
            
            CreateBoardButtons();
        }

        private void DifficultyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ai != null)
            {
                DifficultyLevel newLevel = (DifficultyLevel)difficultyComboBox.SelectedIndex;
                ai.SetDifficulty(newLevel);
            }
        }

        private void CreateBoardButtons()
        {
            squareButtons = new Button[8, 8];

            gamePanel.Controls.Clear();
            gamePanel.RowCount = 8;
            gamePanel.ColumnCount = 8;
            gamePanel.RowStyles.Clear();
            gamePanel.ColumnStyles.Clear();

            for (int i = 0; i < 8; i++)
            {
                gamePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));
                gamePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12.5f));
            }

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Button btn = new Button
                    {
                        Dock = DockStyle.Fill,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font("Segoe UI", 28, FontStyle.Bold),
                        Cursor = Cursors.Hand
                    };
                    
                    int capturedRow = row;
                    int capturedCol = col;
                    btn.Click += (s, e) => SquareButton_Click(capturedRow, capturedCol);
                    btn.FlatAppearance.BorderSize = 0;
                    
                    squareButtons[row, col] = btn;
                    gamePanel.Controls.Add(btn, col, row);
                }
            }
        }

        private void NewGame()
        {
            board = new Board();
            validator = new MoveValidator(board);
            ai = new AIOpponent();
            currentPlayer = PieceColor.White;
            gameOver = false;
            isAIThinking = false;
            selectedRow = null;
            selectedCol = null;
            moveHistory = new List<Move>();
            undoneMoves = new List<Move>();
            whiteScore = 0;
            blackScore = 0;
            isAIMode = false;
            currentValidMoves = null;
            board.ClearEnPassant();
            UpdateBoardDisplay();
            UpdateStatus();
            UpdateMoveHistory();
            UpdateScore();
        }

        private void UpdateBoardDisplay()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Button btn = squareButtons[row, col];
                    Piece piece = board.Squares[row, col];
                    
                    if (piece != null)
                    {
                        btn.Text = GetPieceUnicode(piece);
                        if (piece.Color == PieceColor.White)
                        {
                            btn.ForeColor = Color.Black;
                            btn.BackColor = GetSquareColor(row, col);
                            btn.FlatAppearance.BorderSize = 2;
                            btn.FlatAppearance.BorderColor = Color.Black;
                        }
                        else
                        {
                            btn.ForeColor = Color.Black;
                            btn.BackColor = GetSquareColor(row, col);
                            btn.FlatAppearance.BorderSize = 1;
                            btn.FlatAppearance.BorderColor = Color.DarkGray;
                        }
                    }
                    else
                    {
                        btn.Text = "";
                        btn.BackColor = GetSquareColor(row, col);
                        btn.FlatAppearance.BorderSize = 0;
                    }

                    if (selectedRow == row && selectedCol == col)
                    {
                        btn.BackColor = Color.FromArgb(255, 235, 59);
                    }
                    
                    if (currentValidMoves != null && currentValidMoves.Contains((row, col)))
                    {
                        btn.BackColor = Color.FromArgb(76, 175, 80);
                    }
                }
            }
        }

        private string GetPieceUnicode(Piece piece)
        {
            return (piece.Color, piece.Type) switch
            {
                (PieceColor.White, PieceType.King) => "♔",
                (PieceColor.White, PieceType.Queen) => "♕",
                (PieceColor.White, PieceType.Rook) => "♖",
                (PieceColor.White, PieceType.Bishop) => "♗",
                (PieceColor.White, PieceType.Knight) => "♘",
                (PieceColor.White, PieceType.Pawn) => "♙",
                (PieceColor.Black, PieceType.King) => "♚",
                (PieceColor.Black, PieceType.Queen) => "♛",
                (PieceColor.Black, PieceType.Rook) => "♜",
                (PieceColor.Black, PieceType.Bishop) => "♝",
                (PieceColor.Black, PieceType.Knight) => "♞",
                (PieceColor.Black, PieceType.Pawn) => "♟",
                _ => ""
            };
        }

        private Color GetSquareColor(int row, int col)
        {
            if ((row + col) % 2 == 0)
                return Color.FromArgb(240, 217, 181);
            else
                return Color.FromArgb(181, 136, 99);
        }

        private void SquareButton_Click(int row, int col)
        {
            if (gameOver || isAIThinking) return;

            if (selectedRow == null)
            {
                Piece piece = board.Squares[row, col];
                if (piece != null && piece.Color == currentPlayer)
                {
                    selectedRow = row;
                    selectedCol = col;
                    currentValidMoves = validator.GetValidMovesForPiece(row, col, currentPlayer);
                    UpdateBoardDisplay();
                }
            }
            else
            {
                if (selectedRow == row && selectedCol == col)
                {
                    selectedRow = null;
                    selectedCol = null;
                    currentValidMoves = null;
                    UpdateBoardDisplay();
                    return;
                }
                
                Piece clickedPiece = board.Squares[row, col];
                if (clickedPiece != null && clickedPiece.Color == currentPlayer)
                {
                    selectedRow = row;
                    selectedCol = col;
                    currentValidMoves = validator.GetValidMovesForPiece(row, col, currentPlayer);
                    UpdateBoardDisplay();
                    return;
                }
                
                if (validator.IsValidMove(selectedRow.Value, selectedCol.Value, row, col, currentPlayer, out PieceType promotionPiece))
                {
                    ExecuteMove(selectedRow.Value, selectedCol.Value, row, col, promotionPiece);
                    selectedRow = null;
                    selectedCol = null;
                    currentValidMoves = null;
                    
                    if (isAIMode && !gameOver && currentPlayer == PieceColor.Black)
                    {
                        StartAIMove();
                    }
                }
                else
                {
                    selectedRow = null;
                    selectedCol = null;
                    currentValidMoves = null;
                    UpdateBoardDisplay();
                    statusLabel.Text = "Mossa non valida!";
                    statusLabel.ForeColor = Color.Red;
                    System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
                    timer.Interval = 1000;
                    timer.Tick += (s, args) => { UpdateStatus(); timer.Stop(); };
                    timer.Start();
                }
            }
        }

        private void StartAIMove()
        {
            isAIThinking = true;
            aiThinkingTime = 0;
            statusLabel.Text = "🤖 L'IA sta pensando...";
            UpdateBoardDisplay();
            
            aiThinkingTimer = new System.Windows.Forms.Timer();
            aiThinkingTimer.Interval = 1000;
            aiThinkingTimer.Tick += AiThinkingTimer_Tick;
            aiThinkingTimer.Start();
            
            System.Windows.Forms.Timer moveTimer = new System.Windows.Forms.Timer();
            moveTimer.Interval = 100;
            moveTimer.Tick += (s, args) => 
            {
                moveTimer.Stop();
                MakeAIMove();
            };
            moveTimer.Start();
        }
        
        private void AiThinkingTimer_Tick(object sender, EventArgs e)
        {
            aiThinkingTime++;
            statusLabel.Text = $"🤖 L'IA sta pensando... ({aiThinkingTime}s)";
            
            if (aiThinkingTime >= 15)
            {
                aiThinkingTimer.Stop();
                if (isAIThinking)
                {
                    isAIThinking = false;
                    bool isCheck = validator.IsKingInCheck(PieceColor.Black);
                    bool hasMoves = validator.HasAnyLegalMove(PieceColor.Black);
                    
                    if (!hasMoves)
                    {
                        gameOver = true;
                        if (isCheck)
                        {
                            statusLabel.Text = "SCACCO MATTO! Bianco vince! (IA timeout)";
                            MessageBox.Show("L'IA ha impiegato troppo tempo! SCACCO MATTO dichiarato!", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            statusLabel.Text = "STALLO! Partita patta! (IA timeout)";
                            MessageBox.Show("L'IA ha impiegato troppo tempo! STALLO dichiarato!", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        statusLabel.Text = "L'IA sta impiegando troppo tempo, mossa casuale...";
                        Application.DoEvents();
                        MakeRandomAIMove();
                    }
                }
            }
        }
        
        private void MakeRandomAIMove()
        {
            var validMoves = GetAllValidMovesForColor(PieceColor.Black);
            if (validMoves.Count > 0)
            {
                Random rand = new Random();
                var move = validMoves[rand.Next(validMoves.Count)];
                ExecuteMove(move.startRow, move.startCol, move.endRow, move.endCol, move.promotionPiece);
            }
            isAIThinking = false;
        }
        
        private List<(int startRow, int startCol, int endRow, int endCol, PieceType promotionPiece)> GetAllValidMovesForColor(PieceColor color)
        {
            var moves = new List<(int, int, int, int, PieceType)>();
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
                                    moves.Add((startRow, startCol, endRow, endCol, promotionPiece));
                                }
                            }
                        }
                    }
                }
            }
            return moves;
        }

        private void ExecuteMove(int startRow, int startCol, int endRow, int endCol, PieceType promotionPiece)
        {
            Piece movedPiece = board.Squares[startRow, startCol];
            Piece capturedPiece = board.Squares[endRow, endCol];
            
            if (capturedPiece != null)
            {
                int pieceValue = GetPieceValue(capturedPiece.Type);
                if (currentPlayer == PieceColor.White)
                    whiteScore += pieceValue;
                else
                    blackScore += pieceValue;
                UpdateScore();
            }
            
            Move move = new Move(startRow, startCol, endRow, endCol, movedPiece, capturedPiece);
            
            // EN PASSANT FIX - Complete rewrite
            if (movedPiece.Type == PieceType.Pawn && Math.Abs(endCol - startCol) == 1 && capturedPiece == null)
            {
                // Check if there's an en passant target
                if (board.EnPassantTargetRow.HasValue && board.EnPassantTargetCol.HasValue)
                {
                    // The pawn must capture by moving to the en passant target square
                    if (endRow == board.EnPassantTargetRow.Value && endCol == board.EnPassantTargetCol.Value)
                    {
                        // Find the opponent pawn that just moved two squares
                        int direction = currentPlayer == PieceColor.White ? -1 : 1;
                        int opponentPawnRow = endRow - direction;
                        
                        if (opponentPawnRow >= 0 && opponentPawnRow < 8)
                        {
                            Piece opponentPawn = board.Squares[opponentPawnRow, endCol];
                            if (opponentPawn != null && opponentPawn.Type == PieceType.Pawn && opponentPawn.Color != currentPlayer)
                            {
                                move.IsEnPassant = true;
                                move.EnPassantPawnRow = opponentPawnRow;
                                move.EnPassantPawnCol = endCol;
                                // Remove the captured pawn
                                board.Squares[opponentPawnRow, endCol] = null;
                                
                                // Add score for captured pawn
                                if (currentPlayer == PieceColor.White)
                                    whiteScore += 10;
                                else
                                    blackScore += 10;
                                UpdateScore();
                            }
                        }
                    }
                }
            }
            
            // Execute move
            board.Squares[endRow, endCol] = movedPiece;
            board.Squares[startRow, startCol] = null;
            movedPiece.HasMoved = true;
            
            // Handle castling
            if (movedPiece.Type == PieceType.King && Math.Abs(endCol - startCol) == 2)
            {
                move.IsCastling = true;
                int rookStartCol = endCol > startCol ? 7 : 0;
                int rookEndCol = endCol > startCol ? 5 : 3;
                Piece rook = board.Squares[startRow, rookStartCol];
                if (rook != null)
                {
                    move.RookStartRow = startRow;
                    move.RookStartCol = rookStartCol;
                    move.RookEndRow = startRow;
                    move.RookEndCol = rookEndCol;
                    board.Squares[startRow, rookEndCol] = rook;
                    board.Squares[startRow, rookStartCol] = null;
                    rook.HasMoved = true;
                }
            }
            
            // Handle pawn promotion with user choice
            if (movedPiece.Type == PieceType.Pawn && (endRow == 0 || endRow == 7))
            {
                PieceType selectedPromotion = ShowPromotionDialog();
                move.PromotionPiece = selectedPromotion;
                board.Squares[endRow, endCol] = new Piece(selectedPromotion, currentPlayer);
            }
            else if (promotionPiece != PieceType.None)
            {
                move.PromotionPiece = promotionPiece;
                board.Squares[endRow, endCol] = new Piece(promotionPiece, currentPlayer);
            }
            
            moveHistory.Add(move);
            undoneMoves.Clear();
            
            // Clear en passant target AFTER the move (ghost pawn disappears)
            board.ClearEnPassant();
            
            UpdateBoardDisplay();
            
            // Check for checkmate
            bool isCheck = validator.IsKingInCheck(currentPlayer);
            bool hasMoves = validator.HasAnyLegalMove(currentPlayer);
            
            if (isCheck && !hasMoves)
            {
                gameOver = true;
                string winner = currentPlayer == PieceColor.White ? "Nero" : "Bianco";
                statusLabel.Text = $"SCACCO MATTO! {winner} vince!";
                MessageBox.Show($"SCACCO MATTO! {winner} vince!", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (!hasMoves)
            {
                gameOver = true;
                statusLabel.Text = "STALLO! Partita patta!";
                MessageBox.Show("STALLO! Partita patta!", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                currentPlayer = currentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
                UpdateStatus();
            }
            
            UpdateMoveHistory();
        }
        
        private PieceType ShowPromotionDialog()
        {
            Form promoDialog = new Form
            {
                Text = "Promozione Pedone",
                Size = new Size(450, 280),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(45, 45, 45)
            };
            
            Label label = new Label
            {
                Text = "Scegli il pezzo per la promozione:",
                Location = new Point(20, 20),
                Size = new Size(410, 35),
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            
            Panel buttonPanel = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(410, 150),
                BackColor = Color.Transparent
            };
            
            Button queenBtn = new Button
            {
                Text = "♕ Regina",
                Location = new Point(10, 10),
                Size = new Size(180, 60),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                BackColor = Color.FromArgb(68, 68, 68),
                ForeColor = Color.Gold,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            
            Button rookBtn = new Button
            {
                Text = "♖ Torre",
                Location = new Point(210, 10),
                Size = new Size(180, 60),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                BackColor = Color.FromArgb(68, 68, 68),
                ForeColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            
            Button bishopBtn = new Button
            {
                Text = "♗ Alfiere",
                Location = new Point(10, 80),
                Size = new Size(180, 60),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                BackColor = Color.FromArgb(68, 68, 68),
                ForeColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            
            Button knightBtn = new Button
            {
                Text = "♘ Cavallo",
                Location = new Point(210, 80),
                Size = new Size(180, 60),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                BackColor = Color.FromArgb(68, 68, 68),
                ForeColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            
            queenBtn.FlatAppearance.BorderSize = 1;
            queenBtn.FlatAppearance.BorderColor = Color.Gold;
            rookBtn.FlatAppearance.BorderSize = 1;
            rookBtn.FlatAppearance.BorderColor = Color.Gray;
            bishopBtn.FlatAppearance.BorderSize = 1;
            bishopBtn.FlatAppearance.BorderColor = Color.Gray;
            knightBtn.FlatAppearance.BorderSize = 1;
            knightBtn.FlatAppearance.BorderColor = Color.Gray;
            
            PieceType selectedPiece = PieceType.Queen;
            
            queenBtn.Click += (s, e) => { selectedPiece = PieceType.Queen; promoDialog.Close(); };
            rookBtn.Click += (s, e) => { selectedPiece = PieceType.Rook; promoDialog.Close(); };
            bishopBtn.Click += (s, e) => { selectedPiece = PieceType.Bishop; promoDialog.Close(); };
            knightBtn.Click += (s, e) => { selectedPiece = PieceType.Knight; promoDialog.Close(); };
            
            buttonPanel.Controls.AddRange(new Control[] { queenBtn, rookBtn, bishopBtn, knightBtn });
            promoDialog.Controls.Add(label);
            promoDialog.Controls.Add(buttonPanel);
            promoDialog.ShowDialog();
            
            return selectedPiece;
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
                _ => 0
            };
        }
        
        private void UpdateScore()
        {
            scoreLabel.Text = $"Bianco: {whiteScore} | Nero: {blackScore}";
        }

        private void MakeAIMove()
        {
            if (gameOver || currentPlayer != PieceColor.Black || !isAIMode) 
            {
                isAIThinking = false;
                if (aiThinkingTimer != null) aiThinkingTimer.Stop();
                return;
            }
            
            var bestMove = ai.GetBestMove(board, PieceColor.Black);
            
            isAIThinking = false;
            if (aiThinkingTimer != null) aiThinkingTimer.Stop();
            
            if (bestMove.HasValue)
            {
                var (startRow, startCol, endRow, endCol, promotionPiece) = bestMove.Value;
                ExecuteMove(startRow, startCol, endRow, endCol, promotionPiece);
            }
            else if (!validator.HasAnyLegalMove(PieceColor.Black))
            {
                gameOver = true;
                statusLabel.Text = "SCACCO MATTO! Bianco vince!";
                MessageBox.Show("SCACCO MATTO! Bianco vince!", "Game Over", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void UndoLastMove()
        {
            if (isAIThinking) return;
            
            int movesToUndo = isAIMode ? 2 : 1;
            
            for (int i = 0; i < movesToUndo && moveHistory.Count > 0; i++)
            {
                Move lastMove = moveHistory[moveHistory.Count - 1];
                moveHistory.RemoveAt(moveHistory.Count - 1);
                undoneMoves.Add(lastMove);
            }
            
            ResetBoardFromHistory();
            
            if (isAIMode && movesToUndo == 2)
            {
                currentPlayer = PieceColor.White;
            }
            else
            {
                currentPlayer = currentPlayer == PieceColor.White ? PieceColor.Black : PieceColor.White;
            }
            
            selectedRow = null;
            selectedCol = null;
            gameOver = false;
            currentValidMoves = null;
            
            UpdateBoardDisplay();
            UpdateStatus();
            UpdateMoveHistory();
        }
        
        private void RedoLastMove()
        {
            if (isAIThinking) return;
            
            if (undoneMoves.Count == 0)
            {
                statusLabel.Text = "Nessuna mossa da ripristinare!";
                return;
            }
            
            List<Move> movesToRedo = new List<Move>(undoneMoves);
            undoneMoves.Clear();
            
            foreach (var move in movesToRedo)
            {
                currentPlayer = move.MovedPiece.Color;
                ExecuteMove(move.StartRow, move.StartCol, move.EndRow, move.EndCol, move.PromotionPiece);
            }
        }
        
        private void UndoButton_Click(object sender, EventArgs e)
        {
            UndoLastMove();
        }
        
        private void RedoButton_Click(object sender, EventArgs e)
        {
            RedoLastMove();
        }
        
        private void ResetBoardFromHistory()
        {
            board = new Board();
            validator = new MoveValidator(board);
            whiteScore = 0;
            blackScore = 0;
            
            foreach (var move in moveHistory)
            {
                Piece movedPiece = board.Squares[move.StartRow, move.StartCol];
                board.Squares[move.EndRow, move.EndCol] = movedPiece;
                board.Squares[move.StartRow, move.StartCol] = null;
                movedPiece.HasMoved = true;
                
                if (move.CapturedPiece != null)
                {
                    if (move.MovedPiece.Color == PieceColor.White)
                        whiteScore += GetPieceValue(move.CapturedPiece.Type);
                    else
                        blackScore += GetPieceValue(move.CapturedPiece.Type);
                }
                
                if (move.IsCastling)
                {
                    Piece rook = board.Squares[move.RookStartRow, move.RookStartCol];
                    if (rook != null)
                    {
                        board.Squares[move.RookEndRow, move.RookEndCol] = rook;
                        board.Squares[move.RookStartRow, move.RookStartCol] = null;
                        rook.HasMoved = true;
                    }
                }
                
                if (move.PromotionPiece != PieceType.None)
                {
                    board.Squares[move.EndRow, move.EndCol] = new Piece(move.PromotionPiece, movedPiece.Color);
                }
                
                if (move.IsEnPassant)
                {
                    board.Squares[move.EnPassantPawnRow, move.EnPassantPawnCol] = null;
                    if (move.MovedPiece.Color == PieceColor.White)
                        whiteScore += 10;
                    else
                        blackScore += 10;
                }
            }
            UpdateScore();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Title = "Salva Partita",
                    Filter = "File di Scacchi (*.che)|*.che",
                    DefaultExt = "che",
                    FileName = $"partita_scacchi_{DateTime.Now:yyyyMMdd_HHmmss}.che"
                };
                
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    // Create a serializable snapshot using simple lists instead of 2D arrays
                    var saveData = new
                    {
                        BoardData = SerializeBoardAsList(),
                        EnPassantTargetRow = board.EnPassantTargetRow,
                        EnPassantTargetCol = board.EnPassantTargetCol,
                        CurrentPlayer = currentPlayer.ToString(),
                        IsAIMode = isAIMode,
                        WhiteScore = whiteScore,
                        BlackScore = blackScore,
                        MoveHistoryData = SerializeMoveHistoryAsList(),
                        SavedDate = DateTime.Now
                    };
                    
                    string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
                    File.WriteAllText(saveDialog.FileName, json);
                    
                    MessageBox.Show("Partita salvata con successo!", "Salvataggio", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il salvataggio: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private List<object> SerializeBoardAsList()
        {
            var boardList = new List<object>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var piece = board.Squares[i, j];
                    if (piece != null)
                    {
                        boardList.Add(new { Row = i, Col = j, Type = piece.Type.ToString(), Color = piece.Color.ToString(), HasMoved = piece.HasMoved });
                    }
                }
            }
            return boardList;
        }
        
        private List<object> SerializeMoveHistoryAsList()
        {
            var movesList = new List<object>();
            foreach (var move in moveHistory)
            {
                movesList.Add(new
                {
                    StartRow = move.StartRow,
                    StartCol = move.StartCol,
                    EndRow = move.EndRow,
                    EndCol = move.EndCol,
                    IsCastling = move.IsCastling,
                    IsEnPassant = move.IsEnPassant,
                    PromotionPiece = move.PromotionPiece.ToString(),
                    MovedPieceType = move.MovedPiece?.Type.ToString(),
                    MovedPieceColor = move.MovedPiece?.Color.ToString(),
                    CapturedPieceType = move.CapturedPiece?.Type.ToString(),
                    CapturedPieceColor = move.CapturedPiece?.Color.ToString()
                });
            }
            return movesList;
        }
        
        private void LoadButton_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new OpenFileDialog
                {
                    Title = "Carica Partita",
                    Filter = "File di Scacchi (*.che)|*.che",
                    DefaultExt = "che"
                };
                
                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    string json = File.ReadAllText(openDialog.FileName);
                    var saveData = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                    
                    if (saveData != null)
                    {
                        // Create new board
                        board = new Board();
                        
                        // Restore board from list
                        var boardData = JsonSerializer.Deserialize<List<object>>(saveData["BoardData"].ToString());
                        foreach (var item in boardData)
                        {
                            var pieceDict = JsonSerializer.Deserialize<Dictionary<string, object>>(item.ToString());
                            int row = Convert.ToInt32(pieceDict["Row"]);
                            int col = Convert.ToInt32(pieceDict["Col"]);
                            PieceType type = (PieceType)Enum.Parse(typeof(PieceType), pieceDict["Type"].ToString());
                            PieceColor color = (PieceColor)Enum.Parse(typeof(PieceColor), pieceDict["Color"].ToString());
                            bool hasMoved = Convert.ToBoolean(pieceDict["HasMoved"]);
                            board.Squares[row, col] = new Piece(type, color) { HasMoved = hasMoved };
                        }
                        
                        board.EnPassantTargetRow = saveData.ContainsKey("EnPassantTargetRow") ? Convert.ToInt32(saveData["EnPassantTargetRow"]) : (int?)null;
                        board.EnPassantTargetCol = saveData.ContainsKey("EnPassantTargetCol") ? Convert.ToInt32(saveData["EnPassantTargetCol"]) : (int?)null;
                        currentPlayer = (PieceColor)Enum.Parse(typeof(PieceColor), saveData["CurrentPlayer"].ToString());
                        isAIMode = Convert.ToBoolean(saveData["IsAIMode"]);
                        whiteScore = Convert.ToInt32(saveData["WhiteScore"]);
                        blackScore = Convert.ToInt32(saveData["BlackScore"]);
                        
                        // Restore move history
                        moveHistory = new List<Move>();
                        var moveDataList = JsonSerializer.Deserialize<List<object>>(saveData["MoveHistoryData"].ToString());
                        foreach (var moveItem in moveDataList)
                        {
                            var moveDict = JsonSerializer.Deserialize<Dictionary<string, object>>(moveItem.ToString());
                            int startRow = Convert.ToInt32(moveDict["StartRow"]);
                            int startCol = Convert.ToInt32(moveDict["StartCol"]);
                            int endRow = Convert.ToInt32(moveDict["EndRow"]);
                            int endCol = Convert.ToInt32(moveDict["EndCol"]);
                            bool isCastling = Convert.ToBoolean(moveDict["IsCastling"]);
                            bool isEnPassant = Convert.ToBoolean(moveDict["IsEnPassant"]);
                            PieceType promotion = moveDict["PromotionPiece"].ToString() != "None" ? (PieceType)Enum.Parse(typeof(PieceType), moveDict["PromotionPiece"].ToString()) : PieceType.None;
                            
                            var move = new Move(startRow, startCol, endRow, endCol, null, null)
                            {
                                IsCastling = isCastling,
                                IsEnPassant = isEnPassant,
                                PromotionPiece = promotion
                            };
                            moveHistory.Add(move);
                        }
                        
                        validator = new MoveValidator(board);
                        ai = new AIOpponent();
                        gameOver = false;
                        selectedRow = null;
                        selectedCol = null;
                        undoneMoves = new List<Move>();
                        currentValidMoves = null;
                        
                        aiModeButton.Text = isAIMode ? "🤖 Modalità IA: ON" : "🤖 Modalità IA: OFF";
                        aiModeButton.BackColor = isAIMode ? Color.FromArgb(76, 175, 80) : Color.FromArgb(255, 152, 0);
                        
                        UpdateBoardDisplay();
                        UpdateStatus();
                        UpdateMoveHistory();
                        UpdateScore();
                        
                        MessageBox.Show("Partita caricata con successo!", "Caricamento", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Errore durante il caricamento: {ex.Message}", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            NewGame();
            statusLabel.Text = "Partita resettata! Turno del Bianco";
            statusLabel.ForeColor = Color.White;
        }

        private void AIModeButton_Click(object sender, EventArgs e)
        {
            isAIMode = !isAIMode;
            aiModeButton.Text = isAIMode ? "🤖 Modalità IA: ON" : "🤖 Modalità IA: OFF";
            aiModeButton.BackColor = isAIMode ? Color.FromArgb(76, 175, 80) : Color.FromArgb(255, 152, 0);
            
            if (isAIMode && currentPlayer == PieceColor.Black && !gameOver && !isAIThinking)
            {
                StartAIMove();
            }
        }

        private void UpdateStatus()
        {
            if (!gameOver)
            {
                bool inCheck = validator.IsKingInCheck(currentPlayer);
                if (inCheck)
                {
                    statusLabel.Text = currentPlayer == PieceColor.White ? "Il Bianco è sotto SCACCO!" : "Il Nero è sotto SCACCO!";
                    statusLabel.ForeColor = Color.Red;
                }
                else
                {
                    statusLabel.Text = currentPlayer == PieceColor.White ? "Turno del Bianco" : "Turno del Nero";
                    statusLabel.ForeColor = Color.White;
                }
            }
        }
        
        private void UpdateMoveHistory()
        {
            moveHistoryBox.Clear();
            for (int i = 0; i < moveHistory.Count; i++)
            {
                var move = moveHistory[i];
                string player = i % 2 == 0 ? "Bianco" : "Nero";
                string moveText = $"{i + 1}. {player}: {GetSquareName(move.StartRow, move.StartCol)} → {GetSquareName(move.EndRow, move.EndCol)}";
                
                if (move.CapturedPiece != null)
                    moveText += " ✗";
                if (move.IsCastling)
                    moveText += " (0-0)";
                if (move.PromotionPiece != PieceType.None)
                    moveText += " (=D)";
                if (move.IsEnPassant)
                    moveText += " (a.p.)";
                
                int index = moveHistoryBox.TextLength;
                
                bool isUndone = undoneMoves.Contains(move);
                if (isUndone)
                    moveText = $"[{moveText}]";
                
                moveHistoryBox.AppendText(moveText + "\n");
                
                moveHistoryBox.SelectionStart = index;
                moveHistoryBox.SelectionLength = moveText.Length;
                
                if (isUndone)
                    moveHistoryBox.SelectionColor = Color.Gray;
                else
                    moveHistoryBox.SelectionColor = i % 2 == 0 ? Color.LightGreen : Color.LightBlue;
            }
        }
        
        private string GetSquareName(int row, int col)
        {
            char file = (char)('a' + col);
            int rank = 8 - row;
            return $"{file}{rank}";
        }
    }
}