using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.RankingModels
{
    public class Ranking
    {
        public string Name { get; set; }
        public float[] Ranks { get; set; }
        public bool IsUpdated { get; set; }


        public Ranking(float[] ranks, bool isUpdated = true)
        {
            Ranks = ranks;
            IsUpdated = isUpdated;
        }

        public Ranking(Ranking ranking, bool isUpdated = true)
        {
            Ranks = new float[ranking.Ranks.Length];
            for (int i = 0; i < Ranks.Length; i++)
            {
                Ranks[i] = ranking.Ranks[i];
            }
            IsUpdated = isUpdated;
        }


        public static Ranking Zeros(int itemCount)
        {
            return Value(itemCount, 0);
        }

        public static Ranking Ones(int itemCount)
        {
            return Value(itemCount, 1);
        }

        public static Ranking Value(int itemCount, float value)
        {
            float[] ranks = new float[itemCount];
            for (int i = 0; i < itemCount; i++)
            {
                ranks[i] = value;
            }

            return new Ranking(ranks);
        }

        public override bool Equals(object obj)
        {
            return obj is Ranking ranking &&
                   EqualityComparer<float[]>.Default.Equals(Ranks, ranking.Ranks) &&
                   IsUpdated == ranking.IsUpdated;
        }

        public override int GetHashCode()
        {
            int hashCode = 1733532412;
            hashCode = hashCode * -1521134295 + EqualityComparer<float[]>.Default.GetHashCode(Ranks);
            hashCode = hashCode * -1521134295 + IsUpdated.GetHashCode();
            return hashCode;
        }
    }
}
