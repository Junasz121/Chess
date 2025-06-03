using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace WpfChessApp
{
    public partial class MainWindow : Window
    {
        private Piece[,] board = new Piece[8, 8];
        private Border[,] uiSquares = new Border[8, 8];
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
                        TextBlock pieceText = new TextBlock
                        {
                            FontSize = 32,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            VerticalAlignment = VerticalAlignment.Center
                        };

                        Border square = new Border
                        {
                            Background = (row + col) % 2 == 0 ? lightBrown : darkBrown,
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0.5),
                            Child = pieceText
                        };

                        int r = row - 1;
                        int c = col - 1;
                        uiSquares[r, c] = square;
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

            // Add more pieces as needed
        }

        private void OnSquareClicked(int row, int col)
        {
            if (selectedPiece == null)
            {
                if (board[row, col] != null)
                {
                    selectedPiece = (row, col);
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
                    var piece = board[row, col];
                    var textBlock = uiSquares[row, col].Child as TextBlock;
                    textBlock.Text = piece?.Symbol ?? "";
                }
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
        public override string Symbol => IsWhite ? "♖" : "♜";

        public override List<(int row, int col)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            int[] directions = { -1, 1 };

            foreach (int d in directions)
            {
                for (int r = row + d; r >= 0 && r < 8; r += d)
                {
                    if (board[r, col] == null)
                        moves.Add((r, col));
                    else
                    {
                        if (board[r, col].IsWhite != IsWhite)
                            moves.Add((r, col));
                        break;
                    }
                }

                for (int c = col + d; c >= 0 && c < 8; c += d)
                {
                    if (board[row, c] == null)
                        moves.Add((row, c));
                    else
                    {
                        if (board[row, c].IsWhite != IsWhite)
                            moves.Add((row, c));
                        break;
                    }
                }
            }
            return moves;
        }
    }

    public class Pawn : Piece
    {
        public override string Symbol => IsWhite ? "♙" : "♟";

        public override List<(int row, int col)> GetValidMoves(int row, int col, Piece[,] board)
        {
            var moves = new List<(int, int)>();
            int direction = IsWhite ? -1 : 1;
            int startRow = IsWhite ? 6 : 1;

            if (InBounds(row + direction) && board[row + direction, col] == null)
                moves.Add((row + direction, col));

            if (row == startRow && board[row + direction, col] == null && board[row + 2 * direction, col] == null)
                moves.Add((row + 2 * direction, col));

            foreach (int dc in new[] { -1, 1 })
            {
                int newCol = col + dc;
                if (InBounds(row + direction) && InBounds(newCol))
                {
                    var target = board[row + direction, newCol];
                    if (target != null && target.IsWhite != IsWhite)
                        moves.Add((row + direction, newCol));
                }
            }

            return moves;
        }

        private bool InBounds(int index) => index >= 0 && index < 8;
    }
}
