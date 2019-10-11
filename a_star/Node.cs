using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace a_star
{
    class Node
    {
        public Node(int x, int y, NodeType type)
        {
            this.X = x;
            this.Y = y;
            nodeType = type;
            gScore = 0;
            fScore = 0;
            Neighbors = new List<Node>();
        }

        public static Color startNodeColor = new Color() { R = 70, G = 200, B = 70 };
        public static Color endNodeColor = new Color() { R = 200, G = 70, B = 70 };
        public static Color regularNodeColor = new Color() { R = 40, G = 40, B = 40 };
        public static Color wallNodeColor = new Color() { R = 81, G = 115, B = 127 };
        public static Color previousPathColor = new Color() { R = 219, G = 171, B = 195 };

        public int X;
        public int Y;
        public List<Node> Neighbors;
        public Node CameForm;
        //The cost of going from start to this node
        public double gScore;
        //The cost of going from this node to end
        public double fScore;
        public NodeType nodeType;
        public Color Color
        {
            get
            {
                switch (nodeType)
                {
                    case NodeType.Start:
                        return startNodeColor;
                    case NodeType.End:
                        return endNodeColor;
                    case NodeType.Regular:
                        return regularNodeColor;
                    case NodeType.Wall:
                        return wallNodeColor;
                    case NodeType.Discovered:
                        return Colors.Salmon;
                    case NodeType.FinalPath:
                        return Colors.YellowGreen;
                    case NodeType.PreviousPath:
                        return previousPathColor;
                    default:
                        return Colors.Blue;
                }

            }
        }

    }
}
