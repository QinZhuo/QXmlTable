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
             XmlTableEditor.mainTable.FindAndSelect(findInput.Text);
          
        }

        private void Finder_Load(object sender, EventArgs e)
        {

        }
    }
}
