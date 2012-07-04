﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace HexBox
{
    public class HexText : Control
    {

        private double charwidth;

        public int[] mark = {0,0};

        public Boolean removemarks;
        

        public double CharWidth
        {
            get { return charwidth; }


        }

        public HexText()
        {
            FontFamily = new FontFamily("Consolas");
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            String tempString = "";
            for (int i = 0; i < ByteContent.Count();i++ )
            {
                tempString += String.Format("{0:X2}", ByteContent[i]);
                tempString += " ";
            }

            

            //Console.WriteLine(""+tempString.Count());
            
            FormattedText formattedText = new FormattedText(
                tempString,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal),
                13,
                Brushes.Black);

            formattedText.MaxTextWidth = 340;
            formattedText.LineHeight = 20;

            formattedText.Trimming = TextTrimming.None;

            Point p = new Point();

            p = new Point(0, 0);

           //Console.WriteLine("" + formattedText.WidthIncludingTrailingWhitespace);

            int f = ByteContent.Count()*3;

            if(f>48)
            {
                f = 48;
            }


            charwidth = formattedText.WidthIncludingTrailingWhitespace/f;

            

            if (!removemarks)
            {

                if (mark[0] < mark[1])
                {

                    double y = (int) (mark[0]/32)*20;
                    double x = mark[0]%32*charwidth*3/2;
                    double z = mark[1]%32*charwidth*3/2 - x + 2*charwidth;

                    double z2 = 48*charwidth - x - charwidth;

                    if (z < 0)
                    {
                        z = 0;
                    }

                    double y1 = (int) (mark[1]/32)*20;
                    double x1 = 0;
                    double z1 = mark[1]%32*charwidth*3/2 + 2*charwidth;

                    if (z1 < 0)
                    {
                        z1 = 0;
                    }

                    if (z2 < 0)
                    {
                        z2 = 0;
                    }

                    if (mark[0]%32 > mark[1]%32 || mark[1] - mark[0] > 32)
                    {
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(x, y, z2, 20));
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(x1, y1, z1, 20));
                        int v = (int) mark[1]/32 - (int) mark[0]/32;

                        for (int i = 1; i < v; i++)
                        {
                            double y3 = y + i*20;
                            drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                         new Rect(0, y3, 47*charwidth, 20));
                        }

                    }
                    else
                    {
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(x, y, z, 20));
                    }

                }

                else
                {
                    double y = (int) (mark[0]/32)*20;
                    double x = mark[1]%32*charwidth*3/2;
                    double z = mark[0]%32*charwidth*3/2 - x + 2*charwidth;

                    double z2 = mark[0]%32*charwidth*3/2 + 2*charwidth;

                    if (z < 0)
                    {
                        z = 0;
                    }

                    double y1 = (int) (mark[1]/32)*20;
                    double x1 = 0;
                    double z1 = mark[1]%32*charwidth*3/2 + 2*charwidth;

                    if (z1 < 0)
                    {
                        z1 = 0;
                    }

                    if (z2 < 0)
                    {
                        z2 = 0;
                    }




                    if (mark[0]%32 < mark[1]%32 || mark[0] - mark[1] > 32)
                    {
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(0, y, z2, 20));
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(x1, y1, z1, 20));
                        int v = (int) mark[0]/32 - (int) mark[1]/32;

                        for (int i = 1; i < v; i++)
                        {
                            double y3 = y1 + i*20;
                            drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                         new Rect(0, y3, 47*charwidth, 20));
                        }

                    }
                    else
                    {
                        drawingContext.DrawRectangle(Brushes.LightBlue, new Pen(Brushes.LightBlue, 1.0),
                                                     new Rect(x, y, z, 20));
                    }
                }
            }
            drawingContext.DrawText(formattedText, p);

            //Console.WriteLine(this.RenderSize);
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public void appendText(String text)
        {
            Text += text;
        }

        public byte[] ByteContent
        {
            get { return (byte[])GetValue(ByteProperty); }
            set
            {
                SetValue(ByteProperty, value);
            }
        }

        static byte[] b = { };

        public static readonly DependencyProperty ByteProperty =
            DependencyProperty.Register("ByteContent",
            typeof(byte[]),
            typeof(HexText),
            new FrameworkPropertyMetadata(b, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text",
            typeof(string),
            typeof(HexText),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));
    }
}
