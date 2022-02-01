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
    class MlData
    {
        public List<Neuron> NeuronsList { get; set; }
        public List<Link> Links { get; set; }
        public int ManyToMany { get; set; }
    }
    class Neuron
    {
        public int Index { get; set; }
        public int Count { get; set; }
    }
    class Link
    {
        public string Index { get; set; }
        public string Lines { get; set; }
        public string Weights { get; set; }
    }
    class SizeConfig
    {
        public int NeuronRadius { get; set; } = 100;
        public int Height { get; set; } = 175;
        public int Width { get; set; } = 175;
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        SizeConfig sc = new SizeConfig();
        private List<string> find_links(string data)
        {
            List<string> res = new List<string>();
            string item = "";

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != '|') item += data[i];
                else if (data[i] == 'e') break;
                else
                {
                    res.Add(item);
                    item = "";
                }
            }
            return res;
        }
        private void GenerateFun(object sender, RoutedEventArgs e)
        {
            TestAdd.Children.Clear();
            // draw neurons
            MlData ml_data = getDataFromJson();
            int colums = ml_data.NeuronsList.Count;
            int columCanvas = 0;
            double maxHeigh = FindHeight(ml_data.NeuronsList);

            for (int i = 0; i < colums; i++)
            {
                int rows = ml_data.NeuronsList[i].Count;
                double rowCanvas = (maxHeigh - rows * sc.Height) / 2.0;
                for (int j = 0; j < rows; j++)
                {
                    string ind = $"N{j}{i}";
                    addNeuron(ind, columCanvas, rowCanvas);
                    rowCanvas += sc.Height;
                }
                TestAdd.Height = rowCanvas;
                columCanvas += sc.Width;
            }
            TestAdd.Width = columCanvas;
                

            //draw links
            for (int k = 0; k < ml_data.Links.Count; k++)
            {
                Link link = ml_data.Links[k];
                string name = link.Index;
                List<string> neu_to_neu = find_links(link.Lines);

                for (int j = 0; j < neu_to_neu.Count; j++)
                {
                    addLine(name, neu_to_neu[j]);
                }
            }

            foreach (UIElement item in TestAdd.Children)
            {
                if (item is Ellipse)
                {
                    Panel.SetZIndex(item, 1);
                }
                else
                {
                    Panel.SetZIndex(item, 0);
                }
            }
        }
    }
}
