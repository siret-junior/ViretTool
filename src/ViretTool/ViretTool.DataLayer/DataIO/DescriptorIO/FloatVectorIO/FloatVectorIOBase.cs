using System;

namespace ViretTool.DataLayer.DataIO.DescriptorIO.FloatVectorIO
{
    public abstract class FloatVectorIOBase : IDisposable
    {
        public const string FLOAT_VECTOR_EXTENSION = ".w2vv";

        public abstract void Dispose();
    }
}
