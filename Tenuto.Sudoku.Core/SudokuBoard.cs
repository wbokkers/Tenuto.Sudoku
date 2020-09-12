using System;
using System.IO;
using System.Text;

namespace Tenuto.Sudoku.Core
{
    /// <summary>
    /// A Sudoku board. This can be a board with initial values (aka 'givens') or a solved board.
    /// </summary>
    public class SudokuBoard
    {
        public int[,] Cells { get; } = new int[9, 9];
        public string SudokuNotation { get; internal set; }

        /// <summary>
        /// Construct using string/dot notation (one single line)
        /// </summary>
        public SudokuBoard(string sdnotation)
        {
            SudokuNotation = sdnotation;
            Cells = ToCellValues(SudokuNotation);
        }

        /// <summary>
        /// Construct using string/dot notation, one line for each row
        /// </summary>
        public SudokuBoard(params string[] sdnotPerLine)
        {
            SudokuNotation = ToSudokuNotation(sdnotPerLine);
            Cells = ToCellValues(SudokuNotation);
        }

        public SudokuBoard(int[,] cells)
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    Cells[r, c] = cells[r, c];
                }

            SudokuNotation = ToSudokuNotation(cells);
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

        public static string ToSudokuNotation(int[,] cells)
        {
            var sb = new StringBuilder();
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    sb.Append((char)(cells[r, c] + '0'));
                }

            return sb.ToString();
        }

        public static int[,] ToCellValues(string sdnotation)
        {
            var cells = new int[9, 9];
            var maxLenght = Math.Min(81, sdnotation.Length);
            for (int i = 0; i < maxLenght; i++)
            {
                var row = i / 9;
                var col = i % 9;

                cells[row, col] = sdnotation[i] == '.' ? 0 : sdnotation[i] - '0';
            }
            return cells;
        }

        public static bool operator ==(SudokuBoard obj1, SudokuBoard obj2)
        {
            return obj1.SudokuNotation == obj2.SudokuNotation;
        }

        // this is second one '!='
        public static bool operator !=(SudokuBoard obj1, SudokuBoard obj2)
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

                    var ch = Cells[r, c] == 0 ? ". " : Cells[r, c].ToString() + " ";
                    writer.Write(ch);
                }
                writer.Write("\r\n ");
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is SudokuBoard grid)
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

