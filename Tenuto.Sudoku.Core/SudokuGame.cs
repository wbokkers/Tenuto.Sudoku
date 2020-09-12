using System.Linq;

namespace Tenuto.Sudoku.Core
{
    public class SudokuGame
    {
        /// <summary>
        /// The initial board at the start of the game.
        /// </summary>
        public SudokuBoard InitialBoard { get; private set; }

        /// <summary>
        /// The solution of the game.
        /// </summary>
        public SudokuBoard Solution { get; private set; }

        public SudokuGame(SudokuBoard initialBoard, SudokuBoard finishedBoard)
        {
            InitialBoard = initialBoard;
            Solution = finishedBoard;
        }

        /// <summary>
        /// Get number of values given on the initial board.
        /// </summary>
        public int GivensCount => InitialBoard.SudokuNotation.Where(c => c != '0' && c != '.').Count();
    }
}
