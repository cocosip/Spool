using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spool.Workers
{
    public class FileWorker : IWorker
    {
        public string Name { get; set; }

        public int Number { get; set; }

    }
}