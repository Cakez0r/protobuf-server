using System.Threading;

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
