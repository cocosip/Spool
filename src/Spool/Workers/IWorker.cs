using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Spool.Workers
{
    public interface IWorker
    {
        bool IsSetup { get; }
        string Name { get; }
        int Number { get; }
        string Path { get; }
        bool IsLoaded { get; }
        WorkerState State { get; }
        void Setup();

        void ChangeState(WorkerState state);
        bool TryEntryWrite();
        bool IsPendingEmpty();
        int GetPendingCount();
        int GetProcessingCount();
        Task<SpoolFile> WriteAsync(Stream stream, string ext);
        Task<SpoolFile> WriteAsync(string fileName);
        void ReturnFiles(List<SpoolFile> spoolFiles);
        void ReleaseFiles(List<SpoolFile> spoolFiles);

    }
}