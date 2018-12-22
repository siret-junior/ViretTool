using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels.Temporal
{
    public class BiTemporalRanking
    {
        public float[] PrimaryRanks { get; set; }
        public int[] PrimaryMatchingSecondaries { get; set; }
        
        public float[] SecondaryRanks { get; set; }
        public int[] SecondaryMatchingPrimaries { get; set; }


        public bool IsUpdatedPrimary { get; set; }
        public bool IsUpdatedSecondary { get; set; }
        public bool IsUpdated => IsUpdatedPrimary || IsUpdatedSecondary;


        public BiTemporalRanking(float[] primaryRanks, float[] secondaryRanks, bool isUpdated = true)
        {
            PrimaryRanks = primaryRanks;
            SecondaryRanks = secondaryRanks;

            IsUpdatedPrimary = isUpdated;
            IsUpdatedSecondary = isUpdated;
        }


        public static BiTemporalRanking Zeros(int itemCount)
        {
            return Value(itemCount, 0);
        }

        public static BiTemporalRanking Ones(int itemCount)
        {
            return Value(itemCount, 1);
        }

        public static BiTemporalRanking Value(int itemCount, float value)
        {
            return new BiTemporalRanking(
                Enumerable.Repeat(value, itemCount).ToArray(), 
                Enumerable.Repeat(value, itemCount).ToArray());
        }
    }
}
