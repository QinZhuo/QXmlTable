﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
namespace XmlTable
{
    public static class ExtendsMnager
    {
        public static bool IsXml(this string str)
        {
            return str.Contains("<") && str.Contains("</") && str.Contains(">");
        }
        public static void ChangeValue(this DataGridViewCell cell, string newValue)
        {
            var tableView=cell.DataGridView;
            if (cell.OwningColumn.Name == DataTableExtend.IndexCol)
            {
                return;
            }
            if (cell.RowIndex >= tableView.Rows.Count - 1)
            {
                var data = tableView.DataSource as DataTable;
                data.Rows.Add(data.NewRow());
            }
            if (newValue != cell.Value.ToString()) {

                cell.Value = newValue;
                XmlTableEditor.mainTable.CheckViewType(cell.ColumnIndex, cell.RowIndex);
            }
          
        }
        public static string FixXmlValue(this string value)
        {
            value = value.Replace("<", "&lt;");
            value = value.Replace(">", "&gt;");
            value = value.Replace("'", "&apos;");
            value = value.Replace("\"", "&quot;");
            return value;
        }
        public static string FixViewValue(this string value)
        {
            value = value.Replace("&lt;", "<");
            value = value.Replace("&gt;", ">");
            value = value.Replace("&apos;", "'");
            value = value.Replace("&quot;", "\"");
            return value;
        }
        public static string GetXmlInnerString(this string value)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(" <子单元格编辑>" + value + "</子单元格编辑>");
            var str = "";
            foreach (XmlNode node in xmlDoc.FirstChild.ChildNodes)
            {
                str += node.InnerXml+"\n";
            }
            return str;
        }
    }
    public enum ViewType
    {
        文字,
        下拉框,
        按钮,
        表索引,
        脚本,
    }
    public class ColInfo
    {
        [XmlElement("列名")]
        public string key;
        [XmlElement("显示类型")]
        public ViewType type= ViewType.文字;
        [XmlArray("参数")]
        public string[] typeValues;
        [XmlIgnore]
        public bool init=false;
        [XmlIgnore]
        public List<string> values=new List<string>();
        [XmlIgnore]
         Dictionary<string, string> dic=new Dictionary<string, string>();
        public bool Contains(string key)
        {
            return values.Contains(GetValue(key));
        }
        public string GetDisplay(string key)
        {
            var value=GetValue(key);
            if(value.Contains(" = "))
            {
                return value.Split('=')[1];
            }
            else
            {
                return key;
            }
        }
        public string GetValue(string key)
        {
            if (dic.ContainsKey(key))
            {
                return dic[key];
            }
            else
            {
                return key;
            }
        }
        public string GetKey(string value)
        {
            foreach (var kv in dic)
            {
                if (kv.Value == value)
                {
                    return kv.Key;
                }
            }
            return value;
        }
        public string TablePath
        {
            get
            {
                return XmlTableEditor.mainTable.folderPath + '\\' + typeValues[0];
            }
        }
        public void Init()
        {
            values.Clear();
            dic.Clear();
            switch (type)
            {
                case ViewType.文字:
                    break;
                case ViewType.下拉框:
                    values.AddRange(typeValues);
                    break;
                case ViewType.按钮:
                    break;
                case ViewType.脚本:
                    break;
                case ViewType.表索引:
                 
                    try
                    {
                        if (typeValues.Length < 3)
                        {
                            MessageBox.Show("【索引数据读取错误】["+key+"]参数不足3" );
                            break;
                        }
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.Load(TablePath);
                        var nodes = xmlDoc.SelectNodes("//" + typeValues[1]);
                        foreach (XmlNode node in nodes)
                        {
                            var valueNode = node.ParentNode.SelectSingleNode(typeValues[2]);
                            var value = node.InnerText == valueNode.InnerText ? node.InnerText : node.InnerText + " = " + valueNode.InnerText;
                        
                            // node.ParentNode.SelectSingleNode(typeValues[2]).Value
                            if (!dic.ContainsKey(node.InnerText))
                            {
                                values.Add(value);
                                dic.Add(node.InnerText, value);
                            }
                        
                        }
                    }
                    catch (Exception e)
                    {
                    //    MessageBox.Show("【索引数据读取错误】["+key+"]["+ XmlTableEditor.mainTable.folderPath+ "]" + e);
                        throw;
                    }
                  
                    break;
                default:
                    break;
            }
            init = true;
        }
        public override string ToString()
        {
            string info = key;
            info += "[" + type + "]:";
            foreach (var item in values)
            {
                info += item + " ";
            }
            return info;
        }
    }
    public class XmlTableInfo
    {
        [XmlElement("列信息")]
        public List<ColInfo> colListInfo = new List<ColInfo>();
        public ColInfo this[string key]
        {
            get
            {
                foreach (var colInfo in colListInfo)
                {
                    if (colInfo.key == key)
                    {
                        if (!colInfo.init)
                        {
                            colInfo.Init();
                        }
                        return colInfo;
                    }
                }
                return null;
            }
        }
    }
    public class TextCell : DataGridViewTextBoxCell
    {
        //public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
        //{
        //    return formattedValue.ToString().FixXmlValue();
        //}
        public TextCell():base()
        {
            Style.WrapMode = DataGridViewTriState.True;
            Style.Tag = true;
        }
        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            
            return value.ToString().FixViewValue();
        }
        //protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        //{

        //    base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value.ToString().FixXmlValue(), formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
        //}

    }
    public class InnerXmlCell: DataGridViewButtonCell
    {
        
        //protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        //{
        //    if (string.IsNullOrEmpty(value.ToString()))
        //    {
        //        return ""
        //    }
        //    return FileManager.Load(value.ToString())
        //}
        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            //  var viewValue = value.ToString().Substring(0, Math.Min(value.ToString().Length, 10)) + "...";
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(" <子单元格编辑>" + value + "</子单元格编辑>");
            var str = "";
            foreach (XmlNode node in xmlDoc.FirstChild.ChildNodes)
            {
                foreach (XmlNode subNode in node.ChildNodes)
                {
                    str += subNode.InnerText + " ";
                }
                str += "   ";
            }
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, str, errorText, cellStyle, advancedBorderStyle, paintParts);
        }
    }
    public class ScriptCell: InnerXmlCell
    {
    }
    public class DropDownCell: DataGridViewComboBoxCell
    {
        public ColInfo colInfo;
        public override object ParseFormattedValue(object formattedValue, DataGridViewCellStyle cellStyle, TypeConverter formattedValueTypeConverter, TypeConverter valueTypeConverter)
        {
            return colInfo.GetKey(formattedValue.ToString());
        }
        protected override object GetFormattedValue(object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
        {
            return colInfo.GetValue(value.ToString());
        }
        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, elementState, value, colInfo.GetDisplay(value.ToString()), errorText, cellStyle, advancedBorderStyle, paintParts);
        }
    }
    public class TableIndexCell : DataGridViewComboBoxCell
    {
    }
}
