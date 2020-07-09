using System;
using System.Collections.Generic;
using System.Data;
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
            cell.Value = newValue;
            XmlTableEditor.mainTable.CheckViewType(cell.ColumnIndex, cell.RowIndex);
        }
    }
    public enum ViewType
    {
        文字,
        下拉框,
        按钮,
    }
    public class ColInfo
    {
        [XmlElement("列名")]
        public string key;
        [XmlElement("显示类型")]
        public ViewType type= ViewType.文字;
        [XmlArray("参数")]
        public string[] values;
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
                        return colInfo;
                    }
                }
                return null;
            }
        }
    }
    public class TextCell : DataGridViewTextBoxCell
    {


    }
    public class InnerXmlCell: DataGridViewButtonCell
    {
        

    }
    public class DropDownCell: DataGridViewComboBoxCell
    {

    }
}
