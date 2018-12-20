﻿using System.Collections.Generic;

namespace ViretTool.BusinessLayer.RankingModels.Queries
{
    public class SemanticExampleQuery
    {
        public int[] PositiveExampleIds { get; private set; }
        public int[] NegativeExampleIds { get; private set; }
        public bool UseForSorting { get; private set; }
        public bool UseForFiltering { get; private set; }


        public SemanticExampleQuery(int[] positiveExampleIds, int[] negativeExampleIds, 
            bool useForSorting = true, bool useForFiltering = false)
        {
            PositiveExampleIds = positiveExampleIds;
            NegativeExampleIds = negativeExampleIds;
            UseForSorting = useForSorting;
            UseForFiltering = useForFiltering;
        }
        

        public override bool Equals(object obj)
        {
            return obj is SemanticExampleQuery query &&
                   EqualityComparer<int[]>.Default.Equals(PositiveExampleIds, query.PositiveExampleIds) &&
                   EqualityComparer<int[]>.Default.Equals(NegativeExampleIds, query.NegativeExampleIds);
        }

        public override int GetHashCode()
        {
            int hashCode = 952631468;
            hashCode = hashCode * -1521134295 + EqualityComparer<int[]>.Default.GetHashCode(PositiveExampleIds);
            hashCode = hashCode * -1521134295 + EqualityComparer<int[]>.Default.GetHashCode(NegativeExampleIds);
            return hashCode;
        }
    }
}
