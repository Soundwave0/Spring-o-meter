using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Spring_o_meter
{
    public partial class Form2 : Form
    {
        public string port = "COM1";
        public Boolean finished = false;
        public Form2()
        {
            InitializeComponent();
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            String value = trackBar1.Value.ToString();
            label2.Text = "COM" + value;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            port = label2.Text;
            finished = true;
            this.Close();
    
        }
    }
}
