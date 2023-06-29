using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebApp.Common.Utils
{
    public static class DictionaryUtility
    {
        public static Dictionary<string, string> GetDifferenceFromJson(string data1, string data2)
        {
            Dictionary<string, string> dataDict1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(data1);
            Dictionary<string, string> dataDict2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(data2);

            var data = dataDict1.Except(dataDict2).ToDictionary(x => x.Key, x => x.Value);
            return data;
        }

        public static Dictionary<string, string> GetIntersectFromJson(string data1, string data2)
        {
            Dictionary<string, string> dataDict1 = JsonConvert.DeserializeObject<Dictionary<string, string>>(data1);
            Dictionary<string, string> dataDict2 = JsonConvert.DeserializeObject<Dictionary<string, string>>(data2);

            var data = dataDict1.Intersect(dataDict2).ToDictionary(x => x.Key, x => x.Value);
            return data;
        }

        public static string ConvertToString<T>(this Dictionary<T, T> dict)
        {
            StringBuilder str = new();

            if (dict.IsNullOrEmpty())
            {
                return string.Empty;
            }

            var lastKey = dict.Keys.Last();

            foreach (var d in dict)
            {
                if (d.Key.Equals(lastKey))
                {
                    str.Append($"{d.Key}: {d.Value}");
                }
                else
                {
                    str.Append($"{d.Key}: {d.Value}, ");
                }
            }

            return str.ToString();
        }

        public static bool IsNullOrEmpty<T>(this Dictionary<T, T> dict)
        {
            return dict != null && dict.Count != 0;
        }
    }
}