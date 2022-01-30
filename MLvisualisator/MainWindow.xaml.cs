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

namespace MLvisualisator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private void addNeyron(string id, int columCanvas, int rowCanvas)
        {
            //TextBlock testTxtBlock = new TextBlock();
            //testTxtBlock.Text = id;
            //Canvas.SetLeft(testTxtBlock, columCanvas);
            //Canvas.SetTop(testTxtBlock, rowCanvas);
            Ellipse ell = new Ellipse();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(28, 28, 28, 0);
            ell.Width = 30;
            ell.Height = 30;
            ell.Fill = mySolidColorBrush;
            Canvas.SetLeft(ell, columCanvas);
            Canvas.SetTop(ell, rowCanvas);
            TestAdd.Children.Add(ell);
        }
        private void testAdd(int rows, int id, int columCanvas)
        {
            int rowCanvas = 0;
            for (int i = 0; i < rows; i++)
            {
                string ind = $"  {i},{id}  ";
                addNeyron(ind, columCanvas, rowCanvas);
                rowCanvas += 50;
            }
            TestAdd.Height = rowCanvas;
        }
        public MainWindow()
        {
            InitializeComponent();
        }
        private void GenerateFun(object sender, RoutedEventArgs e)
        {
            TestAdd.Children.Clear();
            int colums = int.Parse(CountOfColum.Text);
            int rows = int.Parse(CCountOfNeyrons.Text);
            int columCanvas = 0;
            for (int i = 0; i < colums; i++)
            {
                testAdd(rows, i, columCanvas);
                columCanvas += 60;
            }
            TestAdd.Width = columCanvas;
        }
    }
}
