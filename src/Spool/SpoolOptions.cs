namespace Spool
{
    public class SpoolOptions
    {
        public FilePoolConfigurations FilePools { get; }

        public SpoolOptions()
        {
            FilePools = new FilePoolConfigurations();
        }
    }
}
