//
// Licensed under the MIT License.
//
// This code is a ported version of a part of k4atypes.h in Azure Kinect Sensor SDK (K4A).
//
// The original source code is available on GitHub.
// https://github.com/microsoft/Azure-Kinect-Sensor-SDK/blob/develop/include/k4a/k4atypes.h
//

namespace K4A
{
    public enum CalibrationType
    {
        Unknown = -1,
        Depth,
        Color,
        Gyro,
        Accel,
        Num,
    }

    public class Calibration
    {
        public CalibrationCamera DepthCameraCalibration = new CalibrationCamera();
        public CalibrationCamera ColorCameraCalibration = new CalibrationCamera();
        public CalibrationExtrinsics SourceToTargetExtrinsics = new CalibrationExtrinsics();
    }

    public class CalibrationCamera
    {
        public CalibrationExtrinsics extrinsics = new CalibrationExtrinsics();
        public CalibrationIntrinsics intrinsics = new CalibrationIntrinsics();
        public int resolutionWidth;
        public int resolutionHeight;
        public float metricRadius;
    }

    public class CalibrationIntrinsics
    {
        public float cx; /**< Principal point in image, x */
        public float cy; /**< Principal point in image, y */
        public float fx; /**< Focal length x */
        public float fy; /**< Focal length y */
        public float k1; /**< k1 radial distortion coefficient */
        public float k2; /**< k2 radial distortion coefficient */
        public float k3; /**< k3 radial distortion coefficient */
        public float k4; /**< k4 radial distortion coefficient */
        public float k5; /**< k5 radial distortion coefficient */
        public float k6; /**< k6 radial distortion coefficient */
        public float codx; /**< Center of distortion in Z=1 plane, x (only used for Rational6KT) */
        public float cody; /**< Center of distortion in Z=1 plane, y (only used for Rational6KT) */
        public float p2; /**< Tangential distortion coefficient 2 */
        public float p1; /**< Tangential distortion coefficient 1 */
        public float metricRadius; /**< Metric radius */
    }

    public class CalibrationExtrinsics
    {
        /**< 3x3 Rotation matrix stored in row major order */
        public float[][] rotation = new float[3][]{ new float[3], new float[3], new float[3]};

        /**< Translation vector, x,y,z (in millimeters) */
        public float[] translation = new float[3];
    }
}
