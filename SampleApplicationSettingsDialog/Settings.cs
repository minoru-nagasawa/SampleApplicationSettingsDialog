using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace SampleApplicationSettingsDialog.Properties
{
    internal sealed partial class Settings
    {
        public Settings()
        {
            // PropertyGridのHelpテキストを設定する
            var descriptionTable = new Dictionary<string, string>()
            {
                { nameof(Settings.BoolSetting),             "bool型の設定" },
                { nameof(Settings.StringSetting),           "string型の設定" },
                { nameof(Settings.StringCollectionSetting), "複数のstring型の設定" },
                { nameof(Settings.DateTimeSetting),         "DateTime型の設定" },
                { nameof(Settings.IntSetting),              "int型の設定" },
            };
            addDescription(descriptionTable);
        }

        /// <summary>
        /// プロパティにDescriptionAttribute属性を追加する
        /// </summary>
        /// <param name="descriptionTable">Key:プロパティ名、Value:テキスト</param>
        private void addDescription(Dictionary<string, string> descriptionTable)
        {
            if (descriptionTable == null)
            {
                return;
            }

            var properties = TypeDescriptor.GetProperties(this);
            foreach (PropertyDescriptor p in properties)
            {
                string text;
                if (descriptionTable.TryGetValue(p.Name, out text))
                {
                    // 属性にDescriptionAttributeを追加する。
                    // 本当はMemberDescriptor.Attributes.Addのようにしたいのだが、Attributes属性はgetだけ定義されている。
                    // そのためリフレクションを使って属性を追加する
                    var fi = p.Attributes.GetType().GetField("_attributes", BindingFlags.NonPublic | BindingFlags.Instance);
                    var attrs = fi.GetValue(p.Attributes) as Attribute[];
                    var listAttr = new List<Attribute>();
                    if (attrs != null)
                    {
                        listAttr.AddRange(attrs);
                    }
                    listAttr.Add(new DescriptionAttribute(text));
                    fi.SetValue(p.Attributes, listAttr.ToArray());
                }
            }
        }
    }
}
