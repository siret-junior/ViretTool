using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.BusinessLayer.OutputGridSorting
{
    public class GridSorterFast : IGridSorter
    {
        private const int Iterations = 500000;
        private readonly IDatasetServicesManager _datasetServicesManager;

        public GridSorterFast(IDatasetServicesManager datasetServicesManager)
        {
            _datasetServicesManager = datasetServicesManager;
        }

        public async Task<int[]> GetSortedFrameIdsAsync(IList<int> topFrameIds, int columnCount, CancellationTokenSource cancellationTokenSource)
        {
            CancellationToken cancellationToken = cancellationTokenSource.Token;
            return await Task.Run(() => GetSortedFrameIds(topFrameIds, columnCount, cancellationToken), cancellationToken);
        }

        private int[] GetSortedFrameIds(IList<int> topFrameIds, int columnCount, CancellationToken cancellationToken)
        {
            List<float[]> data = topFrameIds.Select(_datasetServicesManager.CurrentDataset.SemanticVectorProvider.GetDescriptor).ToList();
            int width = columnCount;
            int height = data.Count / columnCount;
            //we ignore items out of the grid
            int[,] sortedFrames = SortItems(data.Take(width * height).ToList(), width, height, cancellationToken);

            int[] result = new int[width * height];
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    result[i * width + j] = topFrameIds[sortedFrames[j, i]];
                }
            }

            return result;
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

        private static float GetSimilarityScore(int[,] grid, float[,] distances, int x, int y, int ID, int width, int height, int size)
        {
            int x1 = Math.Max(0, x - size), y1 = Math.Max(0, y - size);
            int x2 = Math.Min(width - 1, x + size), y2 = Math.Min(height - 1, y + size);

            // ignore distance from tested item (ID) to item in grid[x, y], which can be swapped
            float xy = distances[ID, grid[x, y]];
            distances[ID, grid[x, y]] = 0;
            distances[grid[x, y], ID] = 0;

            // compute similarity
            float result = 0;
            for (int i = x1; i < x2; i += 1)
            for (int j = y1; j < y2; j += 1)
            {
                result += distances[ID, grid[i, j]];
            }

            // restore distance
            distances[ID, grid[x, y]] = xy;
            distances[grid[x, y], ID] = xy;

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

        // returns indexes of data organized in a 2D grid
        private int[,] SortItems(List<float[]> data, int width, int height, CancellationToken cancellationToken)
        {
            int dataCount = data.Count;
            if (dataCount != width * height)
            {
                return null;
            }

            // compute distances
            float[,] distances = new float[data.Count, data.Count];
            Parallel.For(
                0,
                dataCount,
                new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 },
                (i, state) =>
                {
                    distances[i, i] = 0;
                    for (int j = i + 1; j < dataCount; j++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        distances[i, j] = L2(data[i], data[j]);
                        //distances[i, j] = DotProductL2Distance(data[i], data[j]);
                        distances[j, i] = distances[i, j];
                    }
                });

            // initialize grid            
            int[,] grid = new int[width, height];
            dataCount = 0;
            for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
            {
                grid[i, j] = dataCount++;
            }

            // organize grid
            int swaps = 0, swapOld = 0;
            int x1 = 0, x2 = 0, y1 = 0, y2 = 0;
            float scoreBefore1 = 0, scoreBefore2 = 0, scoreAfter;
            bool keepItem1 = false, keepItem2 = false;
            for (int iteration = 0; iteration < Iterations; iteration++)
            {
                cancellationToken.ThrowIfCancellationRequested();

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
                    scoreBefore1 = GetSimilarityScore(grid, distances, x1, y1, grid[x1, y1], width, height, size);
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
                    scoreBefore2 = GetSimilarityScore(grid, distances, x2, y2, grid[x2, y2], width, height, size);
                    //while (scoreBefore2 < 32)
                    //{
                    //    x2 = r.Next(width); y2 = r.Next(height);
                    //    scoreBefore2 = GetSimilarityScore(grid, distances, x2, y2, grid[x2, y2], width, height, size);
                    //}
                }

                keepItem2 = false;

                scoreAfter = GetSimilarityScore(grid, distances, x1, y1, grid[x2, y2], width, height, size);
                scoreAfter += GetSimilarityScore(grid, distances, x2, y2, grid[x1, y1], width, height, size);

                if (scoreBefore1 + scoreBefore2 > scoreAfter)
                {
                    int id = grid[x1, y1];
                    grid[x1, y1] = grid[x2, y2];
                    grid[x2, y2] = id;
                    swaps++;
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
    }
}
