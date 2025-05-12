using Recorder.MFCC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Recorder.GUI
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }
        
        string path;
        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select a File";
                openFileDialog.Filter = "All Files (*.*)|*.*";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    path = openFileDialog.FileName;
                    listBox1.Items.Clear(); // Clear previous items
                    listBox1.Items.Add(Path.GetFileName(path));
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm();
            mainForm.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {

            string text = textBox1.Text;
            string T = Regex.Match(text, @"\d+").Value;

        }
    }
}
