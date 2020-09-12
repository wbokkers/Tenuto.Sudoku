using System;
using System.Threading;

namespace Tenuto.Sudoku.Core
{
    // Adapted from www.101computing.net/sudoku-generator-algorithm/
    // Improved by Wim Bokkers
    public class SudokuGameFactory
    {
        private static int[,] _Cells = new int[9, 9];

        /// <summary>
        /// Create a game with the specified number of given cells.
        /// There is no quarantee that a low number of givens is achievable. 
        /// You can increase the probability to get a low number of givens, by increasing the maximum number of attempts.
        /// The actual number of givens can be retrieved from the returned game object.
        /// </summary>
        /// <param name="givensCount"></param>
        /// <param name="maxAttempts"></param>
        /// <returns></returns>
        public static SudokuGame Create(int givensCount, int maxAttempts=2000)
        {
            Reset();
            Fill();

            var finalBoard = new SudokuBoard(_Cells);

            var solver = new SudokuSolver();

            // Start Removing Numbers one by one

            // A higher number of attempts will end up removing more numbers from the grid
            //Potentially resulting in more difficiult grids to solve!
            var rnd = new Random();
            var cellsFilled = 81;
            int attempt = 0;
            while (true) 
            {
                //  Select a random cell that is not already empty
                var cell = rnd.Next(0, cellsFilled);
                int i = 0;
                var row = -1;
                var col = -1;
                for (int r = 0; r < 9; r++)
                    for (int c = 0; c < 9; c++)
                        if (_Cells[r, c] != 0)
                        {
                            if (cell == i++)
                            {
                                row = r;
                                col = c;
                                break;
                            }
                        }

                if (row == -1)
                {
                    // no empty cell to fill
                    return new SudokuGame(new SudokuBoard(_Cells), finalBoard);
                }

                //  Remember its cell value in case we need to put it back  
                var backup = _Cells[row, col];
                _Cells[row, col] = 0;
                cellsFilled--;

                //  Count the number of solutions that this grid has (using a backtracking approach implemented Solve())
                //  If the number of solution is different from 1 then we need to cancel the change
                //  by putting the value we took away back in the grid
                if (solver.Solve(SudokuBoard.ToSudokuNotation(_Cells)).Count != 1)
                {
                    _Cells[row, col] = backup;
                    cellsFilled++;
                }

                if (cellsFilled <= givensCount)
                    break;

                attempt++;

                if (attempt >= maxAttempts)
                    break;
            }

            Console.WriteLine($"Generated in {attempt} attempts, with {cellsFilled} cells left");
            return new SudokuGame(new SudokuBoard(_Cells), finalBoard);
        }

    
        private static bool Fill()
        {
            int row = 0;
            int col = 0;
            var numberList = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int i = 0; i < 81; i++)
            {
                row = i / 9;
                col = i % 9;

                // find next empty cell
                if (_Cells[row, col] == 0)
                {
                    var shuffledNumbers = Shuffle(numberList);

                    for (int n = 0; n < 9; n++)
                    {
                        var value = shuffledNumbers[n];

                        // Find a value that can be used at this row, col

                        if (!ValueInRow(value, row) &&
                            !ValueInCol(value, col) &&
                            !ValueInSquare(value, row / 3, col / 3))
                        {
                            _Cells[row, col] = value; // try this value
                            if (IsComplete()) // grid is full
                                return true;
                            else if (Fill()) // not yet filled
                                return true;
                        }
                    }
                    break;
                }
            }

            // clear tried value
            _Cells[row, col] = 0;
            return false;
        }

        private static void Reset()
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    _Cells[r, c] = 0;
        }

        // Check if the grid is full
        private static bool IsComplete()
        {
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                    if (_Cells[row, col] == 0)
                        return false;

            // We have a complete grid!  
            return true;
        }

        private static int[] Shuffle(int[] array)
        {
            var shuffledArray = new int[array.Length];
            int rndNo;

            var rnd = new Random();
            for (int i = array.Length; i >= 1; i--)
            {
                rndNo = rnd.Next(1, i + 1) - 1;
                shuffledArray[i - 1] = array[rndNo];
                array[rndNo] = array[i - 1];
            }
            return shuffledArray;
        }

        private static bool ValueInRow(int value, int row)
        {
            for (int col = 0; col < 9; col++)
                if (_Cells[row, col] == value)
                    return true;

            return false;
        }

        private static bool ValueInCol(int value, int col)
        {
            for (int row = 0; row < 9; row++)
                if (_Cells[row, col] == value)
                    return true;

            return false;
        }

        // 3 x 3 squares 
        private static bool ValueInSquare(int value, int squareRow, int squareCol)
        {
            for (int row = squareRow * 3; row < (squareRow * 3 + 3); row++)
                for (int col = squareCol * 3; col < (squareCol * 3 + 3); col++)
                    if (_Cells[row, col] == value)
                        return true;

            return false;
        }

 
    }
}


