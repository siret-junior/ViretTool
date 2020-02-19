using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViretTool.BusinessLayer.Services
{
    class CosineSimilarityHelper
    {
        public static float CosineSimilarityNormalized01(float[] x, float[] y)
        {
            return (CosineSimilarity(x, y) + 1) / 2;
        }

        public static float CosineSimilarity(float[] x, float[] y)
        {
            double result = 0.0;

            for (int i = 0; i < x.Length; i++)
            {
                result += x[i] * y[i];
            }

            return Convert.ToSingle(result);
        }
    }
}
