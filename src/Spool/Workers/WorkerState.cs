namespace Spool.Workers
{
    public enum WorkerState
    {
        Pending = 1,
        Read = 2,
        Write = 4,
        ReadWrite = 8,
        Complete = 16
    }
}