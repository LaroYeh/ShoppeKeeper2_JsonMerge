using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Windows;



namespace SK2_JsonMerge
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public class JsonObject
        {
            public float Id { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }
        public class ExtraObject : JsonObject
        {
            public string PrevName { get; set;  } //上一個跟比較物有重複的名字
        }
        public class LocalText : JsonObject
        {
            //public float Id { get; set; }
            //public string Name { get; set; }
            //public string Value { get; set; }
            public string LocalValue { get; set; } 
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            string msg = string.Empty;
            int index = 0;
            List<JsonObject> RefObj = new List<JsonObject>();
            List<JsonObject> LocalObj = new List<JsonObject>();
            List<ExtraObject> ExtraObj = new List<ExtraObject>();
            List<LocalText> LocalText = new List<LocalText>();
            
            //序列化兩邊，先用linq組出大概
            try
            {
                #region Deserialize Global Json
                //Reference: https://stackoverflow.com/questions/13652983/dynamic-jcontainer-json-net-iterate-over-properties-at-runtime
                string NameTxt = File.ReadAllText(tbNameFrom.Text).Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty);
                dynamic dynObj_name = JsonConvert.DeserializeObject(NameTxt);

                //JContainer is the base class
                index = 0;
                foreach (JToken token in (dynObj_name as JObject).Children())
                {
                    index++;
                    if (token is JProperty)
                    {
                        var prop = token as JProperty;
                        RefObj.Add(new JsonObject() { Id = index ,Name = prop.Name, Value = prop.Value.ToString() });
                    }
                }
                #endregion
                #region Deserialize Local Json
                //Reference: https://stackoverflow.com/questions/13652983/dynamic-jcontainer-json-net-iterate-over-properties-at-runtime
                string ValueTxt = File.ReadAllText(tbValueFrom.Text).Replace(Environment.NewLine, string.Empty).Replace("\t", string.Empty);
                dynamic dynObj_value = JsonConvert.DeserializeObject(ValueTxt);

                //JContainer is the base class
                index = 0;
                foreach (JToken token in (dynObj_value as JObject).Children())
                {
                    index++;
                    if (token is JProperty)
                    {
                        var prop = token as JProperty;
                        LocalObj.Add(new JsonObject() { Id = index, Name = prop.Name, Value = prop.Value.ToString() });
                    }
                }
                #endregion

                #region Merge Data
                //取得loc比對照物還多的部分，以及定位用，其前一個的屬性名
                var names = RefObj.Select(p => p.Name);
                var loc_names = LocalObj.Select(p => p.Name);
                var extra_names = loc_names.Except(names);
                index = 0;
                foreach (var name in extra_names)
                {
                    index++;
                    var locObj = LocalObj.Where(p => p.Name == name).First();   
                    ExtraObj.Add(new ExtraObject() { Id = index,
                        Name = locObj.Name,
                        Value = locObj.Value,
                        PrevName = getPrevName(LocalObj, locObj.Name, extra_names.ToArray()), 
                    });
                }

                //正式開始Merge
                var data = (from r in RefObj
                            join l in LocalObj on r.Name equals l.Name into _l
                            from l in _l.DefaultIfEmpty()

                            select new LocalText
                            {
                                Id = r.Id,
                                Name = r.Name,
                                Value = r.Value,
                                LocalValue = (l != null) ? l.Value : string.Empty
                            })
                           .Union( from extra in ExtraObj
                                   select new LocalText
                                   {
                                       Id = Convert.ToSingle(string.Format("{0}.{1}", RefObj.Where(p=>p.Name == extra.PrevName).First().Id, extra.Id)),
                                       Name = extra.Name,
                                       Value = "(Localization Only)",
                                       LocalValue = extra.Value
                                   })
                                   .OrderBy(p => p.Id);

                var vData = data.ToList();
                dgPreview.ItemsSource = vData;
                #endregion

            }
            catch (Exception ex)
            {
                msg = ex.Message;
                System.Windows.Forms.MessageBox.Show(msg);
            }
        }
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            string msg = string.Empty;
            try
            {
                //Reference: http://www.wpf-tutorial.com/dialogs/the-savefiledialog/
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
                saveFileDialog.Filter = "Text file (*.txt)|*.txt";
                saveFileDialog.FileName = Path.GetFileName(tbValueFrom.Text);    //預設抓Localization File的檔名
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //取得DataGrid的資料
                    List<LocalText> data = new List<LocalText>();
                    var b = dgPreview.Items;
                    foreach (var item in b)
                    {
                        if (item.GetType().Name == "LocalText")
                            data.Add(item as LocalText);
                    }

                    #region 用JObject回推
                    //https://dotblogs.com.tw/shadow/archive/2012/08/16/74099.aspx
                    List<JProperty> listJProp = new List<JProperty>();
                    foreach (LocalText item in data.OrderBy(p => p.Id))
                    {
                        listJProp.Add(new JProperty(item.Name, item.LocalValue.Replace("\"", "''")));
                    }
                    JObject obj = new JObject(listJProp.ToArray());
                    string json = JsonConvert.SerializeObject(obj, Formatting.Indented);

                    #endregion
                    File.WriteAllText(saveFileDialog.FileName, json); //輸出檔案
                    msg = "Export Completed";

                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            finally
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    System.Windows.Forms.MessageBox.Show(msg);
                }
            }
        }


        /// <summary>
        /// 取得最後一次有重複的屬性名(可能連續兩個都沒有，所以用遞迴處理)
        /// 如果Reference少很多，效能會超級拖，還在想辦法節省步驟
        /// </summary>
        /// <param name="localObj">尋找的母體</param>
        /// <param name="name">額外的屬性名</param>
        /// <param name="extraName">所有的格外名</param>
        /// <returns></returns>
        private string getPrevName(List<JsonObject> localObj, string name, string[] extraName)
        {
            var preID = (localObj.Where(p => p.Name == name).FirstOrDefault().Id) -1;
            var preName = localObj.Where(p => p.Id == preID).First().Name;
            if (extraName.Contains(preName))
            {
                //上一個也是參照來源沒有的
                return getPrevName(localObj, preName, extraName);
            }
            else
            {
                //上一個參照來源有
                return preName;
            }
        }

        private void btnNameFrom_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbNameFrom.Text = dialog.FileName; //顯示選取的路徑
            }
        }

        private void btnValueFrom_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbValueFrom.Text = dialog.FileName; //顯示選取的路徑
            }
        }
    }
}
