using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Samples.SqlServer.SSIS.SpatialComponents
{
    class TransformationMatrix2D
    {
        private double [,] m = new double[3,3];

        public struct Point
        {
            public double X;
            public double Y;
        }

        public TransformationMatrix2D()
        {
            this.m = InitialMatrix();
        }

        public Point Transform(Point p)
        {
            double[] vector = new double[3];
            vector[0] = p.X;
            vector[1] = p.Y;
            vector[2] = 1.0;

            double[] resVector = Multiply(this.m, vector);

            Point resPoint;
            resPoint.X = resVector[0];
            resPoint.Y = resVector[1];

            return resPoint;
        }

        public void Rotate(double angle)
        {
            double[,] rotM = RotationMatrix(angle);

            m = Multiply(rotM, m);
        }

        public void Translate(double X, double Y)
        {
            double[,] tM = TranslationMatrix(X, Y);

            m = Multiply(tM, m);
        }

        public void Scale(double f)
        {
            double[,] tM = ScalingMatrix(f);

            m = Multiply(tM, m);
        }

        private static double[,] InitialMatrix()
        {
            double[,] m = new double[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (i == j)
                    {
                        m[i, j] = 1.0;
                    }
                    else
                    {
                        m[i, j] = 0.0;
                    }
                }
            }

            return m;
        }

        static private double[,] RotationMatrix(double angle)
        {
            double [,] m = InitialMatrix();

            double theta = angle * System.Math.PI / 180.0;

            m[0, 0] = System.Math.Cos(theta);
            m[0, 1] = -System.Math.Sin(theta);
            m[1, 0] = System.Math.Sin(theta);
            m[1, 1] = System.Math.Cos(theta);

            return m;
        }

        static private double[,] TranslationMatrix(double x, double y)
        {
            double[,] m = InitialMatrix();

            m[0, 2] = x;
            m[1, 2] = y;

            return m;
        }

        static private double[,] ScalingMatrix(double f)
        {
            double[,] m = InitialMatrix();

            m[0, 0] = f;
            m[1, 1] = f;

            return m;
        }

        static private double[] Multiply(double[,] a, double[] v)
        {
            double [] res = new double[3];

            for (int i = 0; i < 3; i++)
            {
                res[i] = a[i, 0] * v[0] + a[i, 1] * v[1] + a[i, 2] * v[2];
            }

            return res;
        }

        static private double[,] Multiply(double[,] a, double[,] b)
        {
            double [,] res = new double[3,3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    res[i, j] = a[i, 0] * b[0, j] + a[i, 1] * b[1, j] + a[i, 2] * b[2, j];
                }
            }

            return res;
        }
    }
}
