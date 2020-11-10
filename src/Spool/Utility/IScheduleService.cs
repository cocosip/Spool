using System;

namespace Spool.Utility
{
    /// <summary>
    /// Schedule service
    /// </summary>
    public interface IScheduleService
    {

        /// <summary>
        /// Start a schedule task
        /// </summary>
        /// <param name="name">Task name</param>
        /// <param name="action">Task action</param>
        /// <param name="dueTime">Duetime</param>
        /// <param name="period">Period</param>
        void StartTask(string name, Action action, int dueTime, int period);

        /// <summary>
        /// Stop a schedule task by name
        /// </summary>
        /// <param name="name">Task name</param>
        void StopTask(string name);
    }
}
