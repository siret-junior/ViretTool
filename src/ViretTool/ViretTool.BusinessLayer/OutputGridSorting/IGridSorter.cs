using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.OutputGridSorting
{
    public interface IGridSorter
    {
        Task<int[]> GetSortedFrameIdsAsync(IList<int> topFrameIds, int columnCount, CancellationTokenSource cancellationTokenSource);
    }
}
