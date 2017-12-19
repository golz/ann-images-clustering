using Accord.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ANN08
{
    class Data
    {
        private static Data instance;

        public List<String> classes;

        public List<Bitmap> images;
        public List<String> file_names;
        public List<int> class_indexes;

        private Data() {
            classes = new List<string>();
            images = new List<Bitmap>();
            file_names = new List<string>();
            class_indexes = new List<int>();
        }

        public static Data getInstance()
        {
            if (instance == null) {
                instance = new Data();
            }
            return instance;
        }

        public Bitmap preprocessing(Bitmap image) {

            image = image.Clone(new Rectangle(0, 0, image.Width, image.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            image = Grayscale.CommonAlgorithms.BT709.Apply(image);

            image = new Threshold(127).Apply(image);

            image = new HomogenityEdgeDetector().Apply(image);

            int x = image.Width, y = image.Height, width = 0, height = 0;

            for (int i = 0; i < image.Height; i++) {
                for (int j = 0; j < image.Width; j++) {
                    if (image.GetPixel(j, i).R > 127) {
                        if (x > j) x = j;
                        if (y > i) y = i;
                        if (width < j) width = j;
                        if (height < i) height = i;
                    }
                }
            }

            image = image.Clone(new Rectangle(x, y, width - x, height - y), System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            image = new ResizeBilinear(100, 100).Apply(image);

            return image;
        }
    }
}
