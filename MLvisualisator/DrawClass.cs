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
using System.IO;
using Newtonsoft.Json;

namespace MLvisualisator
{
    public partial class MainWindow
    {
        private double FindHeight(List<Neuron> neuronsList)
        {
            int max = neuronsList[0].Count;
            for(int i = 1; i < neuronsList.Count; i++)
            {
                if (max < neuronsList[i].Count) max = neuronsList[i].Count;
            }

            SizeConfig sc = new SizeConfig();

            double res = sc.Height * max;
            return res;
        }

        private void addLine(string start, string end)
        {
            Ellipse first = new Ellipse();
            Ellipse second = new Ellipse();
            var allElments = TestAdd.Children.OfType<Ellipse>();
            foreach (Ellipse element in allElments)
            {
                if (element.Name == "N" + start) first = element;
                if (element.Name == "N" + end) second = element;
            }
            double x1 = Canvas.GetLeft(first);
            double y1 = Canvas.GetTop(first);
            double x2 = Canvas.GetLeft(second);
            double y2 = Canvas.GetTop(second);

            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(120, 255, 0, 0);

            Line line = new Line
            {
                X1 = x1 + (sc.NeuronRadius / 2),
                X2 = x2 + (sc.NeuronRadius / 2),
                Y1 = y1 + (sc.NeuronRadius / 2),
                Y2 = y2 + (sc.NeuronRadius / 2),
                Name = "W" + start + "_" + end,
                StrokeThickness = 4,
                Stroke = Brushes.Black,
            };

            TestAdd.Children.Add(line);
        }
        private void addNeuron(string id, int columCanvas, double rowCanvas)
        {
            Style style = (Style)FindResource("HoverNeuronColor");

            Ellipse ell = new Ellipse
            {
                Width = sc.NeuronRadius,
                Height = sc.NeuronRadius,
                Name = id,
            };

            Canvas.SetLeft(ell, columCanvas);
            Canvas.SetTop(ell, rowCanvas);

            Panel.SetZIndex(ell, 2);

            TestAdd.Children.Add(ell);
        }
    }
}
