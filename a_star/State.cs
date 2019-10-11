using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a_star
{
    enum State
    {
        StartNodePicking,
        EndNodePicking,
        WallDrawing,
        WallRemoving,
        PathSearching,
        Rendering,
    }
}
