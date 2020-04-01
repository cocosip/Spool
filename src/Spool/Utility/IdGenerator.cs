namespace Spool.Utility
{
    public class IdGenerator
    {
        private readonly SnowflakeDistributeId _snowflakeDistributeId;

        public IdGenerator()
        {
            _snowflakeDistributeId = new SnowflakeDistributeId(1L, 1L);
        }

        public long GenerateId()
        {
            return _snowflakeDistributeId.NextId();
        }

        public string GenerateIdAsString()
        {
            return _snowflakeDistributeId.NextId().ToString();
        }

    }
}
