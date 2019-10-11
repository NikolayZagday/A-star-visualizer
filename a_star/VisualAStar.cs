using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;

namespace a_star
{
    class VisualAStar : INotifyPropertyChanged
    {
        #region Properties and stuff

        public Dispatcher dispatcher;
        private Task pathSearchingTask;
        private CancellationTokenSource cancellationTokenSource;
        private WriteableBitmap writeableBitmap;
        private int renderWidth;
        private int renderHeight;
        private int bitsPerPixel;
        private int pBackBuffer;
        private int backBufferStride;

        public delegate void Warning(string warningMessage);
        public event Warning SomethingWentWrong;

        private bool m_DiagonalSearch = false;
        public bool DiagonalSearch
        {
            get 
            { 
                return m_DiagonalSearch; 
            }
            set 
            { 
                if (State != State.PathSearching)
                {
                    m_DiagonalSearch = value;
                    Reset();
                    OnPropertyChanged();
                }
            }
        }

        Node[,] Nodes;

        private Heuristics m_Heuristics;
        public Heuristics SearchHeuristics
        {
            get
            {
                return m_Heuristics;
            }
            set
            {
                m_Heuristics = value;
                OnPropertyChanged();
            }
        }

        private Node m_StartNode;
        public Node StartNode
        {
            get
            {
                return m_StartNode;
            }
            set
            {
                m_StartNode = value;
                OnPropertyChanged();
                OnPropertyChanged("StartNodeX");
                OnPropertyChanged("StartNodeY");
            }
        }

        public int StartNodeX
        {
            get
            {
                if (StartNode != null)
                    return StartNode.X;

                return -1;
            }
        }

        public int StartNodeY
        {
            get
            {
                if (StartNode != null)
                    return StartNode.Y;

                return -1;
            }
        }

        private Node m_GoalNode;
        public Node GoalNode
        {
            get
            {
                return m_GoalNode;
            }
            set
            {
                m_GoalNode = value;
                OnPropertyChanged();
                OnPropertyChanged("GoalNodeX");
                OnPropertyChanged("GoalNodeY");
            }
        }

        private bool m_ShowPreviousPath = true;
        public bool ShowPreviousPath
        {
            get
            {
                return m_ShowPreviousPath;
            }
            set
            {
                m_ShowPreviousPath = value;
                OnPropertyChanged();
            }
        }

        public int GoalNodeX
        {
            get
            {
                if (GoalNode != null)
                    return GoalNode.X;

                return -1;
            }
        }

        public int GoalNodeY
        {
            get
            {
                if (GoalNode != null)
                    return GoalNode.Y;

                return -1;
            }
        }

        private bool m_delay = true;
        public bool Delay
        {
            get
            {
                return m_delay;
            }
            set
            {
                m_delay = value;
                OnPropertyChanged();
            }
        }

        private int m_MainLoops;
        public int MainLoops
        {
            get
            {
                return m_MainLoops;
            }
            set
            {
                m_MainLoops = value;
                OnPropertyChanged();
            }
        }

        private int m_NodesScaned;
        public int NodesScaned
        {
            get
            {
                return m_NodesScaned;
            }
            set
            {
                m_NodesScaned = value;
                OnPropertyChanged();
            }
        }

        private int xNodeSize = 10;
        private int yNodeSize = 10;

        private int m_XaxisNodeNumber = 50;
        public int XaxisNodeNumber
        {
            get
            {
                return m_XaxisNodeNumber;
            }
            set
            {
                m_XaxisNodeNumber = value;
                OnPropertyChanged();
            }
        }

        private int m_YaxisNodeNumber = 50;
        public int YaxisNodeNumber
        {
            get
            {
                return m_YaxisNodeNumber;
            }
            set
            {
                m_YaxisNodeNumber = value;
                OnPropertyChanged();
            }
        }

        private Color gridColor = new Color() { R = 100, G = 120, B = 100 };
        private Color neighborNodeColor = new Color() { R = 50, G = 50, B = 50 };


