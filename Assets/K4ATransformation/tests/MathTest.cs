
namespace K4ATest
{
    public class MathTest
    {
        public static bool TestMultAx3x3(ref float[] Ax)
        {
            float[][] A = new float[3][]{ new float[3], new float[3], new float[3]};
            float[] x = new float[3];
            float[] y = new float[3];

            A[0][0] = 1.0f; A[0][1] = 2.0f; A[0][2] = 3.0f;
            A[1][0] = 3.0f; A[1][1] = 1.0f; A[1][2] = 2.0f;
            A[2][0] = 2.0f; A[2][1] = 3.0f; A[2][2] = 1.0f;

            x[0] = 1.0f;
            x[1] = 2.0f;
            x[2] = 3.0f;

            // y = A*x
            y[0] = 14.0f;
            y[1] = 11.0f;
            y[2] = 11.0f;

            K4A.Math.MultAx3x3(A, x, ref Ax);

            if (Ax[0].Equals(y[0]) && Ax[1].Equals(y[1]) && Ax[2].Equals(y[2]))
            {
                return true;
            }

            return false;
        }

        public static bool TestTranspose3x3(ref float[][] At)
        {
            float[][] A = new float[3][]{ new float[3], new float[3], new float[3]};
            float[][] B = new float[3][]{ new float[3], new float[3], new float[3]};

            A[0][0] = 1.0f; A[0][1] = 2.0f; A[0][2] = 3.0f;
            A[1][0] = 4.0f; A[1][1] = 5.0f; A[1][2] = 6.0f;
            A[2][0] = 7.0f; A[2][1] = 8.0f; A[2][2] = 9.0f;

            // B is the transposed matrix of A
            B[0][0] = 1.0f; B[0][1] = 4.0f; B[0][2] = 7.0f;
            B[1][0] = 2.0f; B[1][1] = 5.0f; B[1][2] = 8.0f;
            B[2][0] = 3.0f; B[2][1] = 6.0f; B[2][2] = 9.0f;

            K4A.Math.Transpose3x3(A, ref At);

            if (At[0][0].Equals(B[0][0]) && At[0][1].Equals(B[0][1]) && At[0][2].Equals(B[0][2])
             && At[1][0].Equals(B[1][0]) && At[1][1].Equals(B[1][1]) && At[1][2].Equals(B[1][2])
             && At[2][0].Equals(B[2][0]) && At[2][1].Equals(B[2][1]) && At[2][2].Equals(B[2][2]))
            {
                return true;
            }

            return false;
        }

        public static bool TestMultAB3x3x3(ref float[][] AB)
        {
            float[][] A = new float[3][]{ new float[3], new float[3], new float[3]};
            float[][] B = new float[3][]{ new float[3], new float[3], new float[3]};
            float[][] C = new float[3][]{ new float[3], new float[3], new float[3]};

            A[0][0] = 1.0f; A[0][1] = 2.0f; A[0][2] = 3.0f;
            A[1][0] = 4.0f; A[1][1] = 5.0f; A[1][2] = 6.0f;
            A[2][0] = 7.0f; A[2][1] = 8.0f; A[2][2] = 9.0f;

            B[0][0] = 9.0f; B[0][1] = 8.0f; B[0][2] = 7.0f;
            B[1][0] = 6.0f; B[1][1] = 5.0f; B[1][2] = 4.0f;
            B[2][0] = 3.0f; B[2][1] = 2.0f; B[2][2] = 1.0f;

            // C = A * B
            C[0][0] = 30.0f;  C[0][1] = 24.0f;  C[0][2] = 18.0f;
            C[1][0] = 84.0f;  C[1][1] = 69.0f;  C[1][2] = 54.0f;
            C[2][0] = 138.0f; C[2][1] = 114.0f; C[2][2] = 90.0f;

            K4A.Math.MultAB3x3x3(A, B, ref AB);

            if (AB[0][0].Equals(C[0][0]) && AB[0][1].Equals(C[0][1]) && AB[0][2].Equals(C[0][2])
             && AB[1][0].Equals(C[1][0]) && AB[1][1].Equals(C[1][1]) && AB[1][2].Equals(C[1][2])
             && AB[2][0].Equals(C[2][0]) && AB[2][1].Equals(C[2][1]) && AB[2][2].Equals(C[2][2]))
            {
                return true;
            }

            return false;
        }
    }
}
