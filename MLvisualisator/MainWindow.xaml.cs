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

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private MlData addDataFromJson()
        {
            MlData ml_data = new MlData();
            using (FileStream fstream = File.OpenRead("ml_config.json"))
            {
                // преобразуем строку в байты
                byte[] array = new byte[fstream.Length];
                // считываем данные
                fstream.Read(array, 0, array.Length);
                // декодируем байты в строку
                string json_file = System.Text.Encoding.Default.GetString(array);
                ml_data = JsonConvert.DeserializeObject<MlData>(json_file);
            }
            return ml_data;
        }

        private void addNeuron(string id, int columCanvas, int rowCanvas)
        {
            Ellipse ell = new Ellipse();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(28, 28, 28, 0);
            ell.Width = 30;
            ell.Height = 30;
            ell.Fill = mySolidColorBrush;
            ell.Name = id;

            Canvas.SetLeft(ell, columCanvas);
            Canvas.SetTop(ell, rowCanvas);
            Panel.SetZIndex(ell, 2);
            Canvas.SetZIndex(ell, 2);

            TestAdd.Children.Add(ell);
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

            Line line = new Line();
            line.X1 = x1 + 15;
            line.X2 = x2 + 15;
            line.Y1 = y1 + 15;
            line.Y2 = y2 + 15;
            line.Name = "W" + start + "_" + end;
            line.StrokeThickness = 4;

            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(120, 255, 0, 0);

            line.Stroke = mySolidColorBrush;

            Panel.SetZIndex(line, 0);
            Canvas.SetZIndex(line, 0);

            TestAdd.Children.Add(line);
        }
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
            MlData ml_data = addDataFromJson();
            int colums = ml_data.NeuronsList.Count;
            int columCanvas = 0;

            for (int i = 0; i < colums; i++)
            {
                int rowCanvas = 0;
                int rows = ml_data.NeuronsList[i].Count;
                for (int j = 0; j < rows; j++)
                {
                    string ind = $"N{j}{i}";
                    addNeuron(ind, columCanvas, rowCanvas);
                    rowCanvas += 50;
                }
                TestAdd.Height = rowCanvas;
                columCanvas += 60;
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

        }
    }
}
