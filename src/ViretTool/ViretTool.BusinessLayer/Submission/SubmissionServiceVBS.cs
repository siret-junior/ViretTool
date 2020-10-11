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
    public class SubmissionServiceVBS : SubmissionServiceBase
    {
        public SubmissionServiceVBS(IInteractionLogger interactionLogger, ILogger logger, IDatasetServicesManager datasetServicesManager)
            : base(interactionLogger, logger, datasetServicesManager)
        { }


        protected override string GetUrlForSubmission(int teamId, int memberId, FrameToSubmit frameToSubmit)
        {
            return $"{SubmissionUrl}?" +
                $"team={teamId}" +
                $"&member={memberId}" +
                $"&video={frameToSubmit.VideoId + 1}" +
                $"&frame={frameToSubmit.FrameNumber}" +
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


            ResultVBS[] results = resultSet.Take(MAX_RESULTS_COUNT)
                .Select((item, index) => new ResultVBS(
                    // videos are indexed from 1
                    _datasetServicesManager.CurrentDataset.DatasetService.GetVideoIdForFrameId(item.Id) + 1,
                    _datasetServicesManager.CurrentDataset.DatasetService.GetFrameNumberForFrameId(item.Id),
                    item.Rank,
                    index
                    ))
                .ToArray();

            return new ResultLogVBS(query, results);
        }
    }
}
