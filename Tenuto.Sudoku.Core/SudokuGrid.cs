using System;
using System.IO;
using System.Text;

namespace Tenuto.Sudoku.Core
{
    public class SudokuGrid
    {
        public int[,] Grid { get; } = new int[9, 9];
        public string SudokuNotation { get; internal set; }

        /// <summary>
        /// Construct using string/dot notation (one single line)
        /// </summary>
        public SudokuGrid(string sdnotation)
        {
            SudokuNotation = sdnotation;
            Grid = ToGrid(SudokuNotation);
        }

        /// <summary>
        /// Construct using string/dot notation, one line for each row
        /// </summary>
        public SudokuGrid(params string[] sdnotPerLine)
        {
            SudokuNotation = ToSudokuNotation(sdnotPerLine);
            Grid = ToGrid(SudokuNotation);
        }

        public SudokuGrid(int[,] grid)
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    Grid[r, c] = grid[r, c];
                }

            SudokuNotation = ToSudokuNotation(grid);
        }

        public static string ToSudokuNotation(string[] sdnotPerLine)
        {
            var sb = new StringBuilder();
            foreach (var line in sdnotPerLine)
            {
                sb.Append(line);
            }

            return sb.ToString();
        }

        public static string ToSudokuNotation(int[,] grid)
        {
            var sb = new StringBuilder();
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    sb.Append((char)(grid[r, c] + '0'));
                }

            return sb.ToString();
        }

        public static int[,] ToGrid(string sdnotation)
        {
            var grid = new int[9, 9];
            var maxLenght = Math.Min(81, sdnotation.Length);
            for (int i = 0; i < maxLenght; i++)
            {
                var row = i / 9;
                var col = i % 9;

                grid[row, col] = sdnotation[i] == '.' ? 0 : sdnotation[i] - '0';
            }
            return grid;
        }

        //private void PopulateGridFromStrings(string[] initialGrid)
        //{
        //    for (int row = 0; row < 9; row++)
        //    {
        //        string tileDefinition = initialGrid[row];

        //        for (int column = 0; column < 9; column++)
        //        {
        //            _grid[row, column] = tileDefinition[column] == '.' ? 0 : (int)char.GetNumericValue(tileDefinition[column]);
        //        }
        //    }
        //}

        public static bool operator ==(SudokuGrid obj1, SudokuGrid obj2)
        {
            return obj1.SudokuNotation == obj2.SudokuNotation;
        }

        // this is second one '!='
        public static bool operator !=(SudokuGrid obj1, SudokuGrid obj2)
        {
            return obj1.SudokuNotation != obj2.SudokuNotation;
        }

        public void WriteToConsole()
        {
            Write(Console.Out);
        }

        public void WriteToFile(string fileName)
        {
            using var stream = new StreamWriter(fileName);
            Write(stream);
        }

        public void Write(TextWriter writer)
        {
            writer.Write(" ");
            for (int r = 0; r < 9; r++)
            {
                if ((r % 3) == 0 && r != 0)
                    writer.Write("- - - + - - - + - - -\r\n ");
                for (int c = 0; c < 9; c++)
                {
                    if ((c % 3) == 0 && c != 0)
                        writer.Write("| ");

                    var ch = Grid[r, c] == 0 ? ". " : Grid[r, c].ToString() + " ";
                    writer.Write(ch);
                }
                writer.Write("\r\n ");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is SudokuGrid grid)
            {
                return string.Equals(SudokuNotation, grid.SudokuNotation);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return SudokuNotation.GetHashCode();
        }

        public override string ToString()
        {
            return SudokuNotation;
        }
    }
}

