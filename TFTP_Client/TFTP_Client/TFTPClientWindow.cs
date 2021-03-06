﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO; 
using System.Text; 
using System.Windows.Forms;
using System.Reflection;

namespace TFTP_Client
{
    public partial class TFTPClientWindow : Form
    {
        public TFTPClientWindow()
        {
            InitializeComponent();
            Client.getInstance().setContext(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string p = string.Empty;

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                p = openFileDialog1.FileName;

            textBox1.Text = p;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

            upload();
        }

        private void upload() {

            //IF FILE NOT EXISTS LEAVE METHOD WITH AN ERROR
            if (!File.Exists(textBox1.Text)){
                MessageBox.Show("The file does not exist!");
                return;
            }

            //IF FILESIZE == 0 LEAVE METHOD WITH AN ERROR
            if ((new FileInfo(textBox1.Text).Length) == 0)
            {
                if (MessageBox.Show("The file is empty! Do you still want to continue?", "Continue", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }
            
            //IF NOT LEFT, EVERYTHING IS OK
            Client.getInstance().put(textBox1.Text, textBoxIP.Text, textBoxPort.Text); 
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            receive();
        }

        private void receive() {
            Client.getInstance().setRetrPath(textBox3.Text);
            Client.getInstance().get(textBox2.Text, textBoxIP.Text, textBoxPort.Text); 
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        public void protocolMSGInv(String msg)
        {
            Invoke(
                (MethodInvoker)delegate
                {
                    protocolMessage(msg);
                });
        }

        private void protocolMessage(String msg)
        {
            listBox1.Items.Add(msg);
        }
        
    }
}
