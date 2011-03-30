﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;
using System.Threading;

namespace Cryptool.Enigma
{

    class Walze : Canvas
    {
        #region Variables
        int[] umkehrlist1 = { 4, 9, 12, 25, 0, 11, 24, 23, 21, 1, 22, 5, 2, 17, 16, 20, 14, 13, 19, 18, 15, 8, 10, 7, 6, 3 };
        int[] umkehrlist2 = { 24, 17, 20, 7, 16, 18, 11, 3, 15, 23, 13, 6, 14, 10, 12, 8, 4, 1, 5, 25, 2, 22, 21, 9, 0, 19 };
        int[] umkehrlist3 = { 5, 21, 15, 9, 8, 0, 14, 24, 4, 3, 17, 25, 23, 22, 6, 2, 19, 10, 20, 16, 18, 1, 13, 12, 7, 11 };
        TextBlock[] tebo = new TextBlock[26];
        Line[,] larray = new Line[26, 3];
        List<Line> linesToAnimat = new List<Line>();
        List<Line> trashList = new List<Line>();
        public double fast = 400;
        TextBlock[] teboToAnimat = new TextBlock[2];
        public readonly int typ;
        public TextBlock iAm = new TextBlock();
        public Boolean stop = false;
        private double timecounter = 0.0;
        private int counter = 0;
        Boolean wrong;
        public int[] umkehrlist;
        #endregion

        #region Storyboard creating
        public Storyboard startanimation()
        {
            timecounter = 0.0;
            Storyboard sb = new Storyboard();

            sb.Children.Add(animateThisTebo(teboToAnimat[0], true));
            DoubleAnimation[] douret = animateThisLine(linesToAnimat[0]);
            sb.Children.Add(douret[0]);
            sb.Children.Add(douret[1]);
            DoubleAnimation[] douret1 = new DoubleAnimation[2];
            if (!wrong)
                douret1 = animateThisLine(linesToAnimat[1]);
            else
                douret1 = animateThisLineReverse(linesToAnimat[1]);
            sb.Children.Add(douret1[0]);
            sb.Children.Add(douret1[1]);
            DoubleAnimation[] douret2 = animateThisLineReverse(linesToAnimat[2]);
            sb.Children.Add(douret2[0]);
            sb.Children.Add(douret2[1]);
            sb.Children.Add(animateThisTebo(teboToAnimat[1], false));

            return sb;

        }

        public int umkehrlist0(int x, Boolean off)
        {
            resetColors();
            if (off)
            {
                tebo[x].Background = Brushes.Green;
                tebo[umkehrlist[x]].Background = Brushes.Red;
            }
            teboToAnimat[0] = tebo[x];
            teboToAnimat[1] = tebo[umkehrlist[x]];


            larray[x, 0].Stroke = Brushes.Red;
            larray[x, 1].Stroke = Brushes.Red;
            larray[x, 2].Stroke = Brushes.Red;

            larray[umkehrlist[x], 0].Stroke = Brushes.Green;
            larray[umkehrlist[x], 1].Stroke = Brushes.Green;
            larray[umkehrlist[x], 2].Stroke = Brushes.Green;


            if (umkehrlist[x] > x)
            {
                wrong = true;
                if (larray[x, 1].Parent == this)
                {
                    linesToAnimat.Add(larray[x, 0]);
                    linesToAnimat.Add(larray[x, 1]);
                    linesToAnimat.Add(larray[x, 2]);

                }

                else
                {
                    linesToAnimat.Add(larray[umkehrlist[x], 0]);
                    linesToAnimat.Add(larray[umkehrlist[x], 1]);
                    linesToAnimat.Add(larray[umkehrlist[x], 2]);
                }
            }

            else
            {
                wrong = false;
                if (larray[x, 1].Parent == this)
                {


                    linesToAnimat.Add(larray[x, 2]);
                    linesToAnimat.Add(larray[x, 1]);
                    linesToAnimat.Add(larray[x, 0]);
                }

                else
                {


                    linesToAnimat.Add(larray[umkehrlist[x], 2]);
                    linesToAnimat.Add(larray[umkehrlist[x], 1]);
                    linesToAnimat.Add(larray[umkehrlist[x], 0]);
                }
            }

            return umkehrlist[x];
        }

