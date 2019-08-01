using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViretTool.DataLayer.DataIO.DescriptorIO.KeywordIO;

namespace ViretTool.BusinessLayer.Descriptors
{
    public class KeywordDescriptorProvider : IDescriptorProvider<(int synsetId, float probability)[]>, IDisposable
    {
        // TODO: remove or implement
        public byte[] DatasetHeader => throw new NotImplementedException();
        public int DescriptorCount => throw new NotImplementedException();
        public int DescriptorLength => throw new NotImplementedException();
        public (int synsetId, float probability)[][] Descriptors => throw new NotImplementedException();

        private readonly Dictionary<int, (int synsetId, int probability)> _descriptorCache;
        private readonly KeywordInvertedReader _reader;

        public KeywordDescriptorProvider(string inputFile)
        {
            _descriptorCache = new Dictionary<int, (int synsetId, int probability)>();
            _reader = new KeywordInvertedReader(inputFile);
        }

        public void Dispose()
        {
            _reader.Dispose();
        }


        public (int synsetId, float probability)[] this[int frameId]
        {
            get => GetDescriptor(frameId);
        }

        public (int synsetId, float probability)[] GetDescriptor(int frameId)
        {
            return _reader.
        }

        public (int frameId, float probability)[] GetDescriptorInverted(int synsetId)
        {
            throw new NotImplementedException();
        }
    }
}
