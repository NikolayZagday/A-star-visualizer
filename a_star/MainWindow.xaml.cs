using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Diagnostics;
using System.Threading;

namespace a_star
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VisualAStar aStar;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                aStar = new VisualAStar((int)imageWrapper.ActualWidth, (int)imageWrapper.ActualHeight, imgControl, Dispatcher);
                aStar.SomethingWentWrong += (message) => { MessageBox.Show(message); };
                DataContext = aStar;
            };
        }

        private void PickStartNode_Click(object sender, RoutedEventArgs e)
        {
            aStar.State = State.StartNodePicking;
        }

        private void PickEndNode_Click(object sender, RoutedEventArgs e)
        {
            aStar.State = State.EndNodePicking;
        }

        private void DrawWall_Click(object sender, RoutedEventArgs e)
        {
            aStar.State = State.WallDrawing;
        }

        private void RemoveWall_Click(object sender, RoutedEventArgs e)
        {
            aStar.State = State.WallRemoving;
        }

        private async void SearchPath_Click(object sender, RoutedEventArgs e)
        {
            await aStar.SearchPathAsync();
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            aStar.Reset();
        }

        private void ImgControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //var el = ((FrameworkElement)sender);
            if (aStar != null)
                aStar.ChangeSize((int)e.NewSize.Width, (int)e.NewSize.Height);
        }
    }
}
