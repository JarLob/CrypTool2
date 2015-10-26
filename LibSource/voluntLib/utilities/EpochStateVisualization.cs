using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using voluntLib.managementLayer.localStateManagement;
using voluntLib.managementLayer.localStateManagement.states;

namespace voluntLib.utilities
{
    class EpochStateVisualization
    {
        public static Bitmap CreateImage(EpochState state)
        {
            return state.IsFinished() 
                ? CreateBlackImage() 
                : CreateImageForEpochBitmask(state.GetCopyOfBitmask());
        }

        private static Bitmap CreateBlackImage()
        {
            var bmp = new Bitmap(200, 200);
            using (var graph = Graphics.FromImage(bmp))
            {
                var imageSize = new Rectangle(0, 0, 200, 200);
                graph.FillRectangle(Brushes.Black, imageSize);
            }
            return bmp;
        }

        private static Bitmap CreateImageForEpochBitmask(BitArray bitMask)
        {
            //we wanna have a sqrt(length) x sqrt(length) image
            var dimension = (int)Math.Ceiling(Math.Sqrt(bitMask.Length));
            var bitmap = new Bitmap(dimension, dimension);

            var x = 0;
            var y = 0;
            foreach (bool bit in bitMask)
            {
                bitmap.SetPixel(x, y, bit ? Color.Black : Color.White);
                x++;
                if (x >= dimension)
                {
                    x = 0;
                    y++;
                }
            }

            while (x < dimension)
            {
                bitmap.SetPixel(x, dimension - 1, Color.Black);
                x++;
            }
            return bitmap;
        }

    }
}
