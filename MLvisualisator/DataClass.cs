using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

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

        private double ShareOfWeight(List<CurrPath> paths, double curr_path)
        {
            double sum = 0;
            for (int i = 0; i < paths.Count; i++) sum += paths[i].Weight;
            return curr_path / sum;
        }

        private List<double> MatrixMultiplication(List<double> I, List<List<double>> W)
        {
            List<double> X = new List<double>();

            // X = W * I

            for(int i = 0; i < W.Count; i++)
            {
                double element = 0.0;
                for(int j = 0; j < W[i].Count; j++)
                {
                    element += W[i][j] * I[j];
                }
                X.Add(element);
            }

            return X;
        }
    }
}
