using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Utility
{
    public interface IPositionable
    {
        Vector2 Position { get; }
    }
}
