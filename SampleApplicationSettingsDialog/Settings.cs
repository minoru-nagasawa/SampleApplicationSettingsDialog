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
            // PropertyGridのCategoryを設定する
            var categoryTable = new Dictionary<string, string>()
            {
                { nameof(Settings.BoolSetting),             "組み込みのデータ型" },
                { nameof(Settings.StringSetting),           "組み込みのデータ型" },
                { nameof(Settings.StringCollectionSetting), "複合データ型" },
                { nameof(Settings.DateTimeSetting),         "複合データ型" },
                { nameof(Settings.IntSetting),              "組み込みのデータ型" },
            };
            addCategory(categoryTable);

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
        /// プロパティにCategory属性を追加する
        /// </summary>
        /// <param name="categoryTable">Key:プロパティ名、Value:テキスト</param>
        private void addCategory(Dictionary<string, string> categoryTable)
        {
            if (categoryTable == null)
            {
                return;
            }

            var properties = TypeDescriptor.GetProperties(this);
            foreach (PropertyDescriptor p in properties)
            {
                string text;
                if (categoryTable.TryGetValue(p.Name, out text))
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
                    listAttr.Add(new CategoryAttribute(text));
                    fi.SetValue(p.Attributes, listAttr.ToArray());
                }
            }
        }

        /// <summary>
        /// プロパティにDescription属性を追加する
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
