using System.Collections.Generic;
using System.IO;
using System.Linq;
using ViretTool.DataLayer.DataIO.InitialDisplay;

namespace ViretTool.BusinessLayer.Services
{
    public interface IInitialDisplayProvider
    {
        IReadOnlyList<int> InitialDisplayIds { get; }
    }

    public class InitialDisplayProvider : IInitialDisplayProvider
    {
        public InitialDisplayProvider(IDatasetParameters datasetParameters, string datasetDirectory)
        {
            InitialDisplayIds = datasetParameters.IsInitialDisplayPrecomputed
                                    ? new InitialDisplayReader().ReadInitialIds(Path.Combine(datasetDirectory, datasetParameters.InitialDisplayFileName)).ToArray()
                                    : new int[0];
        }

        public IReadOnlyList<int> InitialDisplayIds { get; }
    }
}
