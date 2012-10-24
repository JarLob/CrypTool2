﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Cryptool.PluginBase;
using DimCodeEncoder.model;
using ZXing;
using ZXing.Common;

namespace Cryptool.Plugins.DimCodeEncoder.DimCodes
{
    class EAN13 : DimCode
    {
        #region legend Strings

        private readonly LegendItem icvLegend = new LegendItem
        {
            ColorValue = Color.Blue,
            LableValue = "EAN13_ICV_LABLE",
            DiscValue = "EAN13_ICV_DISC"

        };

        private readonly LegendItem fixedLegend = new LegendItem
        {
            ColorValue = Color.Green,
            LableValue = "EAN13_FIXED_LABLE",
            DiscValue = "EAN13_FIXED_DISC"
        };


        #endregion
        
        public EAN13(DimCodeEncoder caller) : base(caller){/*empty*/}

        protected override Image GenerateBitmap(byte[] input, DimCodeEncoderSettings settings)
        {
            var barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.EAN_13,
                Options = new EncodingOptions
                {
                    Height = 100,
                    Width = 300
                }
            };

            var payload = Encoding.ASCII.GetString(input);

            if (settings.AppendICV)
                payload = payload.Substring(0, 12); // cut of last byte to let the lib calculate the ICV

            return  barcodeWriter.Write(payload);
        }

        protected override byte[] EnrichInput(byte[] input, DimCodeEncoderSettings settings)
        {
            var inp = new byte[13];
            for (int i = 0; i < 13; i++)
            {
                if (input.Length > i )
                {
                    inp[i] = (input[i] != 0) ? input[i] : Encoding.ASCII.GetBytes("0")[0];
                }
                else
                {
                    inp[i] = Encoding.ASCII.GetBytes("0")[0];
                }
            }

            return inp;
        }

        protected override bool VerifyInput(byte[] input, DimCodeEncoderSettings settings)
        {
            if (input.Any(b => b < Encoding.ASCII.GetBytes("0")[0] || b > Encoding.ASCII.GetBytes("9")[0]))
            {
                caller.GuiLogMessage("EAN13_INVALIDE_INPUT", NotificationLevel.Error);
                return false;
            }
            return true;
        }

        protected override List<LegendItem> GetLegend(byte[] input, DimCodeEncoderSettings settings)
        {
            var legend = new List<LegendItem> { fixedLegend };

            if (settings.AppendICV)
                legend.Add(icvLegend);

            return legend;
        }

        protected override Image GeneratePresentationBitmap(Image input, DimCodeEncoderSettings settings)
        {
            var bitmap = new Bitmap(input);
            var barcount = 0;
            bool isOnBlackBar = false;
            for (int x = 0; x < bitmap.Width; x++)
            {
                if (bitmap.GetPixel(x, bitmap.Height/2).R == Color.Black.R)
                {
                   if (!isOnBlackBar)
                        barcount++;
                    isOnBlackBar = true;

                    if (barcount <= 2 || barcount == 15 || barcount == 16  || barcount >= 29) 
                    {
                        bitmap = fillBarOnX(x, bitmap, fixedLegend.ColorValue);
                    }
                    else if ((barcount == 27 || barcount == 28) && settings.AppendICV)
                    {
                        bitmap = fillBarOnX(x, bitmap, icvLegend.ColorValue);
                    }
                }
                else
                {
                    isOnBlackBar = false;
                }
            }
            return bitmap;
        }

        #region helper
        private Bitmap fillBarOnX(int x, Bitmap bitmap, Color color)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                if (bitmap.GetPixel(x, y).R == Color.Black.R)
                    bitmap.SetPixel(x, y, color);
                else
                    y = bitmap.Height;
            }
            return bitmap;
        }
        #endregion
    }
}
