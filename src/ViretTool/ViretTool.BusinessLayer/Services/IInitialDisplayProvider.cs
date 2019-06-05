using System.Collections.Generic;
using System.IO;
using System.Linq;
using ViretTool.DataLayer.DataIO.InitialDisplay;

namespace ViretTool.BusinessLayer.Services
{
    public interface IInitialDisplayProvider
    {
        IReadOnlyList<int> InitialDisplayIds { get; }

        int RowCount { get; }
        int ColumnCount { get; }
    }

    public class InitialDisplayProvider : IInitialDisplayProvider
    {
        public InitialDisplayProvider(IDatasetParameters datasetParameters, string datasetDirectory)
        {
            if (datasetParameters.IsInitialDisplayPrecomputed)
            {
                InitialDisplayReader initialDisplayReader = new InitialDisplayReader();
                string filePath = Path.Combine(datasetDirectory, datasetParameters.InitialDisplayFileName);
                InitialDisplayIds = initialDisplayReader.ReadInitialIds(filePath).ToArray();
                RowCount = initialDisplayReader.ReadRowCount(filePath);
                ColumnCount = initialDisplayReader.ReadColumnCount(filePath);
            }
            else
            {
                InitialDisplayIds = new int[0];
            }
        }

        public IReadOnlyList<int> InitialDisplayIds { get; }
        public int RowCount { get; }
        public int ColumnCount { get; }
    }
}
