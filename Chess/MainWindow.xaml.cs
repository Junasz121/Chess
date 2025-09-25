using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;

namespace WpfChessApp
{
    public partial class MainWindow : Window
    {
        private Piece[,] board = new Piece[8, 8];
        private Border[,] uiSquares = new Border[8, 8];
        private Ellipse[,] moveDots = new Ellipse[8, 8];
        private (int row, int col)? selectedPiece = null;
        private bool isWhiteTurn = true;
        private bool gameOver = false;

        public MainWindow()
        {
            InitializeComponent();
            CreateLabeledBoard();
            InitializeBoardModel();
            RenderBoard();
        }

        private void CreateLabeledBoard()
        {
            for (int i = 0; i < 10; i++)
            {
                MainGrid.RowDefinitions.Add(new RowDefinition());
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            SolidColorBrush lightBrown = new SolidColorBrush(Color.FromRgb(222, 184, 135));
            SolidColorBrush darkBrown = new SolidColorBrush(Color.FromRgb(139, 69, 19));

            for (int row = 0; row < 10; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    if (row >= 1 && row <= 8 && col >= 1 && col <= 8)
                    {
                        Grid squareContent = new Grid();
                        TextBlock pieceText = new TextBlock
                        {
                            FontSize = 32,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        Ellipse dot = new Ellipse
                        {
                            Width = 12,
                            Height = 12,
                            Fill = Brushes.Black,
                            Visibility = Visibility.Hidden,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        squareContent.Children.Add(pieceText);
                        squareContent.Children.Add(dot);

                        Border square = new Border
                        {
                            Background = (row + col) % 2 == 0 ? lightBrown : darkBrown,
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.5),
                            Child = squareContent
                        };

                        int r = row - 1;
                        int c = col - 1;
                        uiSquares[r, c] = square;
                        moveDots[r, c] = dot;
                        square.MouseLeftButtonDown += (s, e) => OnSquareClicked(r, c);

                        Grid.SetRow(square, row);
                        Grid.SetColumn(square, col);
                        MainGrid.Children.Add(square);
                    }
                    else if ((row == 0 || row == 9) && col >= 1 && col <= 8)
                    {
                        char label = (char)('A' + col - 1);
                        AddLabel(label.ToString(), row, col);
                    }
                    else if ((col == 0 || col == 9) && row >= 1 && row <= 8)
                    {
                        string label = (8 - row + 1).ToString();
                        AddLabel(label, row, col);
                    }
                }
            }
        }

        private void AddLabel(string content, int row, int col)
        {
            TextBlock label = new TextBlock
            {
                Text = content,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetRow(label, row);
            Grid.SetColumn(label, col);
            MainGrid.Children.Add(label);
        }

        private void InitializeBoardModel()
        {
            // Pawns
            for (int i = 0; i < 8; i++)
            {
                board[1, i] = new Pawn { IsWhite = false };
                board[6, i] = new Pawn { IsWhite = true };
            }

            // Rooks
            board[0, 0] = new Rook { IsWhite = false };
            board[0, 7] = new Rook { IsWhite = false };
            board[7, 0] = new Rook { IsWhite = true };
            board[7, 7] = new Rook { IsWhite = true };

            // Knights
            board[0, 1] = new Knight { IsWhite = false };
            board[0, 6] = new Knight { IsWhite = false };
            board[7, 1] = new Knight { IsWhite = true };
            board[7, 6] = new Knight { IsWhite = true };

            // Bishops
            board[0, 2] = new Bishop { IsWhite = false };
            board[0, 5] = new Bishop { IsWhite = false };
            board[7, 2] = new Bishop { IsWhite = true };
            board[7, 5] = new Bishop { IsWhite = true };

            // Queens and Kings
            board[0, 3] = new Queen { IsWhite = false };
            board[0, 4] = new King { IsWhite = false };
            board[7, 3] = new Queen { IsWhite = true };
            board[7, 4] = new King { IsWhite = true };
        }

        private void OnSquareClicked(int row, int col)
        {
            if (gameOver) return;

            ClearMoveDots();

            if (selectedPiece == null)
            {
                // selecting a piece
                if (board[row, col] != null && board[row, col].IsWhite == isWhiteTurn)
                {
                    selectedPiece = (row, col);
                    var piece = board[row, col];
                    var moves = GetLegalMoves(piece, row, col);
                    ShowValidMoves(moves);
                }
            }
            else
            {
                var (srcRow, srcCol) = selectedPiece.Value;
                var piece = board[srcRow, srcCol];
                if (piece == null) { selectedPiece = null; return; }

                var validMoves = GetLegalMoves(piece, srcRow, srcCol);

                if (validMoves.Contains((row, col)))
                {
                    // perform move
                    var captured = board[row, col];
                    board[row, col] = piece;
                    board[srcRow, srcCol] = null;

                    // switch turn
                    isWhiteTurn = !isWhiteTurn;

                    // update UI
                    RenderBoard();

                    // check for mate/pat
                    CheckForEndGame();
                }

                selectedPiece = null;
            }
        }

        private void RenderBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var squareContent = uiSquares[row, col].Child as Grid;
                    squareContent.Children.Clear();

                    // Add piece shape
                    UIElement pieceShape = GetPieceShape(board[row, col]);
                    squareContent.Children.Add(pieceShape);

                    // Re-create move dot every time (avoid re-using UIElements)
                    var dot = new Ellipse
                    {
                        Width = 10,
                        Height = 10,
                        Fill = Brushes.Black,
                        Visibility = Visibility.Hidden,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    squareContent.Children.Add(dot);

                    moveDots[row, col] = dot;
                }
            }
        }



