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
        public override string Symbol => IsWhite ? "1" : "1";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board) => new();
    }

    public class Knight : Piece
    {
        public override string Symbol => IsWhite ? "2" : "2";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board) => new();
    }

    public class Bishop : Piece
    {
        public override string Symbol => IsWhite ? "3" : "3";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board) => new();
    }

    public class Queen : Piece
    {
        public override string Symbol => IsWhite ? "4" : "4";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board) => new();
    }

    public class King : Piece
    {
        public override string Symbol => IsWhite ? "5" : "5";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board) => new();
    }

    public class Pawn : Piece
    {
        public override string Symbol => IsWhite ? "6" : "6";
        public override List<(int, int)> GetValidMoves(int row, int col, Piece[,] board) => new();
    }
}

