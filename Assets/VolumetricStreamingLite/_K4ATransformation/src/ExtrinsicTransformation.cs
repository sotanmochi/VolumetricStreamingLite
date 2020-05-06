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

        public static bool GetExtrinsicTransformation(in CalibrationExtrinsics worldToSource, in CalibrationExtrinsics worldToTarget, ref CalibrationExtrinsics sourceToTarget)
        {
            CalibrationExtrinsics sourceToWorld = new CalibrationExtrinsics();
            ExtrinsicsInvert(worldToSource, ref sourceToWorld);
            ExtrinsicsMult(worldToTarget, sourceToWorld, ref sourceToTarget);
            return true;
        }

        public static void ExtrinsicsTransformPoint3(in CalibrationExtrinsics sourceToTarget, in float[] x, ref float[] y)
        {        
            float a = x[0], b = x[1], c = x[2];
            float[][] R = sourceToTarget.rotation;
            float[] t = sourceToTarget.translation;

            y[0] = R[0][0] * a + R[0][1] * b + R[0][2] * c + t[0];
            y[1] = R[1][0] * a + R[1][1] * b + R[1][2] * c + t[1];
            y[2] = R[2][0] * a + R[2][1] * b + R[2][2] * c + t[2];
        }

        public static void ExtrinsicsInvert(in CalibrationExtrinsics x, ref CalibrationExtrinsics xinv)
        {
            Math.Transpose3x3(x.rotation, ref xinv.rotation);
            Math.MultAx3x3(xinv.rotation, x.translation, ref xinv.translation);
            Math.Negate3(xinv.translation, ref xinv.translation);
        }

        public static void ExtrinsicsMult(in CalibrationExtrinsics a, in CalibrationExtrinsics b, ref CalibrationExtrinsics ab)
        {
            float[] Rt = new float[3];
            Math.MultAx3x3(a.rotation, b.translation, ref Rt);
            Math.Add3(Rt, a.translation, ref ab.translation);
            Math.MultAB3x3x3(a.rotation, b.rotation, ref ab.rotation);
        }
    }
}
