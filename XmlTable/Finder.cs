using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XmlTable
{
    public partial class Finder : Form
    {
        public Finder()
        {
            InitializeComponent();
        }

        private void findButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(findInput.Text)) return;
            var cell= XmlTableEditor.mainTable.Find(findInput.Text);
            if (cell != null)
            {
                XmlTableEditor.mainTable.gridView.ClearSelection();
                cell.Selected = true;
                XmlTableEditor.mainTable.gridView.CurrentCell = cell;
            }
            else
            {
                MessageBox.Show("未找到[" + findInput.Text + "]");
            }
        }
    }
}
