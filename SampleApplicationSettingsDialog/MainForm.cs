using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SampleApplicationSettingsDialog.Properties;

namespace SampleApplicationSettingsDialog
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            // データグリッドを更新
            this.dataGridView1.Columns.Add("Name", "Name");
            this.dataGridView1.Columns.Add("Value", "Value");
            this.dataGridView1.Columns[1].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            Settings.Default.SettingChanging += (s, e) => updateDataGrid();
            updateDataGrid();
        }

        /// <summary>
        /// プロパティの編集画面を開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOpen_Click(object sender, EventArgs e)
        {
            using (var form = new SettingsEditForm())
            {
                form.ShowDialog();
            }
        }

        /// <summary>
        /// DataGridを最新の値に更新する
        /// </summary>
        private void updateDataGrid()
        {
            this.dataGridView1.Rows.Clear();
            foreach (SettingsProperty property in Settings.Default.Properties)
            {
                if (Settings.Default[property.Name] is System.Collections.Specialized.StringCollection)
                {
                    var collection = (Settings.Default[property.Name] as System.Collections.Specialized.StringCollection).Cast<string>();
                    this.dataGridView1.Rows.Add(property.Name, string.Join("\n", collection));
                }
                else
                {
                    this.dataGridView1.Rows.Add(property.Name, Settings.Default[property.Name].ToString());
                }
            }
        }

        /// <summary>
        /// user.configのディレクトリを開く。
        /// 存在しない場合があるので、存在するところまでパスを削って開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonOpenDir_Click(object sender, EventArgs e)
        {
            string userConfigPath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;
            string directory = Path.GetDirectoryName(userConfigPath);
            while (!string.IsNullOrEmpty(directory))
            {
                if (Directory.Exists(directory))
                {
                    Process.Start(directory);
                    return;
                }
                directory = Path.GetDirectoryName(directory);
            };
        }
    }
}
