using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebApiTester
{
    public partial class OlDocumentsWindow : Form
    {
        public OlDocumentsWindow()
        {
            InitializeComponent();
            IntellidactIDs=null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(string.IsNullOrWhiteSpace(textBox1.Text))
                IntellidactIDs = null;

            IntellidactIDs = textBox1.Lines;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        public string[] IntellidactIDs { get; set; }
    }
}