        private ColorAnimation animateThisTebo(TextBlock tebo, Boolean c)
        {

            ColorAnimation colorAni = new ColorAnimation();
            colorAni.From = Colors.SkyBlue;
            if (tebo.Background == Brushes.LightSeaGreen)
                colorAni.From = Colors.LightSeaGreen;
            if (c)
                colorAni.To = Colors.YellowGreen;
            else
                colorAni.To = Colors.Tomato;
            colorAni.Duration = new Duration(TimeSpan.FromMilliseconds(1000));

            colorAni.BeginTime = TimeSpan.FromMilliseconds(timecounter);
            Storyboard.SetTarget(colorAni, tebo);
            Storyboard.SetTargetProperty(colorAni, new PropertyPath("(TextBlock.Background).(SolidColorBrush.Color)"));

            timecounter += 1000;

            return colorAni;

        }

        private DoubleAnimation[] animateThisLineReverse(Line l)
        {

            //resetColors();

            Line l1 = new Line();

            Canvas.SetLeft(l, Canvas.GetLeft(l));

            Canvas.SetTop(l, Canvas.GetTop(l));

            l1.StrokeThickness = 5.0;
            l1.Stroke = Brushes.Tomato;


            l1.X1 = l.X2;
            l1.X2 = l.X2;
            DoubleAnimation mydouble1 = new DoubleAnimation();


            l1.Y1 = l.Y2;
            l1.Y2 = l.Y2;


            mydouble1.From = l.Y2;
            mydouble1.To = l.Y1;
            mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));
            mydouble1.BeginTime = TimeSpan.FromMilliseconds(timecounter);


