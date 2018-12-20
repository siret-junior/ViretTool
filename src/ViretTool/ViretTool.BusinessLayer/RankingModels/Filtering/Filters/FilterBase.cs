using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Filtering.Filters
{
    public abstract class FilterBase
    {
        public bool[] Mask { get; protected set; }

        protected bool[] _includeMask;
        protected bool[] _excludeMask;
        
        public bool[] IncludeMask
        {
            get
            {
                return _includeMask;
            }
            protected set
            {
                _includeMask = value;
                _excludeMask = InvertMask(_includeMask, _excludeMask);
            }
        }
        public bool[] ExcludeMask
        {
            get
            {
                return _excludeMask;
            }
            protected set
            {
                _excludeMask = value;
                _includeMask = InvertMask(_excludeMask, _includeMask);
            }
        }


        public FilterBase(bool[] includeMask)
        {
            IncludeMask = includeMask;
            Mask = IncludeMask;
        }


        public void Ignore()
        {
            Mask = null;
        }

        private static bool[] InvertMask(bool[] inputMask, bool[] outputMask)
        {
            // null inversion is null
            if (inputMask == null)
            {
                return null;
            }

            // reallocate inverted mask if lengths do not match
            if (outputMask == null || inputMask.Length != outputMask.Length)
            {
                outputMask = new bool[inputMask.Length];
            }
            
            // invert mask
            Parallel.For(0, inputMask.Length, index =>
            {
                outputMask[index] = !inputMask[index];
            });

            return outputMask;
        }
    }
}
