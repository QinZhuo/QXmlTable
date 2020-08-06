using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
namespace XmlTable
{

    public partial class XmlTableEditor : Form
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(String classname, String title);
        [DllImport("user32.dll")]
        public static extern void SetForegroundWindow(IntPtr hwnd);
        public static XmlTableEditor mainTable;
        //DataTable data;
        // XmlDocument xml;
        public XmlTableInfo tableInfo=new XmlTableInfo();
        public DataGridView gridView
        {
            get
            {
                return tableView;
            }
        }
        public string xmlPath;
        public string folderPath;
        public DateTimeOffset fileChangeTime;
        XmlDocument xmlDoc;
        bool Loading = true;
        InnerData curData;

        public XmlTableEditor()
        {
            InitializeComponent();
        }
        public bool ColumnName(string name)
        {
            if (!curData.data.Columns.Contains(name))
            {
                curData.data.Columns.Add(name);
                return true;
            }
            return false;
        }
       
        private void XmlTableEditor_Load(object sender, EventArgs e)
        {
            var infos = Environment.GetCommandLineArgs();
            mainTable = this;
            if (infos.Length > 1)
            {
                string pathC = infos[1];
                OpenXml(pathC);
            }
            if (infos.Length > 2)
            {
                var findStr = infos[2];
                    FindAndSelect(findStr);
              
            }
            gridView.RowsDefaultCellStyle.BackColor = Color.WhiteSmoke;
            gridView.AlternatingRowsDefaultCellStyle.BackColor = Color.White;


        }
       
        private void tableView_DataSourceChanged(object sender, EventArgs e)
        {
        }
        private void ParseInnerXml(int col,int row, string xmlstr)
        {
            if (!IsXml(xmlstr)) return;
            var xmlDoc = new XmlDocument();
            var data = new DataTable();
          
            xmlDoc.LoadXml(" <子单元格编辑>" + xmlstr+ "</子单元格编辑>");
            var innerData = new InnerData(curData)
            {
                data = data,
                col = col,
                row = row,
                xml = xmlDoc,
                xmlDoc=xmlDoc,
                _path=curData.data.Columns[col].ColumnName,
            };
            curData = innerData;
            ParseXml(innerData);
           

        }
        public static bool IsXml(string str)
        {
            return str.Contains("<") && str.Contains("</")&& str.Contains(">");
        }
        public GridType GetGridType(XmlNode xml)
        {
            GridType type = GridType.none;
            bool nameCheck = true;
            var name = xml.FirstChild.Name;
            foreach (XmlNode node in xml.ChildNodes)
            {
                if (node.Name != name)
                {
                    nameCheck = false;
                    break;
                }
            }
            if (!IsXml(xml.FirstChild.InnerXml))
            {
                if (nameCheck)
                {
                    type = GridType.row;
                }
                else
                {
                    type = GridType.col;
                }
            }
            else
            {
                if (nameCheck)
                {
                    type = GridType.Grid;
                }
                else
                {
                    type = GridType.col;
                }

            }
            return type;
        }
     
        private void ParseXml(InnerData innerData)
        {
            var root = innerData.xml.LastChild;
            var colList = root.FirstChild;
            Loading = true;
            innerData.gridType = GetGridType(root);
            if (innerData.gridType== GridType.Grid)
            {
             
                int x = 0;
                foreach (XmlNode rowList in root.ChildNodes)
                {
                    innerData.data.Rows.Add();
                
                    int y = 0;
                    foreach (XmlNode cell in rowList.ChildNodes)
                    {
                        if (y == 0)
                        {
                            if (!innerData.data.Columns.Contains(DataTableExtend.IndexCol))
                            {
                                var col= innerData.data.Columns.Add(DataTableExtend.IndexCol,typeof(int));
                              
                            }
                            innerData.data.Rows[x][DataTableExtend.IndexCol] = x;
                        }
                            if (!innerData.data.Columns.Contains(cell.Name))
                            {
                                var col= innerData.data.Columns.Add(cell.Name);
                 
                        }
                            innerData.data.Rows[x][cell.Name] = cell.InnerXml;
                       
                        y++;
                    }
                    x++;
                 
                }
            }
            else
            {
                innerData.data.Columns.Add(colList.Name);
              
                int x = 0;
                foreach (XmlNode cell in root.ChildNodes)
                {
                    
                    int y = 0;
                  
                    if (!innerData.data.Columns.Contains(cell.Name))
                    {
                        innerData.data.Columns.Add(cell.Name);
                  
                    }
                   
                    innerData.data.Rows.Add();
                    if (y == 0)
                    {
                        if (!innerData.data.Columns.Contains(DataTableExtend.IndexCol))
                        {
                            innerData.data.Columns.Add(DataTableExtend.IndexCol, typeof(int));
                        }
                        innerData.data.Rows[x][DataTableExtend.IndexCol] = x;
                    }
                    innerData.data.Rows[x][cell.Name]= cell.InnerXml;
                    y++;
                    x++;
                }
            }
            
            tableView.DataSource = innerData.data;
            CheckReadOnly();
            tableView.Columns[DataTableExtend.IndexCol].Visible = false;
            Loading = false;
        }
       