            DoubleAnimation mydouble = new DoubleAnimation();
            mydouble.From = l.X2;
            mydouble.To = l.X1;
            mydouble.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));
            mydouble.BeginTime = TimeSpan.FromMilliseconds(timecounter);
            timecounter += 1000;

            this.Children.Add(l1);

            DoubleAnimation[] douret = new DoubleAnimation[2];
            douret[1] = mydouble1;
            douret[0] = mydouble;
            Storyboard.SetTarget(douret[0], l1);
            Storyboard.SetTarget(douret[1], l1);
            Storyboard.SetTargetProperty(douret[0], new PropertyPath("X2"));
            Storyboard.SetTargetProperty(douret[1], new PropertyPath("Y2"));

            trashList.Add(l1);

            return douret;



        }

        private DoubleAnimation[] animateThisLine(Line l)
        {

            //resetColors();

            Line l1 = new Line();

            Canvas.SetLeft(l, Canvas.GetLeft(l));

            Canvas.SetTop(l, Canvas.GetTop(l));

            l1.StrokeThickness = 5.0;
            l1.Stroke = Brushes.LawnGreen;


            l1.X1 = l.X1;
            l1.X2 = l.X1;
            DoubleAnimation mydouble1 = new DoubleAnimation();


            l1.Y1 = l.Y1;
            l1.Y2 = l.Y1;


            mydouble1.From = l.Y1;
            mydouble1.To = l.Y2;
            mydouble1.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));
            mydouble1.BeginTime = TimeSpan.FromMilliseconds(timecounter);


            DoubleAnimation mydouble = new DoubleAnimation();
            mydouble.From = l.X1;
            mydouble.To = l.X2;
            mydouble.Duration = new Duration(TimeSpan.FromMilliseconds((1000)));
            mydouble.BeginTime = TimeSpan.FromMilliseconds(timecounter);

            timecounter += 1000;
            this.Children.Add(l1);

            //l1.BeginAnimation(Line.X2Property, mydouble);
            //l1.BeginAnimation(Line.Y2Property, mydouble1);



            trashList.Add(l1);

            DoubleAnimation[] douret = new DoubleAnimation[2];
            douret[1] = mydouble1;
            douret[0] = mydouble;
            Storyboard.SetTarget(douret[0], l1);
            Storyboard.SetTarget(douret[1], l1);
            Storyboard.SetTargetProperty(douret[0], new PropertyPath("X2"));
            Storyboard.SetTargetProperty(douret[1], new PropertyPath("Y2"));



            return douret;
        }

        #endregion

        #region Reset
        public void resetColors()
        {
            foreach (Line l in trashList)
            {
                this.Children.Remove(l);
                //l.Opacity = 0.0;
            }
            trashList.Clear();
            linesToAnimat.Clear();
            for (int i = 0; i < tebo.GetLength(0); i++)
            {
                tebo[i].Background = Brushes.SkyBlue;

                if (i % 2 == 0)
                    tebo[i].Background = Brushes.LightSeaGreen;


            }
            foreach (Line l in larray)
            {
                l.Stroke = Brushes.Black;
            }
        }
        #endregion

        #region Constructor

        public Walze(int umkehr, double width, double height)
        {
            typ = umkehr;
            Rectangle myRectangle = new Rectangle();
            myRectangle.Width = 260;
            myRectangle.Height = 764;

            myRectangle.RadiusX = 15;
            myRectangle.RadiusY = 15;

            myRectangle.Fill = Brushes.LightSteelBlue;
            myRectangle.Stroke = Brushes.Silver;
            myRectangle.StrokeThickness = 30;
            this.Children.Add(myRectangle);

            switch (umkehr)
            {
                case 1: this.umkehrlist = umkehrlist1;
                    iAm.Text = "A";
                    ; break;
                case 2: this.umkehrlist = umkehrlist2; iAm.Text = "B"; break;
                case 3: this.umkehrlist = umkehrlist3; iAm.Text = "C"; break;
            }


            double x = 29.39;
            double y = 30.0;
            int ix = 0;
            double distance = 15;
            StackPanel stack = new StackPanel();
            stack.Orientation = Orientation.Vertical;
            for (int i = 0; i < 26; i++)
            {
                TextBlock t = new TextBlock();
                t.Text = "" + Convert.ToChar(i + 65);
                t.Width = 30.0;
                t.Height = 29.39;


                t.FontSize = 20;
                //Canvas.SetLeft(t, 50.0 / 2000 * width + 1);
                //Canvas.SetTop(t, 42.0 / 1000 * height * i + 60);
                t.Background = Brushes.SkyBlue;
                t.TextAlignment = TextAlignment.Center;
                if (i % 2 == 0)
                    t.Background = Brushes.LightSeaGreen;


                stack.Children.Add(t);
                tebo[i] = t;

                Line l2 = new Line();
                l2.Y1 = x / 2 + i * x;
                l2.X1 = 230;
                l2.Y2 = x / 2 + i * x;
                l2.X2 = 20 + (i - ix) * distance;

                l2.StrokeThickness = 1;

                l2.Stroke = Brushes.Black;



                Line l3 = new Line();
                l3.Y1 = x / 2 + umkehrlist[i] * x;
                l3.X1 = 20 + (i - ix) * distance;
                l3.Y2 = x / 2 + i * x;
                l3.X2 = 20 + (i - ix) * distance;

                l3.StrokeThickness = 1;

                l3.Stroke = Brushes.Black;


                Line l4 = new Line();
                l4.Y1 = x / 2 + umkehrlist[i] * x;
                l4.X1 = 230;
                l4.Y2 = x / 2 + umkehrlist[i] * x;
                l4.X2 = 20 + (i - ix) * distance;

                l4.StrokeThickness = 1;

                l4.Stroke = Brushes.Black;

                if (umkehrlist[i] > i)
                {
                    this.Children.Add(l4);
                    this.Children.Add(l2);
                    this.Children.Add(l3);

                }

                else
                {
                    ix++;
                }

                larray[i, 0] = l2;
                larray[i, 1] = l3;
                larray[i, 2] = l4;
            }
            Canvas.SetLeft(stack, 230);

            this.Children.Add(stack);
            iAm.Height = 50;
            iAm.Width = 50;
            iAm.FontSize = 30;
            iAm.TextAlignment = TextAlignment.Center;
            Canvas.SetLeft(iAm, 0);
            Canvas.SetTop(iAm, 0);
            iAm.Background = Brushes.Orange;

            iAm.Uid = "" + typ;
            this.Children.Add(iAm);
        }
        #endregion
    }
}
