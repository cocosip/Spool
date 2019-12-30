namespace Spool.Writer
{
    public interface IFileWriterManager
    {
        void Initialize();

        FileWriterOption GetFileWriterOption();

        /// <summary>Get a file writer
        /// </summary>
        FileWriter Get();

        /// <summary>Return a file writer
        /// </summary>
        void Return(FileWriter fileWriter);
    }
}