        public void CheckViewType( int i, int j)
        {
            var col = tableView.Columns[i];
            var colInfo = tableInfo[curData.Path+col.Name];
            CheckViewType(colInfo, i, j);
        }
        private bool CheckViewType(ColInfo colInfo, int i, int j)
        {
          
            if (i < 0 || j < 0 || j >= tableView.RowCount - 1)
            {
                return false;
            }

          
            if (colInfo != null&&!string.IsNullOrWhiteSpace( tableView[i, j].Value.ToString()))
            {

            
                switch (colInfo.type)
                {
                    case ViewType.文字:
                        if (!(tableView[i, j] is TextCell))
                        {

                            tableView[i, j] = new TextCell();
                        }
                        break;
                    case ViewType.表索引:
                    case ViewType.下拉框:
                   //     MessageBox.Show("下拉框" + tableView[i, j].Value);
                        if (!(tableView[i, j] is DropDownCell))
                        {
                            if (colInfo.Contains( tableView[i, j].Value.ToString()))
                            {

                                tableView[i, j] = new DropDownCell()
                                {
                                    colInfo = colInfo,
                                    DataSource = colInfo.values
                                };
                             //   MessageBox.Show("" + tableView[i, j].Value);
                            }
                            else
                            {
                           //    MessageBox.Show("不存在" + tableView[i, j].Value);
                            }
                        }

                        break;
                    case ViewType.按钮:
                        if (!(tableView[i, j] is InnerXmlCell))
                        {

                            tableView[i, j] = new InnerXmlCell();
                        }
                        break;
                    case ViewType.脚本:
                        if(!(tableView[i, j] is ScriptCell))
                        {
                            tableView[i, j] = new ScriptCell();
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (!(tableView[i, j] is TextCell))
                {
                    tableView[i, j] = new TextCell();
                }
            }
            var flag = false;
            if (tableView.Columns[i].Name !=DataTableExtend.IndexCol)
            {
                flag = IsXml(tableView.Rows[j].Cells[i].Value.ToString());
                if (flag)
                {
                  
                    tableView[i, j] = new InnerXmlCell();
                }
                tableView[i, j].ReadOnly = flag;
              
            }
            return flag;
        }
        private void CheckReadOnly()
        {
           

            for (int i = 0; i < tableView.ColumnCount; i++)
            {
                bool isXmlCol=false;
                var col = tableView.Columns[i];
                //tableView.Columns[i].SortMode = DataGridViewColumnSortMode.Automatic;
                if (tableView.Columns[i].Name==DataTableExtend.IndexCol)
                {
                    tableView.Columns[i].ReadOnly = true;
                    tableView.Columns[i].Width = 50;
                }
                var colInfo = tableInfo[curData.Path+tableView.Columns[i].Name];
            
                for (int j = 0; j < tableView.RowCount; j++)
                {
        
                    if (tableView.Rows[j].Cells[i].Value != null)
                    {

                        if(CheckViewType(colInfo, i, j))
                        {
                            isXmlCol = true;
                        }
                    }
                }
                if (!isXmlCol)
                {
                    tableView.AutoResizeColumn(i, DataGridViewAutoSizeColumnMode.DisplayedCells);
                }
            }
        }
        private void ParseRootXml(string xmlstr)
        {
        
            xmlDoc = new XmlDocument();
            var data = new DataTable();
            xmlDoc.LoadXml(xmlstr);
            Text = xmlDoc.LastChild.Name + " - XmlViewer";
            var innerData = new InnerData(null)
            {
                data = data,
                xmlDoc=xmlDoc,
                col = -1,
                row = -1,
                xml = xmlDoc,
            };
            curData = innerData;
            ParseXml(innerData);
        }
        private void SaveXmlFile()
        {
            xmlDoc.Save(xmlPath);
            fileChangeTime = System.IO.File.GetLastWriteTimeUtc(xmlPath);
            statusLabel.Text = "文件保存成功【" + xmlPath + "】"+ fileChangeTime;
        }
        private void OpenXml(string path)
        {
            xmlPath = path;
            folderPath = path.Substring(0, xmlPath.LastIndexOf('\\'));

            fileChangeTime = System.IO.File.GetLastWriteTimeUtc(path);
            var xmlstr = FileManager.Load(path);
            if(System.IO.File.Exists(path + ".tableInfo")){
             
                tableInfo = FileManager.Deserialize<XmlTableInfo>( FileManager.Load(path + ".tableInfo"));
            }
          
            ParseRootXml(xmlstr);
        }
        private void openXmlFileDialog_FileOk(object sender, CancelEventArgs e)
        {
            if (openXmlFileDialog.CheckPathExists)
            {
                OpenXml(openXmlFileDialog.FileName);
               
            }
        }

        private void xmlFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openXmlFileDialog.ShowDialog();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isEditMode)
            {
               tableView.EndEdit();
            }
          
            if (curData.parent!=null)
            {
                var innerData = curData;
                curData = curData.parent;
                curData.data.GetRow(innerData.row)[innerData.col] = innerData.UpdateChange().InnerXml;
                tableView.DataSource = curData.data;
                CheckReadOnly();
            }
            else
            {
                curData.UpdateChange();
                SaveXmlFile();
            }
           
        }
        public static string AppPath
        {
            get
            {
                var path = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                return path.Substring(0, path.LastIndexOf('\\'));
            }
        }
        public static string TempPath
        {
            get
            {
                return AppPath + '\\' + "scirptTemp.txt";
            }
        }
        private void tableView_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
         
      

