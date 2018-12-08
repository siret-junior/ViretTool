using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Descriptor
{
    interface IDescriptorService<T>
    {
        byte[] DatasetHeader { get; }

        T[] Descriptors { get; }
        T GetDescriptor(int index);
        T this[int index]
        {
            get;
        }
    }
}
