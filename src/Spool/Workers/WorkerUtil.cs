using System.IO;
using System.Text.RegularExpressions;

namespace Spool.Workers
{
    public static class WorkerUtil
    {
        public static string NormalizeName(int number)
        {
            return number.ToString().PadLeft(6, '0');
        }
 
        public static bool IsWorkerName(string name)
        {
            return Regex.IsMatch(name, "^\\d{6}$");
        }

        public static int ParseNumber(string name)
        {
            if (int.TryParse(name, out int number))
            {
                return number;
            }
            return 0;
        }

    }
}