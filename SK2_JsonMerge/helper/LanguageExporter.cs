using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace SK2_JsonMerge.helper
{
    class LanguageExporter
    {
        public string ExportPath { get; private set; }
        public LanguageExporter(string exportPath)
        {
            ExportPath = exportPath;
        }

        public string SerializeObject(ItemCollection items)
        {
            
            List<LocalText> data = new List<LocalText>();
            foreach (var item in items)
            {
                if (item.GetType().Name == "LocalText")
                    data.Add(item as LocalText);
            }

            //用JObject回推
            //https://dotblogs.com.tw/shadow/archive/2012/08/16/74099.aspx
            List<JProperty> listJProp = new List<JProperty>();
            foreach (LocalText item in data.OrderBy(p => p.Id))
            {
                listJProp.Add(new JProperty(item.Name, item.LocalValue.Replace("\"", "''")));
            }
            JObject obj = new JObject(listJProp.ToArray());

            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        #region Sub Function

        #endregion
    }
}