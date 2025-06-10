using System;
using System.Collections.Generic;
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
                            Width = 10,
                            Height = 10,
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
            for (int i = 0; i < 8; i++)
            {
                board[1, i] = new Pawn { IsWhite = false };
                board[6, i] = new Pawn { IsWhite = true };
            }

            board[0, 0] = new Rook { IsWhite = false };
            board[0, 7] = new Rook { IsWhite = false };
            board[7, 0] = new Rook { IsWhite = true };
            board[7, 7] = new Rook { IsWhite = true };

            board[0, 1] = new Knight { IsWhite = false };
            board[0, 6] = new Knight { IsWhite = false };
            board[7, 1] = new Knight { IsWhite = true };
            board[7, 6] = new Knight { IsWhite = true };

            board[0, 2] = new Bishop { IsWhite = false };
            board[0, 5] = new Bishop { IsWhite = false };
            board[7, 2] = new Bishop { IsWhite = true };
            board[7, 5] = new Bishop { IsWhite = true };

            board[0, 3] = new Queen { IsWhite = false };
            board[0, 4] = new King { IsWhite = false };
            board[7, 3] = new Queen { IsWhite = true };
            board[7, 4] = new King { IsWhite = true };
        }

        private void OnSquareClicked(int row, int col)
        {
            ClearMoveDots();

            if (selectedPiece == null)
            {
                if (board[row, col] != null)
                {
                    selectedPiece = (row, col);
                    ShowValidMoves(board[row, col].GetValidMoves(row, col, board));
                }
            }
            else
            {
                var (srcRow, srcCol) = selectedPiece.Value;
                var piece = board[srcRow, srcCol];
                var validMoves = piece.GetValidMoves(srcRow, srcCol, board);

                if (validMoves.Contains((row, col)))
                {
                    board[row, col] = piece;
                    board[srcRow, srcCol] = null;
                }
                selectedPiece = null;
                RenderBoard();
            }
        }

        private void RenderBoard()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var squareContent = uiSquares[row, col].Child as Grid;
                    var textBlock = squareContent.Children[0] as TextBlock;
                    var dot = squareContent.Children[1] as Ellipse;

                    textBlock.Text = board[row, col]?.Symbol ?? "";
                    dot.Visibility = Visibility.Hidden;
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
    }

    public abstract class Piece
    {
        public bool IsWhite { get; set; }
        public abstract string Symbol { get; }
        public abstract List<(int row, int col)> GetValidMoves(int row, int col, Piece[,] board);
    }

    public class Rook : Piece
    {
        public override string Symbol => IsWhite ? "1" : "1";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            int[] directions = { -1, 1 };

            foreach (var dir in directions)
            {
                for (int i = row + dir; i >= 0 && i < 8; i += dir)
                {
                    if (board[i, col] == null || board[i, col].IsWhite != IsWhite)
                    {
                        moves.Add((i, col));
                        if (board[i, col] != null) break;
                    }
                    else break;
                }

                for (int j = col + dir; j >= 0 && j < 8; j += dir)
                {
                    if (board[row, j] == null || board[row, j].IsWhite != IsWhite)
                    {
                        moves.Add((row, j));
                        if (board[row, j] != null) break;
                    }
                    else break;
                }
            }

            return moves;
        }
    }

    public class Knight : Piece
    {
        public override string Symbol => IsWhite ? "2" : "2";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            int[,] offsets = { { -2, -1 }, { -2, 1 }, { -1, -2 }, { -1, 2 }, { 1, -2 }, { 1, 2 }, { 2, -1 }, { 2, 1 } };

            for (int i = 0; i < offsets.GetLength(0); i++)
            {
                int r = row + offsets[i, 0];
                int c = col + offsets[i, 1];
                if (r >= 0 && r < 8 && c >= 0 && c < 8 && (board[r, c] == null || board[r, c].IsWhite != IsWhite))
                {
                    moves.Add((r, c));
                }
            }
            return moves;
        }
    }

    public class Bishop : Piece
    {
        public override string Symbol => IsWhite ? "3" : "3";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            int[] directions = { -1, 1 };

            foreach (var dr in directions)
            {
                foreach (var dc in directions)
                {
                    int r = row + dr;
                    int c = col + dc;
                    while (r >= 0 && r < 8 && c >= 0 && c < 8)
                    {
                        if (board[r, c] == null || board[r, c].IsWhite != IsWhite)
                        {
                            moves.Add((r, c));
                            if (board[r, c] != null) break;
                        }
                        else break;
                        r += dr;
                        c += dc;
                    }
                }
            }
            return moves;
        }
    }

    public class Queen : Piece
    {
        public override string Symbol => IsWhite ? "4" : "4";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var rookMoves = new Rook { IsWhite = this.IsWhite }.GetValidMoves(row, col, board);
            var bishopMoves = new Bishop { IsWhite = this.IsWhite }.GetValidMoves(row, col, board);
            rookMoves.AddRange(bishopMoves);
            return rookMoves;
        }
    }

    public class King : Piece
    {
        public override string Symbol => IsWhite ? "5" : "5";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int r = row + dr;
                    int c = col + dc;
                    if (r >= 0 && r < 8 && c >= 0 && c < 8 && (board[r, c] == null || board[r, c].IsWhite != IsWhite))
                    {
                        moves.Add((r, c));
                    }
                }
            }
            return moves;
        }
    }

    public class Pawn : Piece
    {
        public override string Symbol => IsWhite ? "6" : "6";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            int direction = IsWhite ? -1 : 1;
            int startRow = IsWhite ? 6 : 1;

            if (IsInsideBoard(row + direction, col) && board[row + direction, col] == null)
            {
                moves.Add((row + direction, col));
                if (row == startRow && board[row + 2 * direction, col] == null)
                {
                    moves.Add((row + 2 * direction, col));
                }
            }

            for (int dc = -1; dc <= 1; dc += 2)
            {
                int newCol = col + dc;
                if (IsInsideBoard(row + direction, newCol) && board[row + direction, newCol] != null && board[row + direction, newCol].IsWhite != IsWhite)
                {
                    moves.Add((row + direction, newCol));
                }
            }

            return moves;
        }

        private bool IsInsideBoard(int r, int c) => r >= 0 && r < 8 && c >= 0 && c < 8;
    }
}
