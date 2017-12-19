using Accord.Imaging.Converters;
using Accord.Neuro;
using Accord.Neuro.Learning;
using Accord.Statistics.Analysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ANN08
{
    public partial class MainForm : Form
    {
        private List<Bitmap> images;
        private List<String> file_names;

        private PrincipalComponentAnalysis pca;
        private DistanceNetwork som_network;
        private SOMLearning som_learning;

        public MainForm()
        {
            InitializeComponent();

            images = new List<Bitmap>();
            file_names = new List<string>();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            btnBrowseClustering.Enabled = false;
            btnCluster.Enabled = false;
            btnTraining.Enabled = false;
        }

        private void btnBrowseTraining_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files | *.jpg;*.png;*.gif;";
            openFileDialog1.Multiselect = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                foreach (var path in openFileDialog1.FileNames) {
                    Bitmap bitmap = new Bitmap(path);
                    imageList1.Images.Add(bitmap);
                    images.Add(bitmap);

                    String file_name = System.IO.Path.GetFileName(path);
                    listView1.Items.Add(file_name, imageList1.Images.Count - 1);
                    file_names.Add(file_name);
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbClasses.SelectedIndex == -1)
            {
                MessageBox.Show("Classes must be chosen");
            }
            else if (listView1.Items.Count == 0)
            {
                MessageBox.Show("Training images shouldn't be empty");
            }
            else
            {
                String class_name = cmbClasses.SelectedItem.ToString();

                if (Data.getInstance().classes.IndexOf(class_name) == -1) {
                    Data.getInstance().classes.Add(class_name);
                }

                for (int i = 0; i < listView1.Items.Count; i++) {
                    Data.getInstance().images.Add(images[i]);
                    Data.getInstance().file_names.Add(file_names[i]);
                    Data.getInstance().class_indexes.Add(Data.getInstance().classes.IndexOf(class_name));
                }

                images.Clear();
                file_names.Clear();
                listView1.Items.Clear();
                imageList1.Images.Clear();

                btnTraining.Enabled = true;
            }
        }
        

        private void btnReset_Click(object sender, EventArgs e)
        {
            images.Clear();
            file_names.Clear();

            listView1.Items.Clear();
            imageList1.Images.Clear();

            Data.getInstance().classes.Clear();
            Data.getInstance().images.Clear();
            Data.getInstance().file_names.Clear();
            Data.getInstance().class_indexes.Clear();

            //Kembaliin kondisinya
            btnBrowseClustering.Enabled = false;
            btnCluster.Enabled = false;
            btnTraining.Enabled = false;

            MessageBox.Show("All data has been cleared");
        }

        private void btnTraining_Click(object sender, EventArgs e)
        {
            double[][] input_data = new double[Data.getInstance().images.Count][];
            double[][] output_data = new double[Data.getInstance().images.Count][];

            int max = Data.getInstance().classes.Count - 1;
            int min = 0;

            for (int i = 0; i < Data.getInstance().images.Count; i++) {
                Bitmap image = Data.getInstance().preprocessing(Data.getInstance().images[i]);

                ImageToArray converter = new ImageToArray(0, 1);
                converter.Convert(image, out input_data[i]);

                output_data[i] = new double[1];
                output_data[i][0] = Data.getInstance().class_indexes[i];
                output_data[i][0] = 0 + (output_data[i][0] - min) * (1 - 0) / (max - min);
            }

            pca = new PrincipalComponentAnalysis();
            pca.Method = PrincipalComponentMethod.Center;
            pca.Learn(input_data);
            double[][] input_from_pca = pca.Transform(input_data);

            int a = 0;
            int output_count = 0;
            while (a < Data.getInstance().classes.Count) {
                output_count = a * a;
                a++;
            }

            som_network = new DistanceNetwork(input_from_pca[0].Count(), output_count);
            som_learning = new SOMLearning(som_network);

            int max_iteration = 10000;
            double max_error = 0.0001;

            for (int i = 0; i < max_iteration; i++) {
                double error = som_learning.RunEpoch(input_from_pca);
                if (error < max_error) break;
            }

            btnBrowseClustering.Enabled = true;
            btnTraining.Enabled = false;
        }

        private void btnBrowseClustering_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "Image Files | *.jpg;*.png;*.gif";
            openFileDialog1.Multiselect = false;
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                String path = openFileDialog1.FileName;
                Bitmap bitmap = new Bitmap(path);
                pictureBox1.Image = bitmap;

                btnCluster.Enabled = true;
            }

            listBox1.Items.Clear();
        }

        private void btnCluster_Click(object sender, EventArgs e)
        {
            Bitmap image = new Bitmap(pictureBox1.Image);
            image = Data.getInstance().preprocessing(image);
            pictureBox1.Image = image;

            double[] input = new double[100 * 100];
            ImageToArray converter = new ImageToArray(0, 1);
            converter.Convert(image, out input);

            double[] input_from_pca = pca.Transform(input);
            som_network.Compute(input_from_pca);
            double winner = som_network.GetWinner();
            MessageBox.Show("Winner : " + winner);

            for (int i = 0; i < Data.getInstance().images.Count; i++) {
                Bitmap image2 = Data.getInstance().preprocessing(Data.getInstance().images[i]);

                double[] input2 = new double[100 * 100];
                ImageToArray converter2 = new ImageToArray(0, 1);
                converter2.Convert(image2, out input2);

                double[] input_from_pca2 = pca.Transform(input2);
                som_network.Compute(input_from_pca2);
                double check_winner = som_network.GetWinner();

                if (winner == check_winner) {
                    listBox1.Items.Add(Data.getInstance().file_names[i] + " - " + check_winner);
                }
            }
        }
    }
}
