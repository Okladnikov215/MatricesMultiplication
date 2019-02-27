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


            var nonParallelTime = TimeCheckMatrixMultiplication(nonParallelMult, squareMatrixSide, A, B, out AB);
            var minTime = nonParallelTime;

            var parallelForTime = TimeCheckMatrixMultiplication(parallelMultWithFor, squareMatrixSide, A, B, out AB);
            minTime = Math.Min(minTime, parallelForTime);

            var parallelTaskTime = TimeCheckMatrixMultiplication(parallelMultWithTask, squareMatrixSide, A, B, out AB);
            minTime = Math.Min(minTime, parallelTaskTime);

            Console.WriteLine("Maximum boost is ~{0:0.00}% with {1} processors detected", 100 - minTime / nonParallelTime * 100, Environment.ProcessorCount);
        }

        private static double TimeCheckMatrixMultiplication(Func<Matrix, Matrix, Matrix> mult, int squareMatrixSide, Matrix A, Matrix B, out Matrix AB)
        {
            var methodName = mult.Method.Name;

            var sw = new Stopwatch();
            Console.WriteLine("Begin {1} for Square Matrices {0}x{0}", squareMatrixSide, methodName);
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
