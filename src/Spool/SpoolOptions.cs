namespace Spool
{
    /// <summary>
    /// SpoolOptions
    /// </summary>
    public class SpoolOptions
    {
        /// <summary>
        /// All file pool configurations
        /// </summary>
        public FilePoolConfigurations FilePools { get; }

        /// <summary>
        /// ctor
        /// </summary>
        public SpoolOptions()
        {
            FilePools = new FilePoolConfigurations();
        }
    }
}
