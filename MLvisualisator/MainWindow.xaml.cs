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
        public double Error { get; set; } = 0.0;
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

    public partial class MainWindow : Window
    {
        public int step = 0;
        public int reverse_flag = 0;
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

                // find errors
                List<double> errors = new List<double>();

                for(int i = ml_data.Links.Count - 1; i > ml_data.Links.Count - ml_data.Outputs.Count; i--)
                {
                    ml_data.Links[i].Error = (ml_data.Outputs[ml_data.Links.Count - 1 - i].Value - ml_data.Links[i].Value) * ml_data.Links[i].Value * (1 - ml_data.Links[i].Value);
                    for(int j = 0; j < ml_data.Links[i].Paths.Count; j++)
                    {
                        string path_Name = ml_data.Links[i].Paths[j].Path;
                        for(int k = i - 1; k > 0; k--)
                        {
                            if (ml_data.Links[k].Index == path_Name)
                            {
                                ml_data.Links[k].Error += ShareOfWeight(ml_data.Links[i].Paths, ml_data.Links[i].Paths[j].Weight) * ml_data.Links[i].Error * 0.85;
                            }
                        }
                    }
                }

                // edit global error



                for (int i = 0; i < TestAdd.Children.Count; i++)
                {
                    UIElement childe = TestAdd.Children[i];
                    if (childe is Ellipse ellipse)
                    {
                        string name = (string)childe.GetValue(NameProperty);
                        if (CompareNameAndStep(name, step - 1) == 1) NeuronChangecolor(ellipse, 0);
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
                    string name = (string)childe.GetValue(NameProperty);  // чисто для удобства записываем имя в переменную
                    if (CompareNameAndStep(name, step) == 1)
                    {
                        int row = GetRowFromName(name);
                        ml_data.Links[row].Value = ml_data.Inputs[row].Value;  // у нас всегда сначала в списке идут стартовые элементы, поэтому пишем [row]
                        if (childe is Ellipse ellipse)
                        {
                            NeuronChangecolor(ellipse, 1);
                        }
                        if (childe is Grid grdblock)
                        {
                            TextBlock txtBlock = (TextBlock)grdblock.Children[0];
                            txtBlock.Text = ml_data.Inputs[row].Value.ToString();
                        }
                    }
                }
            }
            else
            {
                for (int i = 1; i < ml_data.Links.Count; i++)
                {
                    string name = ml_data.Links[i].Index;
                    if (CompareNameAndStep(name, step) == 1)  // если мы попали на элемент в текущем столбце
                    {
                        double value = 0;  // будущее значение этого элемента
                        for (int j = i; j >= 0; j--)  // идем в обратную сторону, тк мы смотрим предыдущий столбец
                        {
                            if (CompareNameAndStep(ml_data.Links[j].Index, step - 1) == 1)  // если мы попали на элемент в предыдущем столбце
                            {
                                for (int k = 0; k < ml_data.Links[j].Paths.Count; k++)  // идем по ссылкам элемента из пред столбца
                                {
                                    if (ml_data.Links[j].Paths[k].Path == name)  // если есть ссылка на этот элемент
                                    {
                                        value += ml_data.Links[j].Paths[k].Weight * ml_data.Links[j].Value;  // добавляем значение
                                        break;  // и останвливаем
                                    }
                                }
                            }
                        }
                        ml_data.Links[i].Value = ActivationFunction(value);
                    }
                }

                // change color

                for (int i = 0; i < TestAdd.Children.Count; i += 2)
                {
                    UIElement childe = TestAdd.Children[i];
                    string name = (string)childe.GetValue(NameProperty);
                    if (name[0] == 'W') break;
                    if (CompareNameAndStep(name, step) == 1)
                    {
                        Ellipse ellipse = (Ellipse)childe;
                        NeuronChangecolor(ellipse, 1);
                        Grid grd = (Grid)TestAdd.Children[i + 1];
                        TextBlock txtBlock = (TextBlock)grd.Children[0];
                        txtBlock.Text = ml_data.Links[(i / 2)].Value.ToString();
                    }
                    if (CompareNameAndStep(name, step - 1) == 1 && childe is Ellipse ell) NeuronChangecolor(ell, 0);  // меняем цвет предыдущих нейронов
                }


            }
            step ++;
        }
    }
}