        private void ClearMoveDots()
        {
            for (int row = 0; row < 8; row++)
                for (int col = 0; col < 8; col++)
                    moveDots[row, col].Visibility = Visibility.Hidden;
        }

        private void ShowValidMoves(List<(int row, int col)> moves)
        {
            foreach (var (row, col) in moves)
            {
                moveDots[row, col].Visibility = Visibility.Visible;
            }
        }


        private bool IsKingInCheck(bool isWhite)
        {
            // find king
            (int r, int c)? kingPos = null;
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    if (board[i, j] is King && board[i, j].IsWhite == isWhite)
                        kingPos = (i, j);

            if (kingPos == null) return false; 

            var (kr, kc) = kingPos.Value;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    var attacker = board[i, j];
                    if (attacker != null && attacker.IsWhite != isWhite)
                    {
                        var attacks = attacker.GetValidMoves(i, j, board);
                        if (attacks.Contains((kr, kc))) return true;
                    }
                }
            }

            return false;
        }

        private List<(int row, int col)> GetLegalMoves(Piece piece, int srcRow, int srcCol)
        {
            var valid = piece.GetValidMoves(srcRow, srcCol, board);
            var legal = new List<(int, int)>();

            foreach (var (r, c) in valid)
            {

                var backup = board[r, c];
                board[r, c] = piece;
                board[srcRow, srcCol] = null;

                bool kingInCheck = IsKingInCheck(piece.IsWhite);

 
                board[srcRow, srcCol] = piece;
                board[r, c] = backup;

                if (!kingInCheck) legal.Add((r, c));
            }

            return legal;
        }

        private bool PlayerHasAnyLegalMove(bool isWhite)
        {
            for (int r = 0; r < 8; r++)
            {
                for (int c = 0; c < 8; c++)
                {
                    var piece = board[r, c];
                    if (piece != null && piece.IsWhite == isWhite)
                    {
                        var legal = GetLegalMoves(piece, r, c);
                        if (legal.Count > 0) return true;
                    }
                }
            }
            return false;
        }

        private void CheckForEndGame()
        {
            
            bool playerToMoveIsWhite = isWhiteTurn;
            bool inCheck = IsKingInCheck(playerToMoveIsWhite);
            bool hasMove = PlayerHasAnyLegalMove(playerToMoveIsWhite);

            if (!hasMove)
            {
                gameOver = true;
                if (inCheck)
                {
                    // checkmate
                    string winner = playerToMoveIsWhite ? "Black" : "White";
                    MessageBox.Show($"Checkmate! {winner} wins.", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // stalemate (pat)
                    MessageBox.Show("Stalemate! It's a draw.", "Game Over", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        private UIElement GetPieceShape(Piece piece)
        {
            if (piece == null) return new TextBlock(); // empty square

            Brush color = piece.IsWhite ? Brushes.White : Brushes.Black;

            switch (piece)
            {
                case Pawn:
                    return new Ellipse
                    {
                        Width = 20,
                        Height = 20,
                        Fill = color,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                case Rook:
                    return new Rectangle
                    {
                        Width = 20,
                        Height = 20,
                        Fill = color,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                case Knight:
                    return new Polygon
                    {
                        Points = new PointCollection { new Point(0, 20), new Point(10, 0), new Point(20, 20) },
                        Fill = color,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 20,
                        Height = 20
                    };

                case Bishop:
                    return new Ellipse
                    {
                        Width = 15,
                        Height = 25,
                        Fill = color,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                case Queen:
                    return new Polygon
                    {
                        Points = new PointCollection { new Point(10, 0), new Point(20, 10), new Point(10, 20), new Point(0, 10) },
                        Fill = color,
                        Stretch = Stretch.Uniform,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Width = 20,
                        Height = 20
                    };

                case King:
                    var cross = new Grid { Width = 20, Height = 20 };
                    cross.Children.Add(new Rectangle
                    {
                        Width = 6,
                        Height = 20,
                        Fill = color,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    cross.Children.Add(new Rectangle
                    {
                        Width = 20,
                        Height = 6,
                        Fill = color,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    });
                    return cross;

                default:
                    return new Ellipse
                    {
                        Width = 15,
                        Height = 15,
                        Fill = color
                    };
            }
        }




    }

    public abstract class Piece
    {
        public bool IsWhite { get; set; }
        public abstract string Symbol { get; }
        public abstract List<(int row, int col)> GetValidMoves(int row, int col, Piece[,] board);
    }

    public class Rook : Piece
    {
        public override string Symbol => "1";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            int[] dirs = { -1, 1 };

            foreach (var d in dirs)
            {
                for (int r = row + d; r >= 0 && r < 8; r += d)
                {
                    if (board[r, col] == null) moves.Add((r, col));
                    else
                    {
                        if (board[r, col].IsWhite != this.IsWhite) moves.Add((r, col));
                        break;
                    }
                }

                for (int c = col + d; c >= 0 && c < 8; c += d)
                {
                    if (board[row, c] == null) moves.Add((row, c));
                    else
                    {
                        if (board[row, c].IsWhite != this.IsWhite) moves.Add((row, c));
                        break;
                    }
                }
            }
            return moves;
        }
    }

    public class Knight : Piece
    {
        public override string Symbol => "2";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            int[,] offsets = { { -2, -1 }, { -2, 1 }, { -1, -2 }, { -1, 2 }, { 1, -2 }, { 1, 2 }, { 2, -1 }, { 2, 1 } };
            for (int i = 0; i < offsets.GetLength(0); i++)
            {
                int r = row + offsets[i, 0];
                int c = col + offsets[i, 1];
                if (r >= 0 && r < 8 && c >= 0 && c < 8 && (board[r, c] == null || board[r, c].IsWhite != this.IsWhite))
                    moves.Add((r, c));
            }
            return moves;
        }
    }

    public class Bishop : Piece
    {
        public override string Symbol => "3";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            int[] dirs = { -1, 1 };

            foreach (var dr in dirs)
                foreach (var dc in dirs)
                {
                    int r = row + dr, c = col + dc;
                    while (r >= 0 && r < 8 && c >= 0 && c < 8)
                    {
                        if (board[r, c] == null) moves.Add((r, c));
                        else
                        {
                            if (board[r, c].IsWhite != this.IsWhite) moves.Add((r, c));
                            break;
                        }
                        r += dr; c += dc;
                    }
                }
            return moves;
        }
    }

    public class Queen : Piece
    {
        public override string Symbol => "4";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var rook = new Rook { IsWhite = this.IsWhite };
            var bishop = new Bishop { IsWhite = this.IsWhite };
            var moves = new List<(int, int)>();
            moves.AddRange(rook.GetValidMoves(row, col, board));
            moves.AddRange(bishop.GetValidMoves(row, col, board));
            return moves;
        }
    }

    public class King : Piece
    {
        public override string Symbol => "5";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int r = row + dr, c = col + dc;
                    if (r >= 0 && r < 8 && c >= 0 && c < 8 && (board[r, c] == null || board[r, c].IsWhite != this.IsWhite))
                        moves.Add((r, c));
                }
            return moves;
        }
    }

    public class Pawn : Piece
    {
        public override string Symbol => "6";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            int dir = this.IsWhite ? -1 : 1;
            int start = this.IsWhite ? 6 : 1;

            if (IsInside(row + dir, col) && board[row + dir, col] == null)
            {
                moves.Add((row + dir, col));
                if (row == start && board[row + 2 * dir, col] == null) moves.Add((row + 2 * dir, col));
            }

            foreach (int dc in new[] { -1, 1 })
            {
                int nc = col + dc;
                if (IsInside(row + dir, nc) && board[row + dir, nc] != null && board[row + dir, nc].IsWhite != this.IsWhite)
                    moves.Add((row + dir, nc));
            }

            return moves;
        }

        private bool IsInside(int r, int c) => r >= 0 && r < 8 && c >= 0 && c < 8;
    }
}


        private bool IsInsideBoard(int r, int c) => r >= 0 && r < 8 && c >= 0 && c < 8;
    }
}

