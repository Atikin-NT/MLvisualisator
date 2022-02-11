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
        public double Value { get; set; }
    }
    class CurrPath
    {
        public string Path { get; set; }
        public double Weight { get; set; }
        public double Error { get; set; }

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
        private void Learn()
        {
            int curr_step = step % ml_data.NeuronsList.Count;
            for (int i = 0; i < ml_data.Links.Count; i++)
            {
                string name = ml_data.Links[i].Index;
                if (CompareNameAndStep(name, curr_step) == 1)  // если мы попали на элемент в текущем столбце
                {
                    double value = 0;  // будущее значение этого элемента
                    if (curr_step == 0)
                    {
                        if (i == ml_data.Inputs.Count) break;
                        NextBtn.Content = "Next";
                        reverse_flag = 0;
                        int row = GetRowFromName(name);
                        value = ml_data.Inputs[row].Value;  // у нас всегда сначала в списке идут стартовые элементы, поэтому пишем [row]
                    }
                    else
                    {
                        for (int j = i - 1; j >= 0; j--)  // идем в обратную сторону, тк мы смотрим предыдущий столбец
                        {
                            if (CompareNameAndStep(ml_data.Links[j].Index, curr_step - 1) == 1)  // если мы попали на элемент в предыдущем столбце
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
                if (CompareNameAndStep(name, curr_step) == 1)
                {
                    Ellipse ellipse = (Ellipse)childe;
                    NeuronChangecolor(ellipse, 1);
                    Grid grd = (Grid)TestAdd.Children[i + 1];
                    TextBlock txtBlock = (TextBlock)grd.Children[0];
                    txtBlock.Text = ml_data.Links[(i / 2)].Value.ToString();
                }
                if (curr_step != 0 && CompareNameAndStep(name, curr_step - 1) == 1 && childe is Ellipse ell) NeuronChangecolor(ell, 0);  // меняем цвет предыдущих нейронов
            }
        }
        private void FindErrors()
        {
            for (int i = ml_data.Links.Count - 1; i > 0; i--) // пробегаемся с конца
            {
                int curr_step = ml_data.NeuronsList.Count - 1 - step % ml_data.NeuronsList.Count;
                if (CompareNameAndStep(ml_data.Links[i].Index, curr_step) == 1){
                    if (curr_step == ml_data.NeuronsList.Count - 1) // последний слой
                    {
                        for (int j = 0; j < ml_data.Outputs.Count; j++) // сначало найдем ошибку на выходном слое
                        {
                            if (ml_data.Links[i].Index == ml_data.Outputs[j].Name)
                            {
                                ml_data.Links[i].Error = (ml_data.Outputs[j].Value - ml_data.Links[i].Value) * ml_data.Links[i].Value * (1 - ml_data.Links[i].Value);  // подсчет ошибки на выходном нейроне
                            }
                        }
                    }

                    // find all paths
                    List<double> weightList = new List<double>();

                    for (int j = 0; j < ml_data.Links.Count; j++)
                    {
                        for(int k = 0; k < ml_data.Links[j].Paths.Count; k++)
                        {
                            if (ml_data.Links[j].Paths[k].Path == ml_data.Links[i].Index) weightList.Add(ml_data.Links[j].Paths[k].Weight);
                        }
                    }

                    for (int j = i - 1; j > 0; j--)
                    {
                        if (CompareNameAndStep(ml_data.Links[j].Index, curr_step - 1) == 1) // если мы нашили нейрон из предыдущего слоя
                        {
                            for (int k = 0; k < ml_data.Links[j].Paths.Count; k++)
                            {
                                if (ml_data.Links[j].Paths[k].Path == ml_data.Links[i].Index)
                                {
                                    ml_data.Links[j].Error += ShareOfWeight(weightList, ml_data.Links[j].Paths[k].Weight) * ml_data.Links[i].Error;  // подсчет ошибки для предыдущего слоя нейронов
                                    ml_data.Links[j].Paths[k].Error = ml_data.Links[i].Value * (1 - ml_data.Links[i].Value) * ml_data.Links[i].Error * ml_data.Links[j].Value;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetNewData()
        {
            for (int i = 0; i < ml_data.Links.Count; i++)
            {
                for (int j = 0; j < ml_data.Links[i].Paths.Count; j++)
                {
                    ml_data.Links[i].Paths[j].Weight += ml_data.Links[i].Paths[j].Error;
                }
            }
        }

        private void NextFun(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < 100; i++)
            {
                if (step % (ml_data.NeuronsList.Count * 2) < ml_data.NeuronsList.Count)
                {
                    reverse_flag = 0;
                    Learn();
                }
                else
                {
                    reverse_flag = 1;
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
