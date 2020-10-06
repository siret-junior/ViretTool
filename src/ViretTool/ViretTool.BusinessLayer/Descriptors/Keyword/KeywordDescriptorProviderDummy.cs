namespace ViretTool.BusinessLayer.Descriptors.Keyword
{
    internal class KeywordDescriptorProviderDummy : IDescriptorProvider<(int synsetId, float probability)[]>
    {
        public byte[] DatasetHeader => new byte[0];

        public int DescriptorCount => 0;

        public int DescriptorLength => 0;

        public (int synsetId, float probability)[][] Descriptors { get; } = new (int synsetId, float probability)[0][];

        public (int synsetId, float probability)[] this[int index] => GetDescriptor(index);

        public (int synsetId, float probability)[] GetDescriptor(int index)
        {
            return new (int, float)[0];
        }
    }
}
