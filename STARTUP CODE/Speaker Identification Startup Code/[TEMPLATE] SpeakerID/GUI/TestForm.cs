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
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Recorder.GUI
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();

            comboBox1.Items.Add("Normal DTW");
            comboBox1.Items.Add("Time Sync DTW");
            comboBox1.Items.Add("Pruning DTW");
            comboBox1.Items.Add("Beam Search DTW");
            comboBox1.Items.Add("Time Sync Beam Search DTW");

        }

        private string GetTrainingListPath()
        {
            string path = @"E:\algo\TEST CASES\[2] COMPLETE\Complete SpeakerID Dataset\TrainingList.txt";
            if (!File.Exists(path))
            {
                MessageBox.Show("Training list file not found at:\n" + path);
            }
            return path;
        }

        private string GetTestingListPath()
        {
            string path = @"E:\algo\TEST CASES\[2] COMPLETE\Complete SpeakerID Dataset\TestingList.txt";
            if (!File.Exists(path))
            {
                MessageBox.Show("Testing list file not found at:\n" + path);
            }
            return path;
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
                    listBox1.Items.Clear();
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
            string selectedText = "";

            if (comboBox1.SelectedIndex != -1)
            {
                selectedText = comboBox1.SelectedItem.ToString();
            }
            else
            {
                MessageBox.Show("No Algorithm was selected");
            }
            string selectedAlgorithm = comboBox1.SelectedItem.ToString();

            if (selectedAlgorithm == "Normal DTW")
            {
                if (textBox2.Text == "Complete")
                {
                    if (!int.TryParse(textBox3.Text, out int number) || number != 1)
                    {
                        MessageBox.Show("Invalid number in TextBox3. Must be 1.");
                        return;
                    }

                    string trainingPath = GetTrainingListPath();
                    string testingPath = GetTestingListPath();

                    if (string.IsNullOrEmpty(trainingPath) || string.IsNullOrEmpty(testingPath))
                        return;

                    List<User> trainUsersList = TestcaseLoader.LoadTestcase1Training("E:\\algo\\TEST CASES\\[2] COMPLETE\\Complete SpeakerID Dataset\\TrainingList.txt");
                    List<User> testUsersList = TestcaseLoader.LoadTestcase1Testing("E:\\algo\\TEST CASES\\[2] COMPLETE\\Complete SpeakerID Dataset\\TestingList.txt");

                    string trainingFeaturesPath = FeaturesLoader.TrainingFeaturesSaver(trainUsersList, "Complete", 1);
                    List<UserStruct> testUserStructs = FeaturesLoader.testFeature(testUsersList);

                    List<UserStruct> trainUserStructs = FeaturesLoader.LoadSequencesFromBinaryFile(trainingFeaturesPath);

                    foreach (UserStruct testUser in testUserStructs)
                    {
                        foreach (Sequence testSeq in testUser.Sequences)
                        {
                            double bestMatchScore = double.PositiveInfinity;
                            string bestMatchUser = "";

                            foreach (UserStruct trainUser in trainUserStructs)
                            {
                                foreach (Sequence trainSeq in trainUser.Sequences)
                                {
                                    double score = MFCC.DynamicTimeWrapping.Match(trainSeq, testSeq);
                                    if (score < bestMatchScore)
                                    {
                                        bestMatchScore = score;
                                        bestMatchUser = trainUser.Username;
                                    }
                                }
                            }
                            Console.WriteLine($"Test sequence of {testUser.Username} best matches with {bestMatchUser} (Score: {bestMatchScore})");
                        }
                    }
                }
            }

            if (selectedText == "Time Sync DTW")
            {


            }


            if (selectedText == "Pruning DTW")
            {
                float T;
                bool isParsed = float.TryParse(textBox1.Text, out T);
                if (!isParsed)
                {
                    MessageBox.Show("Please enter T");
                    return;
                }


            }


            if (selectedText == "Beam Search DTW")
            {
                float T;
                bool isParsed = float.TryParse(textBox1.Text, out T);
                if (!isParsed)
                {
                    MessageBox.Show("Please enter a T");
                    return;
                }

            }


            if (selectedText == "Time Sync Beam Search DTW")
            {
                float T;
                bool isParsed = float.TryParse(textBox1.Text, out T);
                if (!isParsed)
                {
                    MessageBox.Show("Please enter a T");
                    return;
                }

            }
        }
    }
}