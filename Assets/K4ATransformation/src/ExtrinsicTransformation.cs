//
// Licensed under the MIT License.
//
// This code is a ported version of a part of extrinsic_transformation.c in Azure Kinect Sensor SDK (K4A).
//
// The original source code is available on GitHub.
// https://github.com/microsoft/Azure-Kinect-Sensor-SDK/blob/develop/src/transformation/extrinsic_transformation.c
//

namespace K4A
{
    public static class ExtrinsicTransformation
    {
        public static bool ApplyExtrinsicTransformation(in CalibrationExtrinsics sourceToTarget, in float[] sourcePoint3d, ref float[] targetPoint3d)
        {
            ExtrinsicsTransformPoint3(sourceToTarget, sourcePoint3d, ref targetPoint3d);
            return true;
        }

        public static void ExtrinsicsTransformPoint3(in CalibrationExtrinsics sourceToTarget, in float[] x, ref float[] y)
        {        
            float a = x[0], b = x[1], c = x[2];
            float[] R = sourceToTarget.rotation;
            float[] t = sourceToTarget.translation;

            y[0] = R[0] * a + R[1] * b + R[2] * c + t[0];
            y[1] = R[3] * a + R[4] * b + R[5] * c + t[1];
            y[2] = R[6] * a + R[7] * b + R[8] * c + t[2];
        }
    }
}
