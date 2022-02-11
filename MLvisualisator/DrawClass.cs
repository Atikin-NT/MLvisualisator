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
using System.Windows.Controls.Primitives;

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

        private void addPopup()
        {
            for (int i = 0; i < 5; i++)
            {
                Popup popup = new Popup
                {
                    Name = "popup" + i.ToString(),
                    StaysOpen = false,
                    Placement = PlacementMode.Mouse,
                    MaxWidth = 180
                };
                StackPanel sp = new StackPanel
                {
                    Orientation = Orientation.Vertical
                };
                TextBlock txtBlock = new TextBlock
                {
                    Name = "DataHeader" + "!",
                    Background = Brushes.LightPink,
                    Opacity = 0.8,
                    Foreground = Brushes.Black,
                    FontSize = 16,
                    Text = "Данные нейрона 0_0"
                };
                TextBlock txtBlockData = new TextBlock
                {
                    Name = "DataNeuron" + "!",
                    Background = Brushes.LightPink,
                    Opacity = 0.8,
                    Foreground = Brushes.Black,
                    FontSize = 14,
                    Text = "Данные нейрона 0_0"
                };
                sp.Children.Add(txtBlock);
                sp.Children.Add(txtBlockData);
                popup.Child = sp;

                TestAdd.Children.Add(popup);
                PopupList[i] = popup;
            }
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

            for(int i = 0; i < ml_data.DrawNeuron.Count; i++)
            {
                Ellipse ell = ml_data.DrawNeuron[i].Ell;
                if (ell.Name == first.Name)
                {
                    ml_data.DrawNeuron[i].Paths.Add(line);
                    break;
                }
            }
        }
        private void addNeuron(string id, double value, int columCanvas, double rowCanvas)
        {
            Ellipse ell = new Ellipse
            {
                Width = sc.NeuronRadius,
                Height = sc.NeuronRadius,
                Name = id,
            };
            ell.MouseUp += NeuronClick;
            Canvas.SetLeft(ell, columCanvas);
            Canvas.SetTop(ell, rowCanvas);

            Panel.SetZIndex(ell, 2);

            // draw text in ellipse

            TextBlock txt = new TextBlock
            {
                Name = id + "_txt",
                Text = value.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = Brushes.Black,
            };

            Grid gr = new Grid
            {
                Width = sc.NeuronRadius,
                Height = sc.NeuronRadius,
                Name = id + "_gr",
            };

            Canvas.SetLeft(gr, columCanvas);
            Canvas.SetTop(gr, rowCanvas);

            Panel.SetZIndex(gr, 3);

            gr.Children.Add(txt);

            TestAdd.Children.Add(ell);
            TestAdd.Children.Add(gr);

            DrawList dl = new DrawList
            {
                Ell = ell,
                EllTxt = txt,
                Paths = new List<Line>()
            };
            if (dl != null) ml_data.DrawNeuron.Add(dl);
        }
        private void NeuronChangecolor(Ellipse ell, int color)
        {
            if (color == 0) ell.Fill = Brushes.Green;
            else ell.Fill = Brushes.Red;
        }
        private void LineChangeColor(Line line, double op)
        {
            SolidColorBrush color = new SolidColorBrush(Color.FromRgb(37, 105, 173));
            color.Opacity = op;
            line.Stroke = color;
        }
    }
}
