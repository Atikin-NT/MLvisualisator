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
        private int compareNameAndStep(string name, int step)
        {
            string strStep = step.ToString();
            int j = 0;
            int startFlag = 0;
            for (int i = 1; i < name.Length; i++)
            {
                if (name[i - 1] == '_') startFlag = 1;
                if (startFlag == 1)
                {
                    if (name[i] != strStep[j]) return 0;
                    j++;
                    if (j == strStep.Length) return 1;
                }
            }
            return 0;
        }
    }
}
