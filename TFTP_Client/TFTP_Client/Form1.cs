using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace TFTP_Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Client client = new Client();

        }
    }
}
