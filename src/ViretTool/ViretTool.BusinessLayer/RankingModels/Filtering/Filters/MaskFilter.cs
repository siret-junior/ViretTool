using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    /// <summary>
    /// TODO: precompute various mask combinations (2^(n masks + 1))
    /// </summary>
    public abstract class MaskFilter
    {
        bool[] _mask;
        public bool[] Mask
        {
            get
            {
                return _mask;
            }
            protected set
            {
                _mask = value;
                MaskInverted = InvertMask(_mask);
            }
        }
        public bool[] MaskInverted { get; private set; }


        public MaskFilter(bool[] mask)
        {
            Mask = mask;
        }

        public void Clear()
        {
            SetMaskTo(_mask, true);
            Mask = _mask;
        }
        
        private static void SetMaskTo(bool[] mask, bool value)
        {
            Parallel.For(0, mask.Length, index =>
            {
                mask[index] = value;
            });
        }

        private static bool[] InvertMask(bool[] mask)
        {
            if (mask == null)
            {
                return null;
            }

            bool[] result = new bool[mask.Length];
            Parallel.For(0, mask.Length, index =>
            {
                result[index] = !mask[index];
            });
            return result;
        }
    }
}
