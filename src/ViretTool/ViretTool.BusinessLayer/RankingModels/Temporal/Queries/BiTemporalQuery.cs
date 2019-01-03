namespace ViretTool.BusinessLayer.RankingModels.Temporal.Queries
{
    public class BiTemporalQuery<TQuery>
    {
        public TQuery PrimaryQuery { get; private set; }
        public TQuery SecondaryQuery { get; private set; }


        public BiTemporalQuery(TQuery primaryQuery, TQuery secondaryQuery)
        {
            PrimaryQuery = primaryQuery;
            SecondaryQuery = secondaryQuery;
        }

        public override bool Equals(object obj)
        {
            return obj is BiTemporalQuery<TQuery> query &&
                   PrimaryQuery.Equals(query.PrimaryQuery) &&
                   SecondaryQuery.Equals(query.SecondaryQuery);
        }

        public override int GetHashCode()
        {
            int hashCode = -1702951762;
            hashCode = hashCode * -1521134295 + PrimaryQuery.GetHashCode();
            hashCode = hashCode * -1521134295 + SecondaryQuery.GetHashCode();
            return hashCode;
        }
    }
}
