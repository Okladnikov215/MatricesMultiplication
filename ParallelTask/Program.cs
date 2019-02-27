using System;
using System.Diagnostics;

namespace ParallelTask
{
    public class Program
    {
        static void Main(string[] args)
        {
            var squareMatrixSide = 400;
            var A = new Matrix(squareMatrixSide, squareMatrixSide).RandomlyRefillMatrix(-100, 100);
            var B = new Matrix(squareMatrixSide, squareMatrixSide).RandomlyRefillMatrix();

            var AB = new Matrix(squareMatrixSide, squareMatrixSide);

            Matrix nonParallelMult(Matrix M1, Matrix M2) => M1 * M2;
            Matrix parallelMultWithFor(Matrix M1, Matrix M2) => Matrix.ParallelMultiplyWithFor(M1, M2);
            Matrix parallelMultWithTask(Matrix M1, Matrix M2) => Matrix.ParallelMultiplyWithTask(M1, M2);


            var nonParallelTime = TimeCheckMatrixMultiplication(nonParallelMult, A, B);
            var minTime = nonParallelTime;

            var parallelForTime = TimeCheckMatrixMultiplication(parallelMultWithFor, A, B);
            minTime = Math.Min(minTime, parallelForTime);

            var parallelTaskTime = TimeCheckMatrixMultiplication(parallelMultWithTask, A, B);
            minTime = Math.Min(minTime, parallelTaskTime);

            Console.WriteLine("Maximum boost is ~{0:0.00}% with {1} processors detected", 100 - minTime / nonParallelTime * 100, Environment.ProcessorCount);
        }

        /// <summary>
        /// Multiplies matrices A and B using passed function mult and returns spent time
        /// </summary>
        /// <param name="mult"> Function which defines how matrices are multiplied</param>
        /// <param name="A"> Left matrix </param>
        /// <param name="B"> Right matrix </param>
        /// <returns> Time spent in seconds </returns>
        private static double TimeCheckMatrixMultiplication(Func<Matrix, Matrix, Matrix> mult, Matrix A, Matrix B)
        {
            var methodName = mult.Method.Name;
            var ARows = A.RowsCount;
            var AColumns = A.ColumnsCount;
            var BRows = B.RowsCount;
            var BColumns = B.ColumnsCount;
            var sw = new Stopwatch();
            Console.WriteLine("Begin {4} for Matrices {0}x{1} and {2}x{3}", ARows, AColumns, BRows, BColumns, methodName);
            var AB = new Matrix(ARows, BColumns);
            GC.Collect();

            sw.Start();
            AB = mult(A, B);
            sw.Stop();

            var swTime = sw.Elapsed.TotalSeconds;
            Console.WriteLine("Time elapsed: " + swTime + "\n");
            return swTime;
        }
    }
}
