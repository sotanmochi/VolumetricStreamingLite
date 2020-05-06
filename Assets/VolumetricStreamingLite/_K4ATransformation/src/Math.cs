//
// Licensed under the MIT License.
//
// This code is a ported version of a part of transformation.c in Azure Kinect Sensor SDK (K4A).
//
// The original source code is available on GitHub.
// https://github.com/microsoft/Azure-Kinect-Sensor-SDK/blob/develop/src/math/math.c
//

namespace K4A
{
    public class Math
    {
        public static void Transpose3x3(in float[][] src, ref float[][] dst)
        {
            for (int i = 0; i < 3; ++i)
            {
                dst[i][i] = src[i][i];
                for (int j = i + 1; j < 3; ++j)
                {
                    float tmp = src[i][j];
                    dst[i][j] = src[j][i];
                    dst[j][i] = tmp;
                }
            }
        }

        public static void Negate3(in float[] src, ref float[] dst)
        {
            dst[0] = -src[0];
            dst[1] = -src[1];
            dst[2] = -src[2];
        }

        public static void Add3(in float[] a, in float[] b, ref float[] dst)
        {
            dst[0] = a[0] + b[0];
            dst[1] = a[1] + b[1];
            dst[2] = a[2] + b[2];
        }

        public static void Scale3(in float[] src, float s, ref float[] dst)
        {
            dst[0] = src[0] * s;
            dst[1] = src[1] * s;
            dst[2] = src[2] * s;
        }

        public static void AddScaled3(in float[] src, float s, ref float[] dst)
        {
            dst[0] += src[0] * s;
            dst[1] += src[1] * s;
            dst[2] += src[2] * s;
        }

        public static float MathDot3(in float[] a, in float[] b)
        {
            return a[0] * b[0] + a[1] * b[1] + a[2] * b[2];
        }

        public static void MultAx3x3(in float[][] A, in float[] x, ref float[] dst)
        {
            float y0 = MathDot3(A[0], x);
            float y1 = MathDot3(A[1], x);
            float y2 = MathDot3(A[2], x);
            dst[0] = y0;
            dst[1] = y1;
            dst[2] = y2;
        }

        public static void MultAtx3x3(in float[][] A, in float[] x, ref float[] dst)
        {
            float x0 = x[0], x1 = x[1], x2 = x[2];
            Scale3(A[0], x0, ref dst);
            AddScaled3(A[1], x1, ref dst);
            AddScaled3(A[2], x2, ref dst);
        }

        public static void MultAB3x3x3(in float[][] A, in float[][] B, ref float[][] dst)
        {
            MultAtx3x3(B, A[0], ref dst[0]);
            MultAtx3x3(B, A[1], ref dst[1]);
            MultAtx3x3(B, A[2], ref dst[2]);
        }
    }
}
