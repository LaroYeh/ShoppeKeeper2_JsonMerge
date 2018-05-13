using System;
using System.IO;
using SK2_JsonMerge.helper;
using System.Windows;
using System.Windows.Forms;
using System.Data;
using System.Collections.Generic;

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

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LanguageMerger merger = new LanguageMerger(tbNameFrom.Text, tbValueFrom.Text);
                dgPreview.ItemsSource = merger.GetMergePriview();
            }
            catch (Exception ex)
            {
                //有錯誤時要出現例外資訊

                System.Windows.MessageBox.Show(ex.Message);
            }
        }
        private void Export_Click(object sender, RoutedEventArgs e)
        {
            string msg = string.Empty;
            try
            {
                //Reference: http://www.wpf-tutorial.com/dialogs/the-savefiledialog/
                SaveFileDialog saveFileDialog = new SaveFileDialog() {
                    Filter = "Text file (*.txt)|*.txt",
                    FileName = Path.GetFileName(tbValueFrom.Text)   //預設抓Localization File的檔名
                };
                if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    //取得DataGrid的資料
                    LanguageExporter exporter = new LanguageExporter(saveFileDialog.FileName);
                    string json = exporter.SerializeObject(dgPreview.Items);    //從DataGrid取得資料
                    File.WriteAllText(exporter.ExportPath, json); //輸出檔案
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

        private void NameFrom_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog() { Multiselect = false};
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbNameFrom.Text = dialog.FileName; //顯示選取的路徑
            }
        }

        private void ValueFrom_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog() { Multiselect = false };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tbValueFrom.Text = dialog.FileName; //顯示選取的路徑
            }
        }

        private void AutoFill_Click(object sender, RoutedEventArgs e)
        {
            //如Local沒有Value, Ref有Value，就從Ref寫過去
            List<LocalText> data = new List<LocalText>();
            foreach (var item in dgPreview.Items)
            {                
                if (item.GetType().Name == "LocalText")
                {
                    LocalText t = (item as LocalText);
                    t.LocalValue = (string.IsNullOrEmpty(t.LocalValue)) ? t.Value : t.LocalValue;
                    data.Add(t);
                }
            }
            dgPreview.DataContext = data;
            dgPreview.Items.Refresh();
        }
    }
}
