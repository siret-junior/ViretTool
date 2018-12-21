using System;
using System.Collections.Generic;

namespace ViretTool.BusinessLayer.OutputGridSorting
{
    class GridSorterGeneral
    {
        private const int Iterations = 1000;

        // returns indexes of data organized in a 2D grid
        public int[,] SortItems(List<float[]> data, int width, int height)
        {
            int dataCount = data.Count;
            if (dataCount != width * height)
            {
                throw new ArgumentException("Cannot sort data outside of given grid!");
            }

            // initialize grid            
            int[,] grid = new int[width, height];
            dataCount = 0;
            for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                grid[i, j] = dataCount++;
            }

            // organize grid
            int x1 = 0, x2 = 0, y1 = 0, y2 = 0;
            float scoreBefore1 = 0, scoreBefore2 = 0;
            bool keepItem1 = false, keepItem2 = false;
            for (int iteration = 0; iteration < Iterations; iteration++)
            {
                Random r = new Random(iteration);
                int size = 5;
                if (iteration < 30000)
                {
                    size = 10;
                }

                if (!keepItem1)
                {
                    x1 = r.Next(width);
                    y1 = r.Next(height);
                    scoreBefore1 = GetSimilarityScore(grid, data, x1, y1, grid[x1, y1], width, height, size);
                    //while (scoreBefore1 < 32)
                    //{
                    //    x1 = r.Next(width); y1 = r.Next(height);
                    //    scoreBefore1 = GetSimilarityScore(grid, distances, x1, y1, grid[x1, y1], width, height, size);
                    //}
                }

                keepItem1 = false;

                if (!keepItem2)
                {
                    x2 = r.Next(width);
                    y2 = r.Next(height);
                    scoreBefore2 = GetSimilarityScore(grid, data, x2, y2, grid[x2, y2], width, height, size);
                    //while (scoreBefore2 < 32)
                    //{
                    //    x2 = r.Next(width); y2 = r.Next(height);
                    //    scoreBefore2 = GetSimilarityScore(grid, distances, x2, y2, grid[x2, y2], width, height, size);
                    //}
                }

                keepItem2 = false;

                float scoreAfter = GetSimilarityScore(grid, data, x1, y1, grid[x2, y2], width, height, size);
                scoreAfter += GetSimilarityScore(grid, data, x2, y2, grid[x1, y1], width, height, size);

                if (scoreBefore1 + scoreBefore2 > scoreAfter)
                {
                    int id = grid[x1, y1];
                    grid[x1, y1] = grid[x2, y2];
                    grid[x2, y2] = id;
                }
                else if (scoreBefore1 != scoreBefore2)
                {
                    // keep item that should be changed
                    keepItem1 = scoreBefore1 > 1.5 * scoreBefore2;
                    keepItem2 = scoreBefore2 > 1.5 * scoreBefore1;
                }
            }

            return grid;
        }

        // for normalized vectors !!
        private static float DotProductL2Distance(float[] v1, float[] v2)
        {
            float result = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                result += v1[i] * v2[i];
            }

            return 2 - 2 * result;
        }

        private static float GetSimilarityScore(int[,] grid, List<float[]> data, int x, int y, int ID, int width, int height, int size)
        {
            int x1 = Math.Max(0, x - size), y1 = Math.Max(0, y - size);
            int x2 = Math.Min(width - 1, x + size), y2 = Math.Min(height - 1, y + size);

            // ignore distance from tested item (ID) to item in grid[x, y], which can be swapped

            // compute similarity
            float result = 0;
            for (int i = x1; i < x2; i += 1)
            for (int j = y1; j < y2; j += 1)
            {
                if (i == x && j == y)
                {
                    continue;
                }

                result += DotProductL2Distance(data[ID], data[grid[i, j]]);
            }

            return result / ((x2 - x1) * (y2 - y1) - 1);
        }

        private static float L2(float[] v1, float[] v2)
        {
            float result = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                result += (v1[i] - v2[i]) * (v1[i] - v2[i]);
            }

            return (float)Math.Sqrt(result);
        }
    }
}
