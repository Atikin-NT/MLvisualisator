using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace MLvisualisator
{
    public partial class MainWindow
    {
        private MlData getDataFromJson()
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
        private int CompareNameAndStep(string name, int step)
        {
            string strStep = step.ToString();
            int j = 0;
            int startFlag = 0;
            for (int i = 1; i < name.Length; i++)
            {
                if (name[i] == '_' && startFlag == 1) break;
                if (name[i] == '_')
                {
                    startFlag = 1;
                    continue;
                }
                if (startFlag == 1)
                {
                    if (name[i] != strStep[j]) return 0;
                    j++;
                }
            }
            return 1;
        }

        private int GetRowFromName(string name)
        {
            int row = 0;
            for(int i = 0; i < name.Length; i++)
            {
                if (name[i] == '_') break;
                row += (row * 10) + name[i] - 48;
            }
            return row;
        }

        private double ActivationFunction(double value)
        {
            String curr = (1.0 / (1 + Math.Exp(-value))).ToString();
            double res = 0.0;
            for(int i = 2; i < curr.Length; i++)
            {
                res += Math.Pow(0.1, i - 1) * (curr[i] - 48);
                if (i == 5) break;
            }
            return res;
        }

        private double ShareOfWeight(List<double> paths, double curr_path)
        {
            double sum = 0;
            for (int i = 0; i < paths.Count; i++) sum += paths[i];
            return curr_path / sum;
        }

        private int CompareDrawNameAndClassName(string draw_name, string class_name, int line = 0, string to_name = "")
        {
            if (line == 1)
            {
                int flag = 0;
                int i = 0;
                for (; i < draw_name.Length - 1; i++)
                {
                    if (draw_name[i + 1] == '_' && flag == 1) break;
                    if (draw_name[i + 1] == '_') flag = 1;
                    if (draw_name[i + 1] != class_name[i]) return 0;
                }
                i++;
                for ( int j = 0; j < to_name.Length; j++)
                {
                    if (draw_name[i] != to_name[j]) return 0;
                    i++;
                }

            }
            else
            {
                for (int i = 0; i < draw_name.Length - 1; i++)
                {
                    if (draw_name[i + 1] != class_name[i]) return 0;
                }
            }
            return 1;
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

            for (int i = 0; i < ml_data.DrawNeuron.Count; i++)
            {
                Ellipse ell = ml_data.DrawNeuron[i].Ell;
                if (CompareNameAndStep(ell.Name, curr_step) == 1)
                {
                    NeuronChangecolor(ell, 1);
                    TextBlock txtBlock = ml_data.DrawNeuron[i].EllTxt;
                    txtBlock.Text = ml_data.Links[i].Value.ToString();

                    double sum = 0;

                    for (int j = 0; j < ml_data.Links[i].Paths.Count; j++)
                    {
                        double tmp = ml_data.Links[i].Paths[j].Weight > 0 ? ml_data.Links[i].Paths[j].Weight : -ml_data.Links[i].Paths[j].Weight;
                        sum += tmp;

                    }

                    for (int j = 0; j < ml_data.DrawNeuron[i].Paths.Count; j++)
                    {
                        Line line = ml_data.DrawNeuron[i].Paths[j];
                        double tmp = ml_data.Links[i].Paths[j].Weight > 0 ? ml_data.Links[i].Paths[j].Weight : -ml_data.Links[i].Paths[j].Weight;
                        LineChangeColor(line, tmp / sum);
                    }
                }
                else
                {
                    NeuronChangecolor(ell, 0);
                }
            }
        }
        private void FindErrors()
        {
            int curr_step = ml_data.NeuronsList.Count - 1 - step % ml_data.NeuronsList.Count;
            for (int i = ml_data.Links.Count - 1; i > 0; i--) // пробегаемся с конца
            {
                if (CompareNameAndStep(ml_data.Links[i].Index, curr_step) == 1)
                {
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
                        for (int k = 0; k < ml_data.Links[j].Paths.Count; k++)
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
            // change color

            for (int i = 0; i < ml_data.DrawNeuron.Count; i++)
            {
                Ellipse ell = ml_data.DrawNeuron[i].Ell;
                if (CompareNameAndStep(ell.Name, curr_step) == 1)
                {
                    NeuronChangecolor(ell, 1);
                }
                else
                {
                    NeuronChangecolor(ell, 0);
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
    }
}