            if (e.ColumnIndex < 0 || e.RowIndex < 0||e.RowIndex>=tableView.Rows.Count-1)
            {
                return;
            }

           
            var cell = tableView[e.ColumnIndex, e.RowIndex];
            //    var rect = cell.ContentBounds;
            var info = tableInfo[cell.OwningColumn.Name];
            if (cell.ReadOnly)
            {
              
                if (info!=null&&info.type == ViewType.脚本)
                {
                    FileManager.Save(TempPath,cell.Value.ToString().GetXmlInnerString());
                    var process = Process.Start(AppPath+"\\"+ info.typeValues[0], TempPath);
                    process.WaitForExit();
                    ParseInnerXml(cell.ColumnIndex, tableView.GetRowIndex(e.RowIndex), cell.Value.ToString());
                    var text = Clipboard.GetText();
                    var count = 1;
                    foreach (var c in text)
                    {
                        if (c.Equals('\n'))
                        {
                            count++;
                        }
                    }
                    while (curData.data.Rows.Count<= count)
                    {
                        curData.data.Rows.Add();
                    }
                    foreach (DataGridViewRow row in gridView.Rows)
                    {
                        foreach (DataGridViewCell c in row.Cells)
                        {
                            if (!c.ReadOnly&&c.RowIndex!=gridView.Rows.Count-1)
                            {
                                c.Value = "";
                                c.Selected = true;
                            }
                        }
                    }
                    CellTable.Pause(text, tableView);
                    statusLabel.Text = "脚本编辑成功";
                }
                else
                {
                    ParseInnerXml(cell.ColumnIndex, tableView.GetRowIndex(e.RowIndex), cell.Value.ToString());
                }
              
                
              //  gridView.Rows[0].Cells[0].Selected = true;
                
            }
    
