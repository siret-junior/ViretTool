using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO
{
    public class KeywordInvertedReader : KeywordInvertedIOBase
    {
        public BinaryReader BaseReader { get; private set; }

        private Dictionary<int, int> _classOffsets;


        public KeywordInvertedReader(string inputFile)
        {
            BaseReader = new BinaryReader(File.Open(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read));

            // check header = 'KS INDEX'+(Int64)-1
            if (BaseReader.ReadInt64() != 0x4b5320494e444558 && /* why? */ BaseReader.ReadInt64() != -1)
            {
                throw new InvalidDataException("Invalid index file format.");
            }
            
            // read offests of each class
            _classOffsets = new Dictionary<int, int>();
            while (true)
            {
                int classId = BaseReader.ReadInt32();
                int classOffset = BaseReader.ReadInt32();

                if (classId == -1)
                {
                    break;
                }
                _classOffsets.Add(classId, classOffset);
            }
        }


        public List<(int frameId, float frameRank)> ReadFrameRanks(int synsetId)
        {
            // TODO: generic cache for all descriptor readers
            //if (mClassCache.ContainsKey(classId))
            //    return mClassCache[classId];

            if (!_classOffsets.ContainsKey(synsetId))
            {
                throw new ArgumentOutOfRangeException("Class ID is incorrect.");
            }

            // TODO: move to property + constructor parameter
            int LIST_DEFAULT_SIZE = 32768;
            List<(int frameId, float rank)> result = new List<(int frameId, float rank)>(LIST_DEFAULT_SIZE);

            BaseReader.BaseStream.Seek(_classOffsets[synsetId], SeekOrigin.Begin);

            // read frame ranks until stop flag (-1)
            while (true)
            {
                int frameId = BaseReader.ReadInt32();
                float frameRank = BaseReader.ReadSingle();

                if (frameId != -1)
                {
                    result.Add((frameId, frameRank));
                }
                else break;
            }
            
            // TODO: cache (in higher level)
            //if (mClassCache.Count == MAX_CACHE_SIZE)
            //{
            //    for (int i = 0; i < CACHE_DELETE; i++)
            //    {
            //        var randClass = mClassCache.Keys.ToList()[mRandom.Next(mClassCache.Count)];
            //        mClassCache.Remove(randClass);
            //    }
            //}
            //mClassCache.Add(synsetId, result);

            return result;
        }


        public override void Dispose()
        {
            BaseReader.Dispose();
        }
    }
}
