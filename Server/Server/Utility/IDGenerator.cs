using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Utility
{
    public static class IDGenerator
    {
        private static int s_nextID = 0;

        public static int GetNextID()
        {
            return Interlocked.Increment(ref s_nextID);
        }
    }
}
