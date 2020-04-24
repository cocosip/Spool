namespace Spool.Utility
{
    /// <summary>Id生成器
    /// </summary>
    public class IdGenerator
    {
        private readonly SnowflakeDistributeId _snowflakeDistributeId;

        /// <summary>Ctor
        /// </summary>
        public IdGenerator()
        {
            _snowflakeDistributeId = new SnowflakeDistributeId(1L, 1L);
        }

        /// <summary>生成Id
        /// </summary>
        public long GenerateId()
        {
            return _snowflakeDistributeId.NextId();
        }

        /// <summary>生成Id
        /// </summary>
        public string GenerateIdAsString()
        {
            return _snowflakeDistributeId.NextId().ToString();
        }

    }
}
