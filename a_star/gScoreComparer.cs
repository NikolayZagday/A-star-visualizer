﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a_star
{
    class gScoreComparer : IComparer<Node>
    {
        public int Compare(Node x, Node y)
        {
            if (x == null || y == null)
            {
                return 0;
            }

            return x.gScore.CompareTo(y.gScore);
        }
    }
}
