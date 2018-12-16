using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptors
{
    public interface IDescriptorProvider<T>
    {
        byte[] DatasetHeader { get; }

        int DescriptorCount { get; }
        int DescriptorLength { get; }

        T[] Descriptors { get; }
        T GetDescriptor(int index);
        T this[int index]
        {
            get;
        }
    }
}