            if (info != null&& cell.Value!=null&&!string.IsNullOrEmpty(cell.Value.ToString()))
            {
                if(info.type== ViewType.表索引)
                {
                    var process= Process.Start(Process.GetCurrentProcess().MainModule.FileName, info.TablePath+" "+cell.Value);
                    SetForegroundWindow(FindWindow( null,process.MainWindowTitle));

                }
            }
        }


        private void tableView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
        
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isEditMode)
            {
              
                Clipboard.SetText(CellTable.Copy(tableView.SelectedCells));
                statusLabel.Text = "复制成功";
            }
           
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isEditMode)
            {
                CellTable.Pause(Clipboard.GetText(), tableView);
                statusLabel.Text = "粘贴成功";
            }
        }
        

        private void deleteRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableView.SelectedCells.Count>0)
            {
                foreach (DataGridViewCell cell in tableView.SelectedCells)
                {
                    if (tableView.Columns[cell.ColumnIndex].Name != DataTableExtend.IndexCol)
                    {
                       cell.ChangeValue("");
                    
                    }
                }
            }
        }

        private void AddColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var name = new ColumnName();
            name.Show();
        }

        private void deleteColumntoolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tableView.SelectedColumns.Count>0)
            {
                tableView.Columns.Remove(tableView.SelectedColumns[0]);
            }
        }
        bool isEditMode=false;
        private void tableView_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            isEditMode = true;
            CopyToolStripMenuItem.Enabled = false;
            pasteToolStripMenuItem.Enabled = false;
            statusLabel.Text = "编辑单元格";
        }

        private void tableView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            isEditMode = false;
            CopyToolStripMenuItem.Enabled = true;
            pasteToolStripMenuItem.Enabled = true;
            statusLabel.Text = "结束编辑单元格";
            CheckViewType(e.ColumnIndex, e.RowIndex);
        }

        private void tableView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }

        private void tableView_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.CellValue1 is int&&e.CellValue2 is int)
            {
                statusLabel.Text = "【"+e.Column.Name +"】列按数值排序";
                e.SortResult = (int)e.CellValue1 - (int)e.CellValue2;
            }
            else
            {
                statusLabel.Text = "【" + e.Column.Name + "】列按字符串排序";
                e.SortResult = System.String.Compare(
                    e.CellValue1.ToString(), e.CellValue2.ToString());
            }
            e.Handled = true;
        }
      

        private void tableView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
          
            //try
            //{
            if (Loading) return;
            //MessageBox.Show("e:" + (e.RowIndex-1));
          
            var row= tableView.RowCount-1;
            if (row <= 0) return;
            if (string.IsNullOrWhiteSpace(tableView.Rows[row - 1].Cells[DataTableExtend.IndexCol].Value.ToString())){
                tableView.Rows[row - 1].Cells[DataTableExtend.IndexCol].Value = e.RowIndex - 1;
                tableView.Rows[row - 1].Cells[DataTableExtend.IndexCol].ReadOnly = true;
            }
            //}
            //catch (Exception)
            //{

            //   // throw;
            //}


        }

        public void FindAndSelect(string str)
        {
            var cell = XmlTableEditor.mainTable.FindCell(str);
            if (cell != null)
            {
                XmlTableEditor.mainTable.gridView.ClearSelection();
                cell.Selected = true;
                XmlTableEditor.mainTable.gridView.CurrentCell = cell;
            }
            else
            {
                MessageBox.Show("未找到[" + str + "]");
            }
        }
    public DataGridViewCell FindCell(string str)
        {
            bool startFind = gridView.CurrentCell == null;
            var startCell = gridView.CurrentCell;
            foreach (DataGridViewRow col in tableView.Rows)
            {
                foreach (DataGridViewCell cell in col.Cells)
                {
                   
                    if (startFind)
                    {
                        if (cell.Value!=null&&cell.Value.ToString().Contains(str))
                        {
                            return cell;
                        }
                    }
                    else
                    {
                        if (cell == gridView.CurrentCell)
                        {
                            startFind = true;
                        }
                    }
                   
                }
            }
            if (startFind)
            {
                foreach (DataGridViewRow col in tableView.Rows)
                {
                    foreach (DataGridViewCell cell in col.Cells)
                    {

                        if (startFind)
                        {
                            if (cell == startCell)
                            {
                                return null;
                            }
                            if (cell.Value.ToString().Contains(str))
                            {
                                return cell;
                            }
                        }
                    }
                }
            }
            return null;
          
        }
        private void tableView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            
        }

        private void XmlTableEditor_Activated(object sender, EventArgs e)
        {
            if(!string.IsNullOrWhiteSpace(xmlPath))
            {
                var newTime = System.IO.File.GetLastWriteTimeUtc(xmlPath);
                if (newTime != fileChangeTime)
                {
                    OpenXml(xmlPath);
                    MessageBox.Show("文件在外部发生了更改自动读取修改内容");
                }
                
            }
           
        }
        private void XmlTableEditor_Deactivate(object sender, EventArgs e)
        {
        }
        Finder finder;
        private void 查找ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (finder == null)
            {
                finder = new Finder();
               
            }
            else if (!finder.Visible)
            {
               
                     finder.ShowDialog();
            }
         
        }

        private void tableView_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
         
        }

        private void tableView_ColumnSortModeChanged(object sender, DataGridViewColumnEventArgs e)
        {
            var col= e.Column;
            //MessageBox.Show(col.SortMode.ToString());
        }

        private void tableView_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            var tLast = lastSortCol;
            lastSortCol = tableView.Columns[e.ColumnIndex];
            tableView.Sort(tableView.Columns[e.ColumnIndex],  tableView.Columns[e.ColumnIndex]== tLast ? ListSortDirection.Descending : ListSortDirection.Ascending);
           
        }
        DataGridViewColumn lastSortCol=null;
        public void UpdateIndex()
        {
            foreach (DataGridViewRow row in gridView.Rows)
            {
                row.Cells[DataTableExtend.IndexCol].Value = row.Cells[DataTableExtend.IndexCol].RowIndex;
            }
        }
        private void tableView_Sorted(object sender, EventArgs e)
        {
         
            if (tableView.SortedColumn.Name == DataTableExtend.IndexCol) return;

            UpdateIndex();

            if (tableView.SortOrder== SortOrder.Descending)
            {
                lastSortCol = null;
            }
            tableView.Sort(tableView.Columns[DataTableExtend.IndexCol], ListSortDirection.Ascending);
        
            CheckReadOnly();
        }
    }

    public enum GridType
    {
        none,
        Grid,
        row,
        col,
        
    }
    public static class DataTableExtend
    {
        public static DataRow GetRow(this DataTable table,int viewrowIndex)
        {
            
            foreach (DataRow row in table.Rows)
            {
                if (int.Parse(row[IndexCol].ToString()) == viewrowIndex)
                {
                    return row;
                }
            }
            MessageBox.Show("错误的索引【" + viewrowIndex + "】");
            return null;
        }
        public static int GetRowIndex(this DataGridView tableView, int row)
        {
            //MessageBox.Show("索引【" + row + "】=>【" + int.Parse(tableView[IndexCol, row].Value.ToString()) + "】");
            try
            {
                return int.Parse(tableView[IndexCol, row].Value.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show("获取索引出错 ["+ tableView[IndexCol, row].Value+"]");
                throw;
            }
          
        }
        public static string IndexCol = "#index#";
    }
 
    [System.Serializable]
    public class CellData
    {
        public int row;
        public int col;
        public string value;
        public CellData()
        {

        }
        public CellData(int rowOffset,int colOffset,string value)
        {
            row = rowOffset;
            col = colOffset ;
            this.value = value;
        }
    }
    public class CellTable:List<List<DataGridViewCell>>
    {
      
        public CellTable(DataGridViewSelectedCellCollection cells)
        {
        
            foreach (DataGridViewCell cell in cells)
            {
                AddCell(cell);
            }
            Sort();
        }
        public static string Copy(DataGridViewSelectedCellCollection cells)
        {
            return new CellTable(cells).GetString();
        }

        public string GetString()
        {
            var value="";
            foreach (var list in this)
            {
                foreach (var cell in list)
                {
                    value += cell.Value+""+ '\t';
                }
                value += '\n';
            }
            return value;
        }
        public static void Pause(string copyData, DataGridView tableView)
        {
            
            var cells = tableView.SelectedCells;
            var table =new List<List<string>>();
            foreach (var listValue in copyData.Split('\n'))
            {
                var newLine = new List<string>();
                table.Add(newLine);
                foreach (var cellValue in listValue.Split('\t'))
                {
                    newLine.Add(cellValue);
                    
                }
            }
            
            var selectTable =new CellTable(tableView.SelectedCells);
            if (selectTable.Count > 0&& selectTable[0].Count>0)
            {
            
                var fristCell=selectTable[0][0];
                for (int x = 0; x < table.Count; x++)
                {
                    for (int y = 0; y < table[x].Count; y++)
                    {
                        var cell = selectTable[x, y];
                        if (cell != null&& !cell.ReadOnly)
                        {
                            selectTable[x, y].ChangeValue( table[x][y]);
                        }
                       
                    }
                }

            }

          
        }
        public void AddCell(DataGridViewCell cell)
        {
            List<DataGridViewCell> addList=null;
            foreach (var list in this)
            {
                if (list.Count > 0)
                {
                    if (list[0].RowIndex == cell.RowIndex)
                    {
                        addList = list;
                    }
                }
            }
            if (addList == null)
            {
                addList= new List<DataGridViewCell>();
                Add(addList);
            }
            addList.Add(cell);
        }
        public  DataGridViewCell this[int row,int col]
        {
            get
            {
                if (row < Count)
                {
                    if (col < base[row].Count)
                    {
                        return base[row][col];
                    }
                }
                return null;
            }
        }
        public new void Sort()
        {
           
            foreach (var list in this)
            {
                list.Sort((a, b) => a.ColumnIndex - b.ColumnIndex);
            }
            this.Sort((a, b) => a[0].RowIndex - b[0].RowIndex);
        }
    }
   
    public class InnerData
    {
        public int col;
        public int row;
        public DataRowCollection lastCell;
        public DataTable data;
        public XmlNode xml;
        public XmlDocument xmlDoc;
        public GridType gridType;
        public InnerData parent;
        public string Path
        {
             get
            {
                if (parent == null)
                {
                    
                    return string.IsNullOrEmpty(_path)?"":_path+".";
                }
                return parent.Path+_path+".";
            }
        }
        public string _path;
        public InnerData(InnerData parent)
        {
            this.parent = parent;
        }
        public XmlNode UpdateChange()
        {
            var root = xml.LastChild;
            List<XmlNode> removeList = new List<XmlNode>();
            for (int row = 0; row < data.Rows.Count; row++)
            {
                var rowNode = root.ChildNodes[row];

                if (rowNode == null)
                {
                    if (root.FirstChild != null)
                    {
                        rowNode = xmlDoc.CreateElement(root.FirstChild.Name);
                        root.AppendChild(rowNode);
                    }
                }

                if (gridType == GridType.Grid)
                {
                    for (int col = 0; col < data.Columns.Count; col++)
                    {
                        var cell = data.GetRow(XmlTableEditor.mainTable.gridView.GetRowIndex(row))[col].ToString();
                        if (!cell.IsXml())
                        {
                            cell = cell.FixXmlValue();
                        }
                        var name = data.Columns[col].ColumnName;
                        if (name == DataTableExtend.IndexCol)
                        {
                            continue;
                        }
                        var cellNode = rowNode.SelectSingleNode(name);
                        if (cellNode != null)
                        {
                            cellNode.InnerXml = cell;
                        }
                        else
                        {
                            var node = xmlDoc.CreateElement(data.Columns[col].ColumnName);
                            node.InnerXml = cell;

                            rowNode.AppendChild(node);
                        }
                        if (string.IsNullOrEmpty(cell)&&cellNode!=null)
                        {
                            rowNode.RemoveChild(cellNode);
                        }
                    }

                }
                else
                {
                    //  MessageBox.Show(gridType.ToString());

                    if (gridType == GridType.row && data.Columns.Count > 2)
                    {
                        gridType = GridType.col;
                    }
                    if (gridType == GridType.row)
                    {
                        var cell = data.Rows[row][0].ToString();
                        if (!cell.IsXml())
                        {
                            cell = cell.FixXmlValue();
                        }
                        rowNode.InnerXml = cell;
                      
                      
                    }
                    else if (gridType == GridType.col)
                    {
                    
                        for (int col = 0; col < data.Columns.Count; col++)
                        {
                            var cell = data.GetRow(XmlTableEditor.mainTable.gridView.GetRowIndex(row))[col].ToString();
                            if (!cell.IsXml())
                            {
                                cell = cell.FixXmlValue();
                            }
                            if (string.IsNullOrEmpty(cell))
                            {
                                continue;
                            }
                            var name = data.Columns[col].ColumnName;
                            if (name == DataTableExtend.IndexCol)
                            {
                                continue;
                            }
                            var cellNode = root.SelectSingleNode(name);
                            if (cellNode != null)
                            {
                                cellNode.InnerXml = cell;
                            }
                            else
                            {
                                var node = xmlDoc.CreateElement(data.Columns[col].ColumnName);
                                node.InnerXml = cell;

                                root.AppendChild(node);
                            }
                        }
                    }
                    // var info = data.Rows[row][0].ToString();
                 
                }
          
                if (string.IsNullOrWhiteSpace(rowNode.InnerText))
                {
                    removeList.Add(rowNode);
                }
            }
            foreach (var node in removeList)
            {
                root.RemoveChild(node);
            }
            return root;
           
        }
    }
}