        private Image Img { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private State m_state;
        public State State
        {
            get { return m_state; }
            set
            {
                m_state = value;
                OnPropertyChanged();

                StateChanged();
            }
        }

        #endregion

        #region Constructor

        public VisualAStar(int width, int height, Image drawingContext, Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            Img = drawingContext;

            State = State.StartNodePicking;

            renderWidth = width;
            renderHeight = height;

            writeableBitmap = new WriteableBitmap(
                   renderWidth,
                   renderHeight,
                   96,
                   96,
                   PixelFormats.Bgr32,
                   null);
            bitsPerPixel = writeableBitmap.Format.BitsPerPixel;
            pBackBuffer = (int)writeableBitmap.BackBuffer;
            backBufferStride = writeableBitmap.BackBufferStride;

            InitNodeArray();
            Render();

            Img.Source = writeableBitmap;
        }

        #endregion

        public void ChangeSize(int width, int height)
        {
            renderWidth = width;
            renderHeight = height;

            writeableBitmap = new WriteableBitmap(
                   renderWidth,
                   renderHeight,
                   96,
                   96,
                   PixelFormats.Bgr32,
                   null);
            bitsPerPixel = writeableBitmap.Format.BitsPerPixel;
            pBackBuffer = (int)writeableBitmap.BackBuffer;
            backBufferStride = writeableBitmap.BackBufferStride;

            Render();
            
            Img.Source = writeableBitmap;
        }

        public void Reset()
        {
            StartNode = null;
            GoalNode = null;
            MainLoops = 0;
            NodesScaned = 0;
            InitNodeArray();
            Render();
        }

        #region Image Events Handling 

        private void StateChanged()
        {
            if (State == State.StartNodePicking)
            {
                Img.MouseMove -= Img_RemoveWall;
                Img.MouseMove -= Img_DrawWall;
                Img.MouseLeftButtonDown += Img_MouseLeftButtonDown;
            }
            else if (State == State.EndNodePicking)
            {
                Img.MouseMove -= Img_RemoveWall;
                Img.MouseMove -= Img_DrawWall;
                Img.MouseLeftButtonDown += Img_MouseLeftButtonDown;
            }
            else if (State == State.WallDrawing)
            {
                Img.MouseMove -= Img_RemoveWall;
                Img.MouseLeftButtonDown -= Img_MouseLeftButtonDown;
                Img.MouseMove += Img_DrawWall;
            }
            else if (State == State.WallRemoving)
            {
                Img.MouseLeftButtonDown -= Img_MouseLeftButtonDown;
                Img.MouseMove -= Img_DrawWall;
                Img.MouseMove += Img_RemoveWall;
            }
            else if (State == State.PathSearching)
            {
                Img.MouseMove -= Img_RemoveWall;
                Img.MouseMove -= Img_DrawWall;
                Img.MouseLeftButtonDown -= Img_MouseLeftButtonDown;
            }
        }

        private void Img_RemoveWall(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = NormalizePoint(e.GetPosition(Img));
                int x = (int)pos.X;
                int y = (int)pos.Y;

                var pickedNode = Nodes[x, y];
                if (pickedNode.nodeType == NodeType.Wall)
                {
                    pickedNode.nodeType = NodeType.Regular;
                    DrawNode(pickedNode);
                }
            }
        }

        private void Img_DrawWall(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var pos = NormalizePoint(e.GetPosition(Img));
                int x = (int)pos.X, y = (int)pos.Y;

                Node n = Nodes[x, y];
                if (n.nodeType != NodeType.Wall ||
                    n.nodeType != NodeType.Start ||
                    n.nodeType != NodeType.End)
                {
                    n.nodeType = NodeType.Wall;
                    DrawNode(n);
                }
            }
        }

        private void Img_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = NormalizePoint(e.GetPosition(Img));
            int x = (int)pos.X, y = (int)pos.Y;

