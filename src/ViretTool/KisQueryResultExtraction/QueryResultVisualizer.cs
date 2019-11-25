using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Temporal;
using ViretTool.BusinessLayer.RankingModels.Temporal.Queries;
using ViretTool.BusinessLayer.Services;
using ViretTool.BusinessLayer.Thumbnails;

namespace KisQueryResultExtraction
{
    public class Task
    {
        public int taskId;
        public int videoId;
        public int startFrame;
        public int endFrame;
        public long startTimeStamp;
        public long endTimeStamp;

        public Task(dynamic task)
        {
            taskId = task.taskId;
            videoId = task.videoId;
            startFrame = task.startFrame;
            endFrame = task.endFrame;
            startTimeStamp = task.startTimeStamp;
            endTimeStamp = task.endTimeStamp;
        }
    }

    public class QueryResultVisualizer
    {
        public static readonly int Width = 3840;
        public static readonly int Height = 2160;
        public static readonly int ThumbWidth = 100;
        public static readonly int ThumbHeight = 75;
        public static readonly int ThumbMarginX = 5;
        public static readonly int ThumbMarginY = 5;
        public static readonly int StartX = 350;

        public static void Visualize(
            DatasetServices dataset,
            Task task,
            BiTemporalQuery query,
            IBiTemporalRankingModule rankingModule, 
            string outputImageFile)
        {
            using (Bitmap bitmap = new Bitmap(Width, Height))
            using(Graphics gfx = Graphics.FromImage(bitmap))
            {
                gfx.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);

                PrintTask(gfx, dataset, task);

                PrintQueries(gfx, dataset, query, rankingModule);

                PrintResults(gfx, dataset, rankingModule);

                bitmap.Save(outputImageFile, ImageFormat.Jpeg);
            }
            


            //// extract intermediate ranks
            //float[] keywordRanks;
            //float[] colorRanks;
            //float[] semanticExampleRanks;
            //float[] faceSketchRanks;
            //float[] textSketchRanks;
            //float[] fusionRanks;
            //float[] maskFilteringRanks;
            //float[] countFilteringRanks;
            //float[] outputRanks;

            //IBiTemporalRankingService rankingService = dataset.RankingService;
            //IBiTemporalRankingModule rankingModule = rankingService.BiTemporalRankingModule;
            //switch (query.PrimaryTemporalQuery)
            //{
            //    case BiTemporalQuery.TemporalQueries.Former:
            //        keywordRanks = rankingModule.KeywordIntermediateRanking.FormerRankingBuffer.Ranks;
            //        colorRanks = rankingModule.ColorSketchIntermediateRanking.FormerRankingBuffer.Ranks;
            //        semanticExampleRanks = rankingModule.SemanticExampleIntermediateRanking.FormerRankingBuffer.Ranks;
            //        faceSketchRanks = rankingModule.FaceSketchIntermediateRanking.FormerRankingBuffer.Ranks;
            //        textSketchRanks = rankingModule.TextSketchIntermediateRanking.FormerRankingBuffer.Ranks;
            //        fusionRanks = rankingModule.IntermediateFusionRanking.FormerRankingBuffer.Ranks;
            //        maskFilteringRanks = rankingModule.FormerFilteringModule.MaskIntermediateRanking.Ranks;
            //        countFilteringRanks = rankingModule.OutputRanking.FormerRankingBuffer.Ranks;
            //        break;

            //    case BiTemporalQuery.TemporalQueries.Latter:
            //        keywordRanks = rankingModule.KeywordIntermediateRanking.LatterRankingBuffer.Ranks;
            //        colorRanks = rankingModule.ColorSketchIntermediateRanking.LatterRankingBuffer.Ranks;
            //        semanticExampleRanks = rankingModule.SemanticExampleIntermediateRanking.LatterRankingBuffer.Ranks;
            //        faceSketchRanks = rankingModule.FaceSketchIntermediateRanking.LatterRankingBuffer.Ranks;
            //        textSketchRanks = rankingModule.TextSketchIntermediateRanking.LatterRankingBuffer.Ranks;
            //        fusionRanks = rankingModule.IntermediateFusionRanking.LatterRankingBuffer.Ranks;
            //        maskFilteringRanks = rankingModule.LatterFilteringModule.MaskIntermediateRanking.Ranks;
            //        countFilteringRanks = rankingModule.OutputRanking.LatterRankingBuffer.Ranks;
            //        break;

            //    default:
            //        throw new NotImplementedException();
            //}
            ////outputRanks = maskFilteringRanks;
            //outputRanks = countFilteringRanks;



        }

        private static void PrintTask(Graphics gfx, DatasetServices dataset, Task task)
        {
            gfx.DrawString($"Task: {task.taskId}, Video: {task.videoId}, Frames: [{task.startFrame}, {task.endFrame}], Timestamps: [{task.startTimeStamp}, {task.endTimeStamp}]",
                new Font("Arial", 64),
                Brushes.Black,
                30, 30);

            int[] videoFrameSet = dataset.DatasetService.GetFrameIdsForVideo(task.videoId);
            int[] shotFrameSet = videoFrameSet
                .Where(frameId => dataset.DatasetService.GetFrameNumberForFrameId(frameId) >= task.startFrame
                    && dataset.DatasetService.GetFrameNumberForFrameId(frameId) <= task.endFrame)
                .ToArray();

            int columns = (Width - StartX) / (ThumbWidth + ThumbMarginX);
            int startY = 150;

            // video
            for (int i = 0; i < columns && i < videoFrameSet.Length; i++)
            {
                // uniform sampling
                int videoFrameId = videoFrameSet[(int)(((float)i / columns) * videoFrameSet.Length)];
                DrawThumbnail(gfx, dataset, videoFrameId, StartX + i * (ThumbWidth + ThumbMarginX), startY);
            }

            // shot
            for (int i = 0; i < columns && i < shotFrameSet.Length; i++)
            {
                int shotFrameId = shotFrameSet[i];
                DrawThumbnail(gfx, dataset, shotFrameId, StartX + i * (ThumbWidth + ThumbMarginX), startY + ThumbHeight + ThumbMarginY);
            }

            //int topVideoPosition = GetFramesetTopPosition(videoFrameSet, outputRanks);
            //int topShotPosition = GetFramesetTopPosition(shotFrameSet, outputRanks);
        }

        private static void DrawThumbnail(Graphics gfx, DatasetServices dataset, int frameId, int x, int y)
        {
            int videoId = dataset.DatasetService.GetVideoIdForFrameId(frameId);
            int frameNumber = dataset.DatasetService.GetFrameNumberForFrameId(frameId);
            Thumbnail<byte[]> thumbRaw = dataset.ThumbnailService.GetThumbnail(videoId, frameNumber);
            using (Bitmap bitmap = new Bitmap(new MemoryStream(thumbRaw.Image)))
            {
                gfx.DrawImage(bitmap, x, y, ThumbWidth, ThumbHeight);
            }
        }

        private static void PrintQueries(Graphics gfx, DatasetServices dataset, BiTemporalQuery query, IBiTemporalRankingModule rankingModule)
        {
            //throw new NotImplementedException();
        }

        private static void PrintResults(Graphics gfx, DatasetServices dataset, IBiTemporalRankingModule rankingModule)
        {
            //throw new NotImplementedException();
        }
    }
}
