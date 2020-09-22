using System;
using System.Collections.Generic;

namespace Tenuto.Sudoku.Core
{
    /// <summary>
    /// A very fast Sudoku solver by Attractive Chaos and Guenter Stertenbrink.
    /// Translated from C to C# by Wim Bokkers.
    /// 
    /// For license and information about this solver, see:
    /// https://raw.githubusercontent.com/attractivechaos/plb/master/sudoku/sudoku_v1.c
    /// </summary>
    public class SudokuSolver
    {
        private readonly Auxiliary _aux;
        private readonly int _maxSolutions;
     
        public SudokuSolver(int maxSolutions = 1000)
        {
            _maxSolutions = Math.Max(1, maxSolutions);
            _aux = GenerateMatrixRepresentation();
        }

        /// <summary>
        /// Solve a Sudoku with the specified initial state
        /// </summary>
        public List<SudokuBoard> Solve(SudokuBoard initialBoard)
        {
            return Solve(initialBoard.SudokuNotation);
        }

        /// <summary>
        /// Solve a Sudoku with the specified initial state (using the standard dot/number representation)
        /// Example: "...84...9..1.....58...2146.7.8....9...........5....3.1.2491...79.....5..3...84..."
        /// </summary>
        public List<SudokuBoard> Solve(string sdnot)
        {
            var solutions = new List<SudokuBoard>();
            
            int i, j, c, r, r2, dir, cand,  min, hints = 0; // dir=1: forward; dir=-1: backtrack
            var sr = new sbyte[729];
            var cr = new sbyte[81]; // sr[r]: # times the row is forbidden by others; cr[i]: row chosen at step i
            var sc = new byte[324]; // bit 1-7: # allowed choices; bit 8: the constraint has been used or not
            var cc = new short[81]; // cc[i]: col chosen at step i
            var sdnot_out = new char[81];

            for (r = 0; r < 729; ++r) 
                sr[r] = 0; // no row is forbidden
            
            for (c = 0; c < 324; ++c) 
                sc[c] = unchecked(0 << 7 | 9); // 9 allowed choices; no constraint has been used
            
            for (i = 0; i < 81; ++i)
            {
                int a = sdnot[i] >= '1' && sdnot[i] <= '9' ? sdnot[i] - '1' : -1; // number from -1 to 8
                if (a >= 0) UpdateStateVectors(sr, sc, i * 9 + a, 1); // set the choice
                if (a >= 0) ++hints; // count the number of hints
                cr[i] = -1;
                cc[i] = -1;
                sdnot_out[i] = sdnot[i];
            }
        
            for (i = 0, dir = 1, cand = 10 << 16 | 0; ;)
            {
                while (i >= 0 && i < 81 - hints)
                { // maximum 81-hints steps
                    if (dir == 1)
                    {
                        min = cand >> 16;
                        cc[i] = unchecked((short)(cand & 0xffff));
                        if (min > 1)
                        {
                            for (c = 0; c < 324; ++c)
                            {
                                if (sc[c] < min)
                                {
                                    min = sc[c];
                                    cc[i] = (short)c; // choose the top constraint
                                    if (min <= 1) break; // this is for acceleration; slower without this line
                                }
                            }
                        }
                        if (min == 0 || min == 10)
                            cr[i--] = unchecked((sbyte)(dir = -1)); // backtrack
                    }
                    c = cc[i];
                    if (dir == -1 && cr[i] >= 0) UpdateStateVectors(sr, sc, _aux.r[c, cr[i]], -1); // revert the choice
                    for (r2 = cr[i] + 1; r2 < 9; ++r2) // search for the choice to make
                        if (sr[_aux.r[c, r2]] == 0) break; // found if the state equals 0
                    if (r2 < 9)
                    {
                        cand = UpdateStateVectors(sr, sc, _aux.r[c, r2], 1); // set the choice
                        cr[i++] = unchecked((sbyte)r2); dir = 1; // moving forward
                    }
                    else cr[i--] = unchecked((sbyte)(dir = -1)); // backtrack
                }
            
                if (i < 0) 
                    break;

                for (j = 0; j < i; ++j)
                {
                    r = _aux.r[cc[j], cr[j]];

                    sdnot_out[r / 9] = (char)(r % 9 + '1'); // print
                }

                var solution = new SudokuBoard(new string(sdnot_out));

                solutions.Add(solution);
                if (solutions.Count >= _maxSolutions)
                    break;

                --i; dir = -1; // backtrack
            }
            return solutions; // return the number of solutions
        }

    

        // update the state vectors when we pick up choice r; v=1 for setting choice; v=-1 for reverting
        private int UpdateStateVectors(sbyte[] sr, byte[] sc, int r, int v)
        {
            int c2, min = 10, min_c = 0;
            for (c2 = 0; c2 < 4; ++c2)
                sc[_aux.c[r, c2]] += unchecked((byte)(v << 7));

            for (c2 = 0; c2 < 4; ++c2)
            { // update # available choices
                int r2, rr, cc2, c = _aux.c[r, c2];
                if (v > 0)
                { // move forward
                    for (r2 = 0; r2 < 9; ++r2)
                    {
                        if (sr[rr = _aux.r[c, r2]]++ != 0) continue; // update the row status
                        for (cc2 = 0; cc2 < 4; ++cc2)
                        {
                            int cc = _aux.c[rr, cc2];
                            if (--sc[cc] < min) // update # allowed choices
                            {
                                min = sc[cc];
                                min_c = cc; // register the minimum number
                            }
                        }
                    }
                }
                else
                { // revert
                    var p = _aux.c;

                    for (r2 = 0; r2 < 9; ++r2)
                    {
                        if (--sr[rr = _aux.r[c, r2]] != 0) continue; // update the row status

                        ++sc[p[rr, 0]]; ++sc[p[rr, 1]]; ++sc[p[rr, 2]]; ++sc[p[rr, 3]]; // update the count array
                    }
                }
            }
            return min << 16 | min_c; // return the col that has been modified and with the minimal available choices
        }


        // generate the sparse representation of the binary matrix
        private Auxiliary GenerateMatrixRepresentation()
        {
            int i, j, k, r, c, c2;
            var nr = new sbyte[324];
            Auxiliary a = new Auxiliary();

            for (i = r = 0; i < 9; ++i) // generate c[729][4]
                for (j = 0; j < 9; ++j)
                    for (k = 0; k < 9; ++k) // this "9" means each cell has 9 possible numbers
                    {
                        a.c[r, 0] = 9 * i + j;                  // row-column constraint
                        a.c[r, 1] = (i / 3 * 3 + j / 3) * 9 + k + 81; // box-number constraint
                        a.c[r, 2] = 9 * i + k + 162;            // row-number constraint
                        a.c[r, 3] = 9 * j + k + 243;            // col-number constraint
                        ++r;
                    }

            for (c = 0; c < 324; ++c)
                nr[c] = 0;

            for (r = 0; r < 729; ++r) // generate r[][] from c[][]
                for (c2 = 0; c2 < 4; ++c2)
                {
                    k = a.c[r, c2];
                    a.r[k, nr[k]++] = r;
                }

            return a;
        }

     
        // the sparse representation of the binary matrix
        private class Auxiliary
        {
            public int[,] r = new int[324, 9]; // M(r[c][i], c) is a non-zero element
            public int[,] c = new int[729, 4]; // M(r, c[r][j]) is a non-zero element
        }
    }
}