            if (x < Nodes.GetLength(0) && y < Nodes.GetLength(1))
            {
                if (State == State.StartNodePicking)
                {
                    if (StartNode != null)
                    {
                        StartNode.nodeType = NodeType.Regular;
                        DrawNode(StartNode);
                    }

                    Nodes[x, y].nodeType = NodeType.Start;
                    StartNode = Nodes[x, y];
                    DrawNode(StartNode);
                }
                else if (State == State.EndNodePicking)
                {
                    if (GoalNode != null)
                    {
                        GoalNode.nodeType = NodeType.Regular;
                        DrawNode(GoalNode);
                    }

                    Nodes[x, y].nodeType = NodeType.End;
                    GoalNode = Nodes[x, y];
                    DrawNode(GoalNode);
                }
            }
        }

        #endregion

        #region Bitmap Drawing Methods

        private void Render()
        {
            xNodeSize = (int)Math.Ceiling(renderWidth / (double)XaxisNodeNumber);
            yNodeSize = (int)Math.Ceiling(renderHeight / (double)YaxisNodeNumber);
            //xNodeSize = renderWidth / XaxisNodeNumber;
            //yNodeSize = renderHeight / YaxisNodeNumber;

            int backBuffer = pBackBuffer;

            unsafe
            {
                for (int i = 0; i < Nodes.GetLength(0); i++)
                {
                    for (int j = 0; j < Nodes.GetLength(1); j++)
                    {
                        var node = Nodes[i, j];

                        int color_data = node.Color.R << 16;
                            color_data |= node.Color.G << 8;
                            color_data |= node.Color.B << 0;

                        int x = node.X * xNodeSize;
                        int y = node.Y * yNodeSize;
                        int x2 = x + xNodeSize;
                        int y2 = y + yNodeSize;

                        for (int row = x; row < x2 && row < renderWidth; row++)
                        {
                            for (int col = y; col < y2 && col < renderHeight; col++)
                            {
                                backBuffer = pBackBuffer;
                                backBuffer += col * backBufferStride;
                                backBuffer += row * 4;
                                *((int*)backBuffer) = color_data;
                            }
                        }
                    }
                }
            }

            dispatcher.BeginInvoke(new Action(() =>
            {
                writeableBitmap.Lock();
                writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, renderWidth, renderHeight));
                writeableBitmap.Unlock();
            }));
        }

        private void DrawNode(Node node)
        {
            //xNodeSize = renderWidth / XaxisNodeNumber;
            //yNodeSize = renderHeight / YaxisNodeNumber;
            xNodeSize = (int)Math.Ceiling(renderWidth / (double)XaxisNodeNumber);
            yNodeSize = (int)Math.Ceiling(renderHeight / (double)YaxisNodeNumber);

            int x = node.X * xNodeSize;
            int y = node.Y * yNodeSize;
            int x2 = x + xNodeSize;
            int y2 = y + yNodeSize;

            unsafe
            {
                int pBackBuffer = (int)writeableBitmap.BackBuffer;

                int color_data = node.Color.R << 16;
                color_data |= node.Color.G << 8;
                color_data |= node.Color.B << 0;

                for (int row = x; row < x2; row++)
                {
                    for (int col = y; col < y2; col++)
                    {
                        pBackBuffer = (int)writeableBitmap.BackBuffer;
                        pBackBuffer += col * writeableBitmap.BackBufferStride;
                        pBackBuffer += row * 4;
                        *((int*)pBackBuffer) = color_data;
                    }
                }
            }

            writeableBitmap.Lock();
            int rectWidth = x + xNodeSize > renderWidth ? (renderWidth - x) : xNodeSize;
            int rectHeight = y + yNodeSize > renderHeight ? (renderHeight - y) : yNodeSize;
            writeableBitmap.AddDirtyRect(new Int32Rect(x, y, rectWidth, rectHeight));
            writeableBitmap.Unlock();
        }

        private void DrawGrid()
        {
            writeableBitmap.Lock();

            unsafe
            {
                int pBackBuffer = (int)writeableBitmap.BackBuffer;

                int color_data = gridColor.R << 16;
                color_data |= gridColor.G << 8;
                color_data |= gridColor.B << 0;

                for (int i = xNodeSize; i < renderWidth; i += xNodeSize)
                {
                    for (int j = 1; j < renderHeight; j++)
                    {
                        pBackBuffer = (int)writeableBitmap.BackBuffer;
                        pBackBuffer += j * writeableBitmap.BackBufferStride;
                        pBackBuffer += i * 4;
                        *((int*)pBackBuffer) = color_data;
                    }
                }

                for (int i = yNodeSize; i < renderHeight; i += yNodeSize)
                {
                    for (int j = 1; j < renderWidth; j++)
                    {
                        pBackBuffer = (int)writeableBitmap.BackBuffer;
                        pBackBuffer += i * writeableBitmap.BackBufferStride;
                        pBackBuffer += j * 4;
                        *((int*)pBackBuffer) = color_data;
                    }
                }
            }

            // Specify the area of the bitmap that changed.
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, renderWidth, renderHeight));

            writeableBitmap.Unlock();
        }

        #endregion

        private void Reconstruct_path(Node node)
        {
            while(node.CameForm != null)
            {
                node = node.CameForm;
                node.nodeType = NodeType.FinalPath;
            }
        }

        public async Task SearchPathAsync()
        {
            if (StartNode == null && GoalNode == null)
            {
                SomethingWentWrong?.Invoke("Pick starting node and goal node.");
                return;
            }

            if (StartNode == null)
            {
                SomethingWentWrong?.Invoke("Pick a starting node.");
                return;
            }

            if (GoalNode == null)
            {
                SomethingWentWrong?.Invoke("Pick a goal node.");
                return;
            }

            if (pathSearchingTask != null)
            {
                if (pathSearchingTask.Status != TaskStatus.RanToCompletion)
                    cancellationTokenSource.Cancel();
            }

            cancellationTokenSource = new CancellationTokenSource();

            State = State.PathSearching;
            ClearDiscoveredNodes();


            pathSearchingTask = Task.Run(() =>
            {
                try
                {
                    A_star(StartNode, GoalNode, cancellationTokenSource);
                }
                catch (OperationCanceledException)
                {
                    
                }
            }, cancellationTokenSource.Token);

            await pathSearchingTask;

            State = State.StartNodePicking;
        }

        private void A_star(Node start, Node end, CancellationTokenSource ctSource)
        {
            MainLoops = 0;
            NodesScaned = 0;

            var closedList = new List<Node>();

            var openList = new List<Node>() { start };

            while (openList.Count != 0)
            {
                ctSource.Token.ThrowIfCancellationRequested();

                openList.Sort(new fScoreComparer());
                Node current = openList.First();

                if (current == end)
                {
                    Node n = current;
                    while (n.CameForm != null)
                    {
                        n = n.CameForm;
                        n.nodeType = NodeType.FinalPath;

                        if (m_delay)
                        {
                            Thread.Sleep(16);
                            Render();
                        }
                    }

                    if (!m_delay)
                        Render();

                    break;
                }

                openList.Remove(current);
                closedList.Add(current);

                foreach (var neighbor in current.Neighbors)
                {
                    NodesScaned++;
                    if (neighbor.nodeType == NodeType.Wall)
                        continue;

                    if (closedList.Contains(neighbor))
                        continue;

                    var tentative_gScore = current.gScore + GetDistanceBetween(current, neighbor);

                    if (!openList.Contains(neighbor))
                    {
                        if (neighbor.nodeType != NodeType.End && neighbor.nodeType != NodeType.PreviousPath)
                            neighbor.nodeType = NodeType.Discovered;

                        openList.Add(neighbor);
                    }
                    else if (tentative_gScore >= neighbor.gScore)
                        continue;

                    neighbor.CameForm = current;
                    neighbor.gScore = tentative_gScore;
                    double hueristicValue = 0;

                    switch (SearchHeuristics)
                    {
                        case Heuristics.Manhattan:
                            hueristicValue = ManhattanHeuristic(neighbor, end);
                            break;
                        case Heuristics.Diagonal:
                            hueristicValue = DiagonalHeuristic(neighbor, end);
                            break;
                        case Heuristics.Euclidean:
                            hueristicValue = EuclideanHueristic(neighbor, end);
                            break;
                        case Heuristics.None:
                            break;
                        default:
                            break;
                    }

                    neighbor.fScore = neighbor.gScore + hueristicValue;
                }

                MainLoops++;

                if (m_delay)
                {
                    Thread.Sleep(16);
                    Render();
                }
            }
            Render();
        }

        private double ManhattanHeuristic(Node node, Node goal)
        {
            double dx = Math.Abs(node.X - goal.X);
            double dy = Math.Abs(node.Y - goal.Y);
            double D = 1;

            return D * (dx + dy);
        }

        private double DiagonalHeuristic(Node node, Node goal)
        {
            double dx = Math.Abs(node.X - goal.X);
            double dy = Math.Abs(node.Y - goal.Y);
            double D = 1; // The cost of a horizontal or vertical movement
            double D2 = 1; // The cost of a diagonal movement

            return D * (dx + dy) + (D2 - 2 * D) * Math.Min(dx, dy);
        }

        private double EuclideanHueristic(Node node, Node goal)
        {
            double dx = Math.Abs(node.X - goal.X);
            double dy = Math.Abs(node.Y - goal.Y);
            double D = 1;
            return D * Math.Sqrt(dx * dx + dy * dy);
        }

        private int GetDistanceBetween(Node current, Node neighbor)
        {
            return 1;
        }

        #region Helpers

        private Point NormalizePoint(Point p)
        {
            int unNormX = (int)Math.Ceiling(p.X / xNodeSize);
            int unNormY = (int)Math.Ceiling(p.Y / yNodeSize);

            int x = unNormX == 0 ? unNormX : unNormX - 1;
            int y = unNormY == 0 ? unNormY : unNormY - 1;

            return new Point(x, y);
        }

        private void InitNodeArray()
        {
            Nodes = new Node[XaxisNodeNumber, YaxisNodeNumber];
            xNodeSize = renderWidth / XaxisNodeNumber;
            yNodeSize = renderHeight / YaxisNodeNumber;

            for (int i = 0; i < Nodes.GetLength(0); i++)
                for (int j = 0; j < Nodes.GetLength(1); j++)
                    Nodes[i, j] = new Node(i, j, NodeType.Regular);
            

            // all possible node neighbourhood cases
            //    +---+---+---+---+
            //    | 1 | 2 | 2 | 3 |
            //    +---+---+---+---+
            //    | 4 | 5 | 5 | 6 |
            //    +---+---+---+---+
            //    | 4 | 5 | 5 | 6 |
            //    +---+---+---+---+
            //    | 7 | 8 | 8 | 9 |
            //    +---+---+---+---+

            var xLen = Nodes.GetLength(0);
            var yLen = Nodes.GetLength(1);

            for (int x = 0; x < xLen; x++)
            {
                for (int y = 0; y < yLen; y++)
                {
                    var node = Nodes[x, y];

                    //case 1
                    if (x == 0 && y == 0)
                    {
                        var n = new List<Node>();
                        n.Add(Nodes[x + 1, y]);
                        n.Add(Nodes[x, y + 1]);

                        if (DiagonalSearch)
                            n.Add(Nodes[x + 1, y + 1]);
                        
                        node.Neighbors = n;
                    }
                    //case 2
                    else if (x != 0 && x < xLen - 1 && y == 0)
                    {
                        var n = new List<Node>();
                        n.Add(Nodes[x, y + 1]);
                        n.Add(Nodes[x + 1, y]);
                        n.Add(Nodes[x - 1, y]);

                        if (DiagonalSearch)
                        {
                            n.Add(Nodes[x - 1, y + 1]);
                            n.Add(Nodes[x + 1, y + 1]);
                        }

                        node.Neighbors = n;
                    }
                    //case 3
                    else if (x == (xLen - 1) && y == 0)
                    {
                        var n = new List<Node>();
                        n.Add(Nodes[x - 1, y]);
                        n.Add(Nodes[x, y + 1]);

                        if (DiagonalSearch)
                            n.Add(Nodes[x - 1, y + 1]);

                        node.Neighbors = n;
                    }
                    //case 4
                    else if (x == 0 && y != 0 && y < yLen - 1)
                    {
                        var n = new List<Node>();
                        n.Add(Nodes[x + 1, y]);
                        n.Add(Nodes[x, y + 1]);
                        n.Add(Nodes[x, y - 1]);

                        if (DiagonalSearch)
                        {
                            n.Add(Nodes[x + 1, y + 1]);
                            n.Add(Nodes[x + 1, y - 1]);
                        }

                        node.Neighbors = n;
                    }
                    //case 5
                    else if (x != 0 && x < xLen - 1 && y != 0 && y < yLen - 1)
                    {
                        var n = new List<Node>();
                        n.Add(Nodes[x + 1, y]);
                        n.Add(Nodes[x, y - 1]);
                        n.Add(Nodes[x - 1, y]);
                        n.Add(Nodes[x, y + 1]);

                        if (DiagonalSearch)
                        {
                            n.Add(Nodes[x + 1, y + 1]);
                            n.Add(Nodes[x + 1, y - 1]);
                            n.Add(Nodes[x - 1, y - 1]);
                            n.Add(Nodes[x - 1, y + 1]);
                        }

                        node.Neighbors = n;
                    }
                    //case 6
                    else if (x == (xLen - 1) && y != 0 && y < (yLen - 1))
                    {
                        var n = new List<Node>();
                        n.Add(Nodes[x - 1, y]);
                        n.Add(Nodes[x, y + 1]);
                        n.Add(Nodes[x, y - 1]);

                        if (DiagonalSearch)
                        {
                            n.Add(Nodes[x - 1, y + 1]);
                            n.Add(Nodes[x - 1, y - 1]);
                        }

                        node.Neighbors = n;
                    }
                    //case 7
                    else if (x == 0 && y == (yLen - 1))
                    {
                        var n = new List<Node>();

                        n.Add(Nodes[x + 1, y]);
                        n.Add(Nodes[x, y - 1]);

                        if (DiagonalSearch)
                            n.Add(Nodes[x + 1, y - 1]);
                       
                        node.Neighbors = n;
                    }
                    //case 8
                    else if (y == (yLen - 1) && x != 0 && x < (xLen - 1))
                    {
                        var n = new List<Node>();

                        n.Add(Nodes[x, y - 1]);
                        n.Add(Nodes[x + 1, y]);
                        n.Add(Nodes[x - 1, y]);
                        if (DiagonalSearch)
                        {
                            n.Add(Nodes[x + 1, y - 1]);
                            n.Add(Nodes[x - 1, y - 1]);
                        }
                        
                        node.Neighbors = n;
                    }
                    //case 9
                    else if (x == (xLen - 1) && y == (yLen - 1))
                    {
                        var n = new List<Node>();
                        n.Add(Nodes[x, y - 1]);
                        n.Add(Nodes[x - 1, y]);

                        if (DiagonalSearch)
                            n.Add(Nodes[x - 1, y - 1]);

                        node.Neighbors = n;
                    }
                }
            }
        }

        private void ClearDiscoveredNodes()
        {
            for (int y = 0; y < Nodes.GetLength(1); y++)
            {
                for (int x = 0; x < Nodes.GetLength(0); x++)
                {
                    var n = Nodes[x, y];
                    if (n.nodeType == NodeType.Discovered || n.nodeType == NodeType.PreviousPath)
                    {
                        n.nodeType = NodeType.Regular;
                    }
                    else if (n.nodeType == NodeType.FinalPath)
                    {
                        if (ShowPreviousPath)
                        {
                            n.nodeType = NodeType.PreviousPath;
                        }
                        else
                        {
                            n.nodeType = NodeType.Regular;
                        }
                    }
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string name = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
