using System.Collections.Generic;

namespace Spool.Group
{
    public interface ITrainManager
    {

        /// <summary>Find trains
        /// </summary>
        List<Train> FindTrains();

        /// <summary>Whether the name is a Train's name
        /// </summary>
        bool IsTrainName(string name);

        /// <summary>Get train index from name
        /// </summary>
        int GetTrainIndex(string name);
    }
}
