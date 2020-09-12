using System;
using System.Collections.Generic;

namespace Tenuto.Sudoku.Core
{
    public class SudokuGenerator
    {
        readonly int[,] _grid = new int[9, 9];
   
        public SudokuGenerator()
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    _grid[r, c] = 0;
        }

        public SudokuGenerator(SudokuGenerator copy)
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    _grid[r, c] = copy._grid[r, c];
        }

        public SudokuGenerator(string[] tileDefinitions)
        {
            PopulateGridFromStrings(tileDefinitions);
        }



        public bool Fill()
        {
            int row = 0;
            int col = 0;
            var numberList = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            for (int i = 0; i < 81; i++)
            {
                row = i / 9;
                col = i % 9;

                // find next empty cell
                if (_grid[row, col] == 0)
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
                            _grid[row, col] = value; // try this value
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
            _grid[row, col] = 0;
            return false;
        }

        public SudokuBoard MakeRandomPuzzle(int cellsLeft)
        {
            Fill();

            var solver = new SudokuFastSolver();

            // Start Removing Numbers one by one

            // A higher number of attempts will end up removing more numbers from the grid
            //Potentially resulting in more difficiult grids to solve!
            var rnd = new Random();
            var cellsFilled = 81;
            int attempt=0;
            while(true) // don't do more then 1000 attempts
            {
                //  Select a random cell that is not already empty
                var cell = rnd.Next(0, cellsFilled);
                int i = 0;
                var row = -1;
                var col = -1;
                for(int r=0; r<9; r++)
                    for(int c=0; c<9; c++)
                        if(_grid[r,c]!=0)
                        {
                            if(cell == i++)
                            {
                                row = r;
                                col = c;
                                break;
                            }
                        }
     
                if(row == -1)
                {
                    // no empty cell to fill
                    return new SudokuBoard(_grid);
                }

                //  Remember its cell value in case we need to put it back  
                var backup = _grid[row, col];
                _grid[row, col] = 0;
                cellsFilled--;

                //  Count the number of solutions that this grid has (using a backtracking approach implemented Solve())
                //  If the number of solution is different from 1 then we need to cancel the change
                //  by putting the value we took away back in the grid
                if (solver.Solve(SudokuBoard.ToSudokuNotation(_grid)).Count != 1)
                {
                    _grid[row, col] = backup;
                    cellsFilled++;
                }

                if (cellsFilled <= cellsLeft)
                    break;

                attempt++;

                if (attempt >= 8000)
                    break;
            }

            Console.WriteLine($"Generated in {attempt} attempts, with {cellsFilled} cells left");
            return new SudokuBoard(_grid);
        }

        // Check if the grid is full
        private bool IsComplete()
        {
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                    if (_grid[row, col] == 0)
                        return false;

            // We have a complete grid!  
            return true;
        }

        private int[] Shuffle(int[] array)
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

        private bool ValueInRow(int value, int row)
        {
            for (int col = 0; col < 9; col++)
                if (_grid[row, col] == value)
                    return true;

            return false;
        }

        private bool ValueInCol(int value, int col)
        {
            for (int row = 0; row < 9; row++)
                if (_grid[row, col] == value)
                    return true;

            return false;
        }

        // 3 x 3 squares 
        private bool ValueInSquare(int value, int squareRow, int squareCol)
        {
            for (int row = squareRow * 3; row < (squareRow * 3 + 3); row++)
                for (int col = squareCol * 3; col < (squareCol * 3 + 3); col++)
                    if (_grid[row, col] == value)
                        return true;

            return false;
        }

        private void PopulateGridFromStrings(string[] initialGrid)
        {
            for (int row = 0; row < 9; row++)
            {
                string tileDefinition = initialGrid[row];

                for (int column = 0; column < 9; column++)
                {
                    _grid[row, column] = tileDefinition[column] == '.' ? 0 : (int)char.GetNumericValue(tileDefinition[column]);
                }
            }
        }
    }


    //# Sudoku Generator Algorithm - www.101computing.net/sudoku-generator-algorithm/


    //# A backtracking/recursive function to check all possible combinations of numbers until a solution is found
    //def solveGrid(grid) :
    //  global counter
    //  #Find next empty cell
    //  for i in range(0,81) :
    //    row=i//9
    //    col = i % 9
    //    if grid[row][col]==0:
    //      for value in range(1,10) :
    //        #Check that this value has not already be used on this row
    //        if not(value in grid[row]) :
    //          #Check that this value has not already be used on this column
    //          if not value in (grid[0][col], grid[1][col], grid[2][col], grid[3][col], grid[4][col], grid[5][col], grid[6][col], grid[7][col], grid[8][col]):
    //            #Identify which of the 9 squares we are working on
    //            square=[]
    //            if row<3:
    //              if col<3:
    //                square=[grid[i][0:3] for i in range(0, 3)]
    //              elif col<6:
    //                square=[grid[i][3:6] for i in range(0, 3)]
    //              else:  
    //                square=[grid[i][6:9] for i in range(0, 3)]
    //            elif row<6:
    //              if col<3:
    //                square=[grid[i][0:3] for i in range(3, 6)]
    //              elif col<6:
    //                square=[grid[i][3:6] for i in range(3, 6)]
    //              else:  
    //                square=[grid[i][6:9] for i in range(3, 6)]
    //            else:
    //              if col<3:
    //                square=[grid[i][0:3] for i in range(6, 9)]
    //              elif col<6:
    //                square=[grid[i][3:6] for i in range(6, 9)]
    //              else:  
    //                square=[grid[i] [6:9] for i in range(6,9)]
    //            #Check that this value has not already be used on this 3x3 square
    //            if not value in (square[0] + square[1] + square[2]):
    //              grid[row][col]=value
    //              if checkGrid(grid) :
    //                counter+=1
    //                break
    //              else:
    //                if solveGrid(grid) :
    //                  return True
    //      break
    //  grid[row][col]=0  

    //numberList=[1,2,3,4,5,6,7,8,9]
    //# shuffle(numberList)

    //# A backtracking/recursive function to check all possible combinations of numbers until a solution is found
    //    def fillGrid(grid):
    //  global counter
    //  #Find next empty cell
    //  for i in range(0,81) :
    //    row=i//9
    //    col = i % 9
    //    if grid[row][col]==0:
    //      shuffle(numberList)
    //      for value in numberList:
    //        #Check that this value has not already be used on this row
    //        if not(value in grid[row]) :
    //          #Check that this value has not already be used on this column
    //          if not value in (grid[0][col], grid[1][col], grid[2][col], grid[3][col], grid[4][col], grid[5][col], grid[6][col], grid[7][col], grid[8][col]):
    //            #Identify which of the 9 squares we are working on
    //            square=[]
    //            if row<3:
    //              if col<3:
    //                square=[grid[i][0:3] for i in range(0, 3)]
    //              elif col<6:
    //                square=[grid[i][3:6] for i in range(0, 3)]
    //              else:  
    //                square=[grid[i][6:9] for i in range(0, 3)]
    //            elif row<6:
    //              if col<3:
    //                square=[grid[i][0:3] for i in range(3, 6)]
    //              elif col<6:
    //                square=[grid[i][3:6] for i in range(3, 6)]
    //              else:  
    //                square=[grid[i][6:9] for i in range(3, 6)]
    //            else:
    //              if col<3:
    //                square=[grid[i][0:3] for i in range(6, 9)]
    //              elif col<6:
    //                square=[grid[i][3:6] for i in range(6, 9)]
    //              else:  
    //                square=[grid[i] [6:9] for i in range(6,9)]
    //            #Check that this value has not already be used on this 3x3 square
    //            if not value in (square[0] + square[1] + square[2]):
    //              grid[row][col]=value
    //              if checkGrid(grid) :
    //                return True
    //              else:
    //                if fillGrid(grid) :
    //                  return True
    //      break
    //  grid[row][col]=0             

    //#Generate a Fully Solved Grid
    //fillGrid(grid)
    //drawGrid(grid)
    //myPen.getscreen().update()
    //sleep(1)


    //#Start Removing Numbers one by one

    //#A higher number of attempts will end up removing more numbers from the grid
    //#Potentially resulting in more difficiult grids to solve!
    //attempts = 5 
    //counter=1
    //while attempts>0:
    //  #Select a random cell that is not already empty
    //  row = randint(0,8)
    //  col = randint(0,8)
    //  while grid[row][col]==0:
    //    row = randint(0,8)
    //    col = randint(0,8)
    //  #Remember its cell value in case we need to put it back  
    //  backup = grid[row][col]
    //  grid[row][col]=0

    //  #Take a full copy of the grid
    //  copyGrid = []
    //  for r in range(0,9) :
    //     copyGrid.append([])
    //     for c in range(0,9) :
    //        copyGrid[r].append(grid[r][c])

    //  # Count the number of solutions that this grid has (using a backtracking approach implemented in the solveGrid() function)
    //    counter=0      
    //  solveGrid(copyGrid)
    //  #If the number of solution is different from 1 then we need to cancel the change by putting the value we took away back in the grid
    //  if counter!=1:
    //    grid[row][col]=backup
    //# We could stop here, but we can also have another attempt with a different cell just to try to remove more numbers
    //    attempts -= 1

    //  myPen.clear()
    //  drawGrid(grid)
    //  myPen.getscreen().update()

    //print("Sudoku Grid Ready")
}
