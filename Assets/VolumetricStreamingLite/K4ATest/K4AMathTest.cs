using UnityEngine;

namespace K4ATest.Unity
{
    public class K4AMathTest : MonoBehaviour
    {
        void Start()
        {
            bool[] testResults = new bool[3];

            float[] Ax = new float[3];
            testResults[0] = K4ATest.MathTest.TestMultAx3x3(ref Ax);
            Debug.Log("Ax = (" + Ax[0] + ", " + Ax[1] + ", " + Ax[2] +")");

            float[][] At = new float[3][]{ new float[3], new float[3], new float[3]};
            testResults[1] = K4ATest.MathTest.TestTranspose3x3(ref At);
            Debug.Log("Transposed matrix");
            Debug.Log("| " + At[0][0] + ", " + At[0][1] + ", " + At[0][2] +" |");
            Debug.Log("| " + At[1][0] + ", " + At[1][1] + ", " + At[1][2] +" |");
            Debug.Log("| " + At[2][0] + ", " + At[2][1] + ", " + At[2][2] +" |");

            float[][] AB = new float[3][]{ new float[3], new float[3], new float[3]};
            testResults[2] = K4ATest.MathTest.TestMultAB3x3x3(ref AB);
            Debug.Log("A*B matrix");
            Debug.Log("| " + AB[0][0] + ", " + AB[0][1] + ", " + AB[0][2] +" |");
            Debug.Log("| " + AB[1][0] + ", " + AB[1][1] + ", " + AB[1][2] +" |");
            Debug.Log("| " + AB[2][0] + ", " + AB[2][1] + ", " + AB[2][2] +" |");

            Debug.Log("************************");
            Debug.Log("      TEST RESULTS      ");
            Debug.Log("************************");
            Debug.Log(" MultAx3x3: " + testResults[0]);
            Debug.Log(" Transpose3x3: " + testResults[1]);
            Debug.Log(" MultAB3x3x3: " + testResults[2]);
            Debug.Log("************************");
        }
    }
}
