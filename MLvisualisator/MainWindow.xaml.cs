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
    class MlData
    {
        public List<Neuron> NeuronsList { get; set; }
        public List<Link> Links { get; set; }
        public int ManyToMany { get; set; }
        public List<Input_Output> Inputs { get; set; }
        public List<Input_Output> Outputs { get; set; }
        public List<DrawList> DrawNeuron { get; set; }
    }
    class Neuron
    {
        public int Index { get; set; }
        public int Count { get; set; }
    }
    class Link
    {
        public string Index { get; set; }
        public double Value { get; set; }
        public double Error { get; set; } = 0.0;
        public List<CurrPath> Paths { get; set; }
    }
    class Input_Output
    {
        public string Name { get; set; }
        public double Value { get; set; }
    }
    class CurrPath
    {
        public string Path { get; set; }
        public double Weight { get; set; }
        public double Error { get; set; }

    }
    class DrawList
    {
        public Ellipse Ell { get; set; }
        public TextBlock EllTxt { get; set; }
        public List<Line> Paths { get; set; }
    }
    class DrawLines{
        public Line line { get; set; }
    }
    class SizeConfig
    {
        public int NeuronRadius { get; set; } = 100;
        public int Height { get; set; } = 175;
        public int Width { get; set; } = 175;
    }


    public partial class MainWindow : Window
    {
        public int step = 0;
        public int popupCount = 0;
        public string firstPopupName = "";

        public List<Popup> PopupList = new List<Popup>();
        public MainWindow()
        {
            InitializeComponent();
        }

        SizeConfig sc = new SizeConfig();
        MlData ml_data = new MlData();  // вообще все данные по нейронам
        private void GenerateFun(object sender, RoutedEventArgs e)
        {
            step = 0;
            TestAdd.Children.Clear();
            // draw neurons
            ml_data = getDataFromJson();
            ml_data.DrawNeuron = new List<DrawList>();

            addPopup();

            int colums = ml_data.NeuronsList.Count;
            int columCanvas = 0;
            double maxHeigh = FindHeight(ml_data.NeuronsList);
            int global_id_for_value = 0;

            for (int i = 0; i < colums; i++)
            {
                int rows = ml_data.NeuronsList[i].Count;
                double rowCanvas = (maxHeigh - rows * sc.Height) / 2.0;
                for (int j = 0; j < rows; j++)
                {
                    string ind = $"N{j}_{i}";
                    double neuron_value = ml_data.Links[global_id_for_value].Value;
                    addNeuron(ind, neuron_value, columCanvas, rowCanvas);
                    rowCanvas += sc.Height;
                    global_id_for_value++;
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

                for (int j = 0; j < link.Paths.Count; j++)
                {
                    if (link.Paths[j].Path == null) continue; // skip last neurons colum
                    addLine(name, link.Paths[j].Path);
                }
            }
        }
        private void ClearPopup(object sender, RoutedEventArgs e)
        {
            foreach (Popup popup in PopupList) popup.IsOpen = false;
        }

        private void Button_MouseEnter_1(object sender, MouseEventArgs e)
        {
            popup1.IsOpen = true;
        }

        private void NextFun(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                if (step % (ml_data.NeuronsList.Count * 2) < ml_data.NeuronsList.Count)
                {
                    Learn();
                }
                else
                {
                    if (step % (ml_data.NeuronsList.Count * 2) != ml_data.NeuronsList.Count * 2 - 1)
                    {
                        FindErrors();
                    }
                    else
                    {
                        SetNewData();
                    }
                }
                step++;
            }
        }
    }
}
