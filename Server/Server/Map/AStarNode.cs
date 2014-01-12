using System;
using System.Runtime.CompilerServices;

namespace Server.Map
{
    public class AStarNode : IComparable<AStarNode>
    {
        public int Index;
        public float F;
        public float G;
        public float H;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(AStarNode other)
        {
            return (int)(F - other.F);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return Index == obj.GetHashCode();
        }
    }
}
