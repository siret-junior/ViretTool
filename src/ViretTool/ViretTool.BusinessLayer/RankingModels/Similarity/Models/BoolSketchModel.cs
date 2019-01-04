using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.BusinessLayer.RankingModels.Filtering.Filters;
using ViretTool.BusinessLayer.RankingModels.Queries;
using ViretTool.DataLayer.DataIO.DescriptorIO.BoolSignatureIO;

namespace ViretTool.BusinessLayer.RankingModels.Similarity.Models
{
    public class BoolSketchModel : IBoolSketchModel, IFaceSketchModel, ITextSketchModel
    {
        public IRankFusion RankFusion { get; }

        public ColorSketchQuery CachedQuery { get; private set; }

        public RankingBuffer InputRanking { get; private set; }
        public RankingBuffer OutputRanking { get; private set; }

        public bool[][] Descriptors { get; }

        public BoolSketchModel(IRankFusion rankFusion, bool[][] boolSignatures)
        {
            RankFusion = rankFusion;
            Descriptors = boolSignatures;
        }


        public void ComputeRanking(ColorSketchQuery query, RankingBuffer InputRanking, RankingBuffer OutputRanking)
        {
            throw new NotImplementedException();
        }


        //public static BoolSketchModel FromDirectory(string inputDirectory, string extension)
        //{
        //    string filterFilename = Directory.GetFiles(inputDirectory)
        //        .Where(file => file.EndsWith(extension)).First();

        //    Descriptors = new bool[DescriptorCount][];
        //    for (int i = 0; i < DescriptorCount; i++)
        //    {
        //        Descriptors[i] = reader.ReadDescriptor(i);
        //    }

        //    bool[][] filterAttributes = BoolSignatureReader.ReadFilter(filterFilename);
        //    BoolSketchModel thresholdFilter = new BoolSketchModel(filterAttributes);
        //    return thresholdFilter;
        //}
    }
}
