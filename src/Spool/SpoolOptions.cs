namespace Spool
{
    public class SpoolOptions
    {
        public FilePoolConfigurations FilePools { get; set; }

        public SpoolOptions()
        {
            FilePools = new FilePoolConfigurations();
        }
    }
}