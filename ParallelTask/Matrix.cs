using System;
using System.Text;
using System.Threading.Tasks;

namespace ParallelTask
{
    public class Matrix
    {
        private double[,] innerMatrix;
        private int rowsCount;
        private int columnsCount;

        public Matrix(int rowsCount, int columnsCount)
        {
            if (rowsCount < 1 || columnsCount < 1)
            {
                throw new ArgumentException("Matrix must have at least 1 row and 1 column");
            }

            this.rowsCount = rowsCount;
            this.columnsCount = columnsCount;
            innerMatrix = new double[rowsCount, columnsCount];
        }

        public Matrix(double[,] twoDimArray)
        {
            var rowsCount = twoDimArray.GetLength(0);
            var columnsCount = twoDimArray.GetLength(1);

            if (rowsCount < 1 || columnsCount < 1)
            {
                throw new ArgumentException("Matrix must have at least 1 row and 1 column");
            }

            this.rowsCount = rowsCount;
            this.columnsCount = columnsCount;
            innerMatrix = new double[rowsCount, columnsCount];

            for (int i = 0; i < rowsCount; i++)
            {
                for (int j = 0; j < columnsCount; j++)
                {
                    innerMatrix[i, j] = twoDimArray[i, j];
                }
            }
        }

        public double this[int i, int j]
        {
            get
            {
                if (i >= this.rowsCount || j >= this.columnsCount)
                {
                    throw new IndexOutOfRangeException();
                }

                return innerMatrix[i, j];
            }

            set
            {
                if (i >= this.rowsCount || j >= this.columnsCount)
                {
                    throw new IndexOutOfRangeException();
                }

                innerMatrix[i, j] = value;
            }
        }

        /// <summary>
        /// Overload to sequentially multiply matrices without any parallelism
        /// </summary>
        /// <param name="A"> Left matrix </param>
        /// <param name="B"> Right matrix </param>
        /// <returns> Result of multiplication </returns>
        public static Matrix operator *(Matrix A, Matrix B)
        {
            var AColumnsCount = A.columnsCount;

            if (AColumnsCount != B.rowsCount)
            {
                throw new ArgumentException("This matrices have incorrect sizes and can't be multiplied");
            }

            var ABRowsCount = A.rowsCount;
            var ABColumnsCount = B.columnsCount;
            var AB = new Matrix(ABRowsCount, ABColumnsCount);

            for (int i = 0; i < ABRowsCount; i++)
            {
                for (int k = 0; k < AColumnsCount; k++)
                {
                    for (int j = 0; j < ABColumnsCount; j++)
                    {
                        AB[i, j] += A[i, k] * B[k, j];
                    }
                }
            }

            return AB;
        }

        /// <summary>
        /// Creates Environment.ProcessorCount tasks and distributes rows evenly(or almost)
        /// to multiply matrices in parallel
        /// </summary>
        /// <param name="A"> Left matrix </param>
        /// <param name="B"> Right matrix </param>
        /// <returns> Result of multiplication </returns>
        public static Matrix ParallelMultiplyWithTask(Matrix A, Matrix B)
        {
            var AColumnsCount = A.columnsCount;

            if (AColumnsCount != B.rowsCount)
            {
                throw new ArgumentException("This matrices have incorrect sizes and can't be multiplied");
            }

            var ABRowsCount = A.rowsCount;
            var ABColumnsCount = B.columnsCount;
            var AB = new Matrix(ABRowsCount, ABColumnsCount);

            var tasksCount = Environment.ProcessorCount;
            var taskRows = AB.rowsCount / tasksCount;


            var multiplyingTasks = new Task[tasksCount];

            for (int i = 0; i < tasksCount; i++)
            {
                var rStart = i * taskRows;
                var rEnd = (i + 1) * taskRows;

                if (i == tasksCount - 1)
                {
                    rEnd += AB.rowsCount % tasksCount;
                }

                multiplyingTasks[i] = new Task(() =>
                {
                    for (int r = rStart; r < rEnd; r++)
                        for (int c = 0; c < ABColumnsCount; c++)
                        {
                            var row = A.GetRow(r);
                            var column = B.GetColumn(c);
                            AB[r, c] = (row * column)[0, 0];
                        }
                });
            }

            foreach (var task in multiplyingTasks)
            {
                task.Start();
            }

            foreach (var task in multiplyingTasks)
            {
                task.Wait();
            }

            return AB;
        }

