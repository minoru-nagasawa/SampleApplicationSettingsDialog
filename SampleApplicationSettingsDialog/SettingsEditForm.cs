using SampleApplicationSettingsDialog.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SampleApplicationSettingsDialog
{
    public partial class SettingsEditForm : Form
    {
        /// <summary>
        /// オブジェクトのディープコピーを作成する
        /// </summary>
        public static T deepCopy<T>(T src)
        {
            using (var memoryStream = new System.IO.MemoryStream())
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, src);
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// 設定のコピーを保管する
        /// </summary>
        private Settings copiedSettings;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SettingsEditForm()
        {
            InitializeComponent();

            // StringCollectionの初期の編集画面では追加ができない。
            // これを実行することで編集画面が変わり、追加できるようになる。
            TypeDescriptor.AddAttributes(typeof(System.Collections.Specialized.StringCollection),
                                         new EditorAttribute("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
                                         typeof(System.Drawing.Design.UITypeEditor)));

            // 設定のコピーを作成する
            copiedSettings = new Settings();
            foreach (SettingsProperty property in Settings.Default.Properties)
            {
                // StringCollection型のために必要
                copiedSettings[property.Name] = deepCopy(Settings.Default[property.Name]);
            }

            // コピーしたオブジェクトを表示させる
            propertyGrid1.SelectedObject = copiedSettings;
        }

        /// <summary>
        /// 編集値を保存する
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSave_Click(object sender, EventArgs e)
        {
            foreach (SettingsProperty property in Settings.Default.Properties)
            {
                // StringCollection型のために必要
                Settings.Default[property.Name] = deepCopy(copiedSettings[property.Name]);
            }
            Settings.Default.Save();
            MessageBox.Show("保存しました");
        }

        /// <summary>
        /// 現在の保存されている値をエクスポートする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonExport_Click(object sender, EventArgs e)
        {
            // ファイルを選択
            string fullPath;
            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = "user.config";
                sfd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                sfd.Filter = "設定ファイル(*.config)|*.config";
                sfd.FilterIndex = 1;
                sfd.Title = "エクスポート先のファイルを選択してください";
                sfd.RestoreDirectory = true;
                if (sfd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                fullPath = sfd.FileName;
            }

            // ファイルをコピー
            try
            {
                // user.configのパスを取得
                string userConfigPath = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath;

                // ファイルが無ければSave()して生成する
                if (!File.Exists(userConfigPath))
                {
                    Settings.Default.Save();
                }

                // エクスポートはファイルをコピーするだけ
                File.Copy(userConfigPath, fullPath, true);
                MessageBox.Show("エクスポートしました");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "エクスポート失敗", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// ファイルから設定をインポートする
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonImport_Click(object sender, EventArgs e)
        {
            // ファイル選択
            string fullPath = "";
            using (var ofd = new OpenFileDialog())
            {
                ofd.FileName = "user.config";
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                ofd.Filter = "設定ファイル(*.config)|*.config";
                ofd.FilterIndex = 1;
                ofd.Title = "インポートするファイルを選択してください";
                ofd.RestoreDirectory = true;
                if (ofd.ShowDialog() != DialogResult.OK)
                {
                    return;
                }

                fullPath = ofd.FileName;
            }

            // 読み込み
            ClientSettingsSection section = null;
            try
            {
                // ExeConfigFilenameにインポートするファイルだけ指定しても、そのファイルにはセクション情報が書かれていないためGetSectionで正しく読めない。
                // さらに、ExeConfigFilenameにアプリケーション設定、RoamingUserConfigFilenameにインポートするファイルを指定しても、正しく動かない場合がある。
                // 例えばインポートするファイルに吐かれていない新規設定がある場合、本来は現在値を保持してほしいが、デフォルト値で上書きしてしまう。
                // ということで、ExeConfigFilename/RoamingUserConfigFilenam/LocalUserConfigFilenameの3つを指定して読み込む。
                var tmpAppConfig  = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var tmpUserCOnfig = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                var exeFileMap = new ExeConfigurationFileMap
                {
                    ExeConfigFilename         = tmpAppConfig.FilePath,
                    RoamingUserConfigFilename = tmpUserCOnfig.FilePath,
                    LocalUserConfigFilename   = fullPath
                };
                var config = ConfigurationManager.OpenMappedExeConfiguration(exeFileMap, ConfigurationUserLevel.PerUserRoamingAndLocal);
                section = (ClientSettingsSection)config.GetSection($"userSettings/{typeof(Settings).FullName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "インポート失敗", MessageBoxButtons.OK);
                return;
            }

            // データの更新
            try
            {
                // Key:プロパティ名、Value:読み込んだファイルの該当プロパティのSettingElement、のDictionaryを作成する
                var dict = new Dictionary<string, SettingElement>();
                foreach (SettingElement v in section.Settings)
                {
                    dict.Add(v.Name, v);
                }

                // 現在の設定を更新する
                foreach (SettingsPropertyValue value in copiedSettings.PropertyValues)
                {
                    SettingElement element;
                    if (dict.TryGetValue(value.Name, out element))
                    {
                        // SerializedValueを1度も参照していないと、参照したときの元の値に戻ってしまうという仕様になっている。
                        // https://referencesource.microsoft.com/#System/sys/system/configuration/SettingsPropertyValue.cs,69
                        // その対策として、リフレクションで無理やり内部のメンバをfalseに変更する。
                        // リフレクションを使わなくても、var dummy = value.SerializedValueを実行して1度参照する方法でもよい。
                        var _ChangedSinceLastSerialized = typeof(SettingsPropertyValue).GetField("_ChangedSinceLastSerialized", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Instance);
                        _ChangedSinceLastSerialized.SetValue(value, false);

                        // 値の設定
                        value.SerializedValue = element.Value.ValueXml.InnerXml;

                        // value.Deserializedをfalseにすると、value.PropertyValueにアクセスしたときにDeserializeされる.
                        // https://referencesource.microsoft.com/#System/sys/system/configuration/SettingsPropertyValue.cs,40
                        value.Deserialized = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "インポート失敗", MessageBoxButtons.OK);
                return;
            }

            // 画面を更新
            propertyGrid1.SelectedObject = copiedSettings;

            // メッセージ
            MessageBox.Show("インポートした設定を反映するには保存を押してください");
        }
    }
}
