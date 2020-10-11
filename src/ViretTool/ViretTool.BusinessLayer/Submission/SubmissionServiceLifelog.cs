using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using ViretTool.BusinessLayer.ActionLogging;
using ViretTool.BusinessLayer.Descriptors.Models;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.ResultLogging;
using ViretTool.BusinessLayer.Services;

namespace ViretTool.BusinessLayer.Submission
{
    public class SubmissionServiceLifelog : SubmissionServiceBase
    {
        public SubmissionServiceLifelog(IInteractionLogger interactionLogger, ILogger logger, IDatasetServicesManager datasetServicesManager)
            : base(interactionLogger, logger, datasetServicesManager)
        { }

        protected override string GetUrlForSubmission(int teamId, int memberId, FrameToSubmit frameToSubmit)
        {
            int frameId = _datasetServicesManager.CurrentDataset.DatasetService.GetFrameIdForFrameNumber(frameToSubmit.VideoId, frameToSubmit.FrameNumber);
            LifelogFrameMetadata metadata = _datasetServicesManager.CurrentDataset.LifelogDescriptorProvider[frameId];
            return $"{SubmissionUrl}?" +
                $"team={teamId}" +
                $"&member={memberId}" +
                $"&item={metadata.FileName}" +
                $"&session={SessionId}"; 
        }

        protected override IResultLog GetResultLog(BiTemporalQuery query, BiTemporalRankedResultSet biTemporalResultSet)
        {
            List<PairedRankedFrame> resultSet;
            switch (biTemporalResultSet.TemporalQuery.PrimaryTemporalQuery)
            {
                case BiTemporalQuery.TemporalQueries.Former:
                    resultSet = biTemporalResultSet.FormerTemporalResultSet;
                    break;
                case BiTemporalQuery.TemporalQueries.Latter:
                    resultSet = biTemporalResultSet.LatterTemporalResultSet;
                    break;
                default:
                    throw new NotImplementedException("Unknown primary temporal query.");
            }


            ResultLifelog[] results = resultSet.Take(MAX_RESULTS_COUNT)
                .Select((item, index) => new ResultLifelog(
                    _datasetServicesManager.CurrentDataset.LifelogDescriptorProvider[item.Id].FileName,
                    item.Rank,
                    index
                    ))
                .ToArray();

            return new ResultLogLifelog(query, results);
        }
    }
}
