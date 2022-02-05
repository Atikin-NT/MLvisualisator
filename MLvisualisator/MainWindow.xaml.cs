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
        public List<Input_Output> Inputs { get; set; }
        public List<Input_Output> Outputs { get; set; }
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
        public List<CurrPath> Paths { get; set; }
    }
    class Input_Output
    {
        public string Name { get; set; }
        public int Value { get; set; }
    }
    class CurrPath
    {
        public string Path { get; set; }
        public double Weight { get; set; }

    }
    class SizeConfig
    {
        public int NeuronRadius { get; set; } = 100;
        public int Height { get; set; } = 175;
        public int Width { get; set; } = 175;
    }

    class Matrix
    {
        public List<double> OutputList { get; set; }
        public List<double> InputList { get; set; }
        public List<List<double>> Weights { get; set; }
        public List<List<double>> Current { get; set; }

    }

    public partial class MainWindow : Window
    {
        public int step = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        SizeConfig sc = new SizeConfig();
        MlData ml_data = new MlData();
        Matrix matrixConfig = new Matrix();
        private void GenerateFun(object sender, RoutedEventArgs e)
        {
            step = 0;
            TestAdd.Children.Clear();
            // draw neurons
            ml_data = getDataFromJson();
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
        private void NextFun(object sender, RoutedEventArgs e)
        {
            if (step == ml_data.NeuronsList.Count)
            {
                for (int i = 0; i < TestAdd.Children.Count; i++)
                {
                    UIElement childe = TestAdd.Children[i];
                    if (childe is Ellipse ellipse)
                    {
                        string name = (string)childe.GetValue(NameProperty);
                        if (compareNameAndStep(name, step - 1) == 1) NeuronChangecolor(ellipse, 0);
                    }
                }
                step = 0;
            }
            else if (step == 0)
            {
                NextBtn.Content = "Next";
                for (int i = 0; i < TestAdd.Children.Count; i++)
                {
                    UIElement childe = TestAdd.Children[i];
                    string name = (string)childe.GetValue(NameProperty);
                    if (compareNameAndStep(name, step) == 1) 
                    {
                        int id = (int)name[1] - 48;
                        ml_data.Links[id].Value = ml_data.Inputs[id].Value;
                        if (childe is Ellipse ellipse)
                        {
                            NeuronChangecolor(ellipse, 1);
                        }
                        if (childe is Grid grdblock)
                        {
                            if(grdblock.Children[0] is TextBlock txtblock) txtblock.Text = ml_data.Inputs[id].Value.ToString();
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < ml_data.Links.Count; i++)
                {
                    List<double> tmp = new List<double>();
                    string name = ml_data.Links[i].Index;
                    if (compareNameAndStep(name, step) == 1)
                    {
                        double value = 0;
                        for (int j = 0; j < ml_data.Links.Count; j++)
                        {
                            if (compareNameAndStep(ml_data.Links[j].Index, step - 1) == 1)
                            {
                                double curr_value = 0;
                                for (int k = 0; k < ml_data.Links[j].Paths.Count; k++)
                                {
                                    if (ml_data.Links[j].Paths[k].Path == name)
                                    {
                                        curr_value = ml_data.Links[j].Paths[k].Weight * ml_data.Links[j].Value;
                                        break;
                                    }
                                }
                                value += curr_value;
                            }
                        }
                        ml_data.Links[i].Value = value;
                    }
                }

                // change color

                for (int i = 0; i < TestAdd.Children.Count; i++)
                {
                    UIElement childe = TestAdd.Children[i];
                    string name = (string)childe.GetValue(NameProperty);
                    if (compareNameAndStep(name, step) == 1)
                    {
                        for (int j = 0; j < ml_data.Links.Count; j++)
                        {
                            if ("N" + ml_data.Links[j].Index == name || "N" + ml_data.Links[j].Index + "_gr" == name)
                            {
                                if (childe is Ellipse ellipse)
                                {
                                    NeuronChangecolor(ellipse, 1);
                                }
                                if (childe is Grid grdblock)
                                {
                                    if (grdblock.Children[0] is TextBlock txtblock) txtblock.Text = ml_data.Links[j].Value.ToString();
                                }
                                break;
                            }
                        }
                    }
                    if (compareNameAndStep(name, step - 1) == 1 && childe is Ellipse ell) NeuronChangecolor(ell, 0);  // меняем цвет предыдущих нейронов
                }


            }
            step ++;
        }
    }
}
