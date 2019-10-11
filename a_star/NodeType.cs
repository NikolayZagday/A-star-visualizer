using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a_star
{
    enum NodeType
    {
        Start,
        End,
        Regular,
        Wall,
        Discovered,
        FinalPath,
        PreviousPath
    }
}