        /// <summary>
        /// Fills matrix with random numbers
        /// </summary>
        /// <param name="minNumber"> Sets lower bound for random numbers</param>
        /// <param name="maxNumber"> Sets upper bound for random numbers</param>
        /// <returns> Matrix with all numbers randomized </returns>
        public Matrix RandomlyRefillMatrix(double minNumber = 0, double maxNumber = 1)
        {
            var randomizedMatrix = new Matrix(rowsCount, columnsCount);
            var rngGen = new Random();
            for (int i = 0; i < this.rowsCount; i++)
            {
                for (int j = 0; j < this.columnsCount; j++)
                {
                    randomizedMatrix[i, j] = rngGen.NextDouble() * (maxNumber - minNumber) + minNumber;
                }
            }

            return randomizedMatrix;
        }

        /// <summary>
        /// Multiplies matrices with the help of Parallel.For
        /// </summary>
        /// <param name="A"> Left matrix </param>
        /// <param name="B"> Right matrix </param>
        /// <returns> Result of multiplication </returns>
        public static Matrix ParallelMultiplyWithFor(Matrix A, Matrix B)
        {
            var AColumnsCount = A.columnsCount;

            if (AColumnsCount != B.rowsCount)
            {
                throw new ArgumentException("This matrices have incorrect sizes and can't be multiplied");
            }

            var ABRowsCount = A.rowsCount;
            var ABColumnsCount = B.columnsCount;
            var AB = new Matrix(ABRowsCount, ABColumnsCount);

            Parallel.For(0, ABRowsCount, i =>
            {
                Parallel.For(0, AColumnsCount, k =>
                {
                    Parallel.For(0, ABColumnsCount, j =>
                    {
                        AB[i, j] += A[i, k] * B[k, j];
                    });
                });
            });

            return AB;
        }

        /// <summary>
        /// Creates N*M tasks for NxK and KxM matrices to multiply matrices A and B.
        /// Works worse than not parallel version
        /// </summary>
        /// <param name="A"> Left matrix </param>
        /// <param name="B"> Right matrix </param>
        /// <returns> Result of multiplication </returns>
        public static Matrix BadParallelMultiplyWithTask(Matrix A, Matrix B)
        {
            var AColumnsCount = A.columnsCount;

            if (AColumnsCount != B.rowsCount)
            {
                throw new ArgumentException("This matrices have incorrect sizes and can't be multiplied");
            }

            var ABRowsCount = A.rowsCount;
            var ABColumnsCount = B.columnsCount;
            var AB = new Matrix(ABRowsCount, ABColumnsCount);

            var multiplyingTasks = new Task[ABRowsCount, ABColumnsCount];

            for (int i = 0; i < ABRowsCount; i++)
            {
                for (int j = 0; j < ABColumnsCount; j++)
                {
                    var iLocal = i;
                    var jLocal = j;
                    var row = A.GetRow(iLocal);
                    var column = B.GetColumn(jLocal);
                    multiplyingTasks[iLocal, jLocal] = new Task(() =>
                    {
                        AB[iLocal, jLocal] = (row * column)[0, 0];
                    });
                }
            }

            foreach (var task in multiplyingTasks)
            {
                task.Start();
            }

            foreach (var task in multiplyingTasks)
            {
                task.Wait();
            }

            return AB;
        }

        /// <summary>
        /// Get a column from a matrix
        /// </summary>
        /// <param name="j"> Column number </param>
        /// <returns> Matrix with 1 desired column </returns>
        private Matrix GetColumn(int j)
        {
            var column = new Matrix(rowsCount, 1);

            for (int i = 0; i < rowsCount; i++)
            {
                column[i, 0] = this[i, j];
            }

            return column;
        }

        /// <summary>
        /// Gets a row from a matrix
        /// </summary>
        /// <param name="i"> Row number </param>
        /// <returns> Matrix with 1 desired row </returns>
        private Matrix GetRow(int i)
        {
            var row = new Matrix(1, columnsCount);

            for (int j = 0; j < columnsCount; j++)
            {
                row[0, j] = this[i, j];
            }

            return row;
        }

        public override string ToString()
        {
            var matrixStringBuilder = new StringBuilder();

            for (int i = 0; i < this.rowsCount; i++)
            {
                for (int j = 0; j < this.columnsCount; j++)
                {
                    var appendString = string.Format("{0:0.00} ", innerMatrix[i, j]);
                    matrixStringBuilder.Append(appendString);
                }

                matrixStringBuilder.Append("\n");
            }

            return matrixStringBuilder.ToString();
        }
    }
}
