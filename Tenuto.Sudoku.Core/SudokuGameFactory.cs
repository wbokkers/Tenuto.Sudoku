using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Tenuto.Sudoku.Core
{
    public class SudokuGameFactory
    {
        /// <summary>
        /// Create a game with the specified number of given cells.
        /// There is no quarantee that a low number of givens is achievable. 
        /// You can increase the probability to get a low number of givens, by increasing the maximum number of attempts.
        /// The actual number of givens can be retrieved from the returned game object.
        /// </summary>
        public static SudokuGame Create(int givensCount, int maxAttempts = 2000)
        {
            // Start with a random finished board
            // and use it as the initial board 
            var finishedBoard = RandomFinishedBoard();
            var initialBoard = new SudokuBoard(finishedBoard); // start with finished

            var cells = initialBoard.Cells;

            // Create a solver that returns 2 solutions at most
            var solver = new SudokuSolver(maxSolutions:2);

            // Start Removing Numbers one by one from the cells of the initial board

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
                        if (cells[r, c] != 0)
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
                    return new SudokuGame(initialBoard, finishedBoard);
                }

                //  Remember its cell value in case we need to put it back  
                var backup = cells[row, col];
                cells[row, col] = 0;
                cellsFilled--;

                //  Count the number of solutions that this grid has (using a backtracking approach implemented Solve())
                //  If the number of solution is different from 1 then we need to cancel the change
                //  by putting the value we took away back in the grid
                if (solver.Solve(initialBoard).Count != 1)
                {
                    cells[row, col] = backup;
                    cellsFilled++;
                }

                if (cellsFilled <= givensCount)
                    break;

                attempt++;

                if (attempt >= maxAttempts)
                    break;
            }

            Console.WriteLine($"Generated in {attempt} attempts, with {cellsFilled} cells left");
            return new SudokuGame(initialBoard, finishedBoard);
        }

        private static SudokuBoard RandomFinishedBoard()
        {
            var rnd = new Random();

            // Start with a board with a single random number 
            // in a random cell
            var randomNumber = rnd.Next(9) + 1;
            var randomPos = rnd.Next(81);
            var sdot = new char[81];
            for (int i = 0; i < 81; i++)
                sdot[i] = '.';
            sdot[randomPos] = (char)('0' + randomNumber);
           
            // Find at most 200 solutions
            var solver = new SudokuSolver(maxSolutions:200);
            var filledBoards = solver.Solve(new string(sdot));
            if (filledBoards.Count == 0)
                return null;

            // Get a random solution
            var boardIndex = rnd.Next(filledBoards.Count);
            return filledBoards[boardIndex];
        }
    }
}


