using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Map
{
    public class AStarNode : IComparable<AStarNode>
    {
        public int Index;
        public float F;
        public float G;
        public float H;

        public int CompareTo(AStarNode other)
        {
            return (int)(F - other.F);
        }
    }
}
