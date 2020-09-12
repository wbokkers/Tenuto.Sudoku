using System.Linq;
using Tenuto.Sudoku.Core;
using Xunit;

namespace Tenuto.Sudoku.Test
{
    public class SudokuSolverTest
    {

        [Fact]
        public void SudokuSolver_WithSolution()
        {
            // Arrange
            var solver = new SudokuSolver();
            var initialBoard = new SudokuBoard(
                "...84...9",
                "..1.....5",
                "8...2146.",
                "7.8....9.",
                ".........",
                ".5....3.1",
                ".2491...7",
                "9.....5..",
                "3...84..."
            );

            var solvedBoard = new SudokuBoard(
                "632845179",
                "471369285",
                "895721463",
                "748153692",
                "163492758",
                "259678341",
                "524916837",
                "986237514",
                "317584926"
            );

            // Act
            var solutions = solver.Solve(initialBoard);

            // Assert
            Assert.Single(solutions);
            Assert.Equal(solvedBoard, solutions.First());
        }

        [Fact]
        public void SudokuSolver_MultipleSolutions()
        {
            // Arrange
            var solver = new SudokuSolver();
            var initialBoard = new SudokuBoard(
                "...84...9",
                "..1.....5",
                "8...2.46.", // Removed a "1" on this line
                "7.8....9.",
                ".........",
                ".5....3.1",
                ".2491...7",
                "9.....5..",
                "3...84..."
            );

            // Act
            var solutions = solver.Solve(initialBoard);

            // Assert
            Assert.Equal(20, solutions.Count());
        }

        [Fact]
        public void SudokuSolver_SolveRandomHardGame()
        {
            // Arrange
            var game = SudokuGameFactory.Create(26, 2000);

            var solver = new SudokuSolver();

            // Act
            var solutions = solver.Solve(game.InitialBoard);

            // Assert
            Assert.Single(solutions);
            Assert.Equal(game.Solution, solutions.First());
            Assert.Equal(26, game.GivensCount);
        }

        [Fact]
        public void SudokuSolver_SolveRandomEasyGame()
        {
            // Arrange
            var game = SudokuGameFactory.Create(60, 2000);

            var solver = new SudokuSolver();

            // Act
            var solutions = solver.Solve(game.InitialBoard);

            // Assert
            Assert.Single(solutions);
            Assert.Equal(game.Solution, solutions.First());
            Assert.Equal(60, game.GivensCount);
        }



    }
}
