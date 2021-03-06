﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Tenuto.Sudoku.Core
{
    /// <summary>
    /// A Sudoku board. This can be a board with initial values (aka 'givens') or a solved board.
    /// </summary>
    public class SudokuBoard : IEquatable<SudokuBoard>
    {
        public int[,] Cells { get; } = new int[9,9];

        public string SudokuNotation => ToSudokuNotation(Cells);

        /// <summary>
        /// Construct using string/dot notation (one single line)
        /// </summary>
        public SudokuBoard(string sdnotation)
        {
            Cells = ToCellValues(sdnotation);
        }

        /// <summary>
        /// Construct using string/dot notation, one line for each row
        /// </summary>
        public SudokuBoard(params string[] sdnotPerLine)
        {
            Cells = ToCellValues(ToSudokuNotation(sdnotPerLine));
        }

        public SudokuBoard(int[,] cells)
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    Cells[r, c] = cells[r, c];
                }
        }

        public SudokuBoard(SudokuBoard copy)
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    Cells[r, c] = copy.Cells[r, c];
                }
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
            if (object.ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        // this is second one '!='
        public static bool operator !=(SudokuBoard obj1, SudokuBoard obj2)
        {
            if (object.ReferenceEquals(obj1, obj2))
                return false;

            return !obj1.Equals(obj2);
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
            if (obj is SudokuBoard other)
            {
                return Equals(other);
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

        public bool Equals([AllowNull] SudokuBoard other)
        {
            if (other == null)
                return false;

            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    if (Cells[r, c] != other.Cells[r, c])
                        return false; // one difference found: not equal

            return true;
        }
    }
}

