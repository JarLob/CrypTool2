﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Org.BouncyCastle.Math;

namespace PKCS1.Library
{
    class myFloat
    {
        static int prec = 1024;
        BigInteger x;
        int e;        

        public myFloat(myFloat z)
        {
            x = z.x;
            e = z.e;
        }

        public myFloat()
        {
            x = BigInteger.ValueOf(0);
            e = 0;
        }

        
        public myFloat(double d) //fehlerhaft
        {
            if (d != 0.0)
            {
                int l = Convert.ToInt32( Math.Log(d) / Math.Log(2.0) );
                if (l < 64)
                {
                    x = to_ZZ((d * Math.Pow(2.0, 64.0 - (double)l)));
                    e = l - 64;
                }
                else
                {
                    x = to_ZZ(d);
                    e = 0;
                }
                e += 1024;
            }
            else
            {
                x = to_ZZ(0.0);
                e = 0;
            }
            normalize();
        }

        public static void setPrec(int iPrec)
        {
            prec = iPrec;
        }

        public void normalize()
        {
            //int l = NumBits(x); // Bitlänge l von x, l(0) = 0
            int l = x.BitLength;

            if (l == 0)
                e = 0;
            else
            {
                int d = prec - l;

                if (d > 0)
                {
                    //x <<= d;
                    x = x.ShiftLeft(d);
                    e -= d;
                }
                if (d < 0)
                {
                    //x >>= -d;
                    x = x.ShiftRight(-d);
                    e -= d;
                }
            }
        }

        public static BigInteger to_ZZ(ref myFloat x)
        {
            BigInteger res = x.x;
            
            if (x.e <= prec)               
                //res <<= prec + x.e;
                res = res.ShiftRight(prec - x.e);
            else
                //res >>= prec - x.e;
                res = res.ShiftLeft(prec - x.e);

            return res;
        }

        private BigInteger to_ZZ(double x) //fehlerhaft
        {
            long bits = BitConverter.DoubleToInt64Bits(x);
            int exponent = (int)((bits >> 52) & 0x7ffL);
            long mantissa = bits & 0xfffffffffffffL;

            BigInteger res = BigInteger.ValueOf(mantissa);
            if (exponent >= 0)
                res = res.ShiftLeft(prec + exponent);
            else
                res = res.ShiftRight(prec - exponent);

            return res;
        }

        public static void to_Float(ref myFloat res, ref BigInteger x)
        {
            res.x = x;
            //res.e = 1024;
            res.e = prec;
            res.normalize();
        }

        public static void add(ref myFloat res, ref myFloat op1, ref myFloat op2)
        {
            int d = op1.e - op2.e;
            if (d >= 0)
            {
                res.x = op1.x.Add( (op2.x.ShiftRight(d) ) );
                res.e = op1.e;
            }
            else
            {
                res.x = (op1.x.ShiftRight(-d) ).Add(op2.x);
                res.e = op2.e;
            }
            res.normalize();
        }

        public void sub(ref myFloat res, ref myFloat op1, ref myFloat op2)
        {
            int d = op1.e - op2.e;
            if ( d >= 0 )
            {
                res.x = op1.x.Subtract( ( op2.x.ShiftRight( d ) ) );
                res.e = op1.e;
            }
            else
            {
                res.x = (op1.x.ShiftRight( -d ) ).Subtract( op1.x );
                res.e = op2.e;
            }
            res.normalize();
        }

        public static void mul(ref myFloat res, ref myFloat op1, ref myFloat op2 )
        {
            res.x = op1.x.Multiply( op2.x );
            res.e = op1.e + op2.e - prec;
            res.normalize();
        }

        public static void div (ref myFloat res, ref myFloat op1, ref myFloat op2 )
        {
            res.x = ( op1.x.ShiftLeft( prec ) ).Divide( op2.x );
            res.e = op1.e - op2.e;
            res.normalize();
        }
    }
}
