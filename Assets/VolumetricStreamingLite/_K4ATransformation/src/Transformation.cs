//
// Licensed under the MIT License.
//
// This code is a ported version of a part of transformation.c in Azure Kinect Sensor SDK (K4A).
//
// The original source code is available on GitHub.
// https://github.com/microsoft/Azure-Kinect-Sensor-SDK/blob/develop/src/transformation/transformation.c
//

namespace K4A
{
    public static class Transformation
    {
        public static bool InitXyTables(Calibration calibration, CalibrationType camera, out float[] xyTables)
        {
            int width = 0;
            int height = 0;

            switch (camera)
            {
                case CalibrationType.Depth:
                    width = calibration.DepthCameraCalibration.resolutionWidth;
                    height = calibration.DepthCameraCalibration.resolutionHeight;
                    break;
                case CalibrationType.Color:
                    width = calibration.ColorCameraCalibration.resolutionWidth;
                    height = calibration.ColorCameraCalibration.resolutionHeight;
                    break;
                default:
                    xyTables = null;
                    return false;
            }

            xyTables = new float[2 * width * height];

            float[] point2d = new float[2];
            float[] point3d = new float[3];
            int valid = 0;

            for (int y = 0, idx = 0; y < height; y++)
            {
                point2d[1] = (float)y;
                for (int x = 0; x < width; x++, idx++)
                {
                    point2d[0] = (float)x;

                    bool succeeded = Transformation2dTo3d(calibration, point2d, 1.0f, camera, camera, ref point3d, ref valid);
                    if (!succeeded)
                    {
                        return false;
                    }

                    if (valid == 0)
                    {
                        xyTables[2 * idx] = 0.0f;
                        xyTables[2 * idx + 1] = 0.0f;
                    }
                    else
                    {
                        xyTables[2 * idx] = point3d[0];
                        xyTables[2 * idx + 1] = point3d[1];
                    }
                }
            }

            return true;
        }

        public static bool Transformation2dTo3d(Calibration calibration, float[] sourcePoint2d, float sourceDepth, 
                                                CalibrationType sourceCamera, CalibrationType targetCamera, ref float[] targetPoint3d, ref int valid)
        {   
            bool succeeded = false;
            
            if (sourceCamera == CalibrationType.Depth)
            {
                succeeded = IntrinsicTransformation.Unproject(calibration.DepthCameraCalibration, sourcePoint2d, sourceDepth, ref targetPoint3d, ref valid);
            }
            else if (sourceCamera == CalibrationType.Color)
            {
                succeeded = IntrinsicTransformation.Unproject(calibration.ColorCameraCalibration, sourcePoint2d, sourceDepth, ref targetPoint3d, ref valid);
            }

            if (!succeeded)
            {
                return false;
            }

            if (sourceCamera == targetCamera)
            {
                return true;
            }
            else
            {
                return Transformation3dTo3d(calibration, targetPoint3d, sourceCamera, targetCamera, ref targetPoint3d);;
            }
        }

        public static bool Transformation3dTo3d(Calibration calibration, float[] sourcePoint3d,
                                                CalibrationType sourceCamera, CalibrationType targetCamera, ref float[] targetPoint3d)
        {
            if (sourceCamera == targetCamera)
            {
                targetPoint3d[0] = sourcePoint3d[0];
                targetPoint3d[1] = sourcePoint3d[1];
                targetPoint3d[2] = sourcePoint3d[2];
                return true;
            }

            bool succeeded = ExtrinsicTransformation.ApplyExtrinsicTransformation(calibration.SourceToTargetExtrinsics, sourcePoint3d, ref targetPoint3d);
            if (!succeeded)
            {
                return false;
            }

            return true;
        }

        public static bool Transformation3dTo2d(Calibration calibration, float[] sourcePoint3d,
                                                CalibrationType sourceCamera, CalibrationType targetCamera, ref float[] targetPoint2d, ref int valid)
        {
            float[] targetPoint3d = new float[3];
            if (sourceCamera == targetCamera)
            {
                targetPoint3d[0] = sourcePoint3d[0];
                targetPoint3d[1] = sourcePoint3d[1];
                targetPoint3d[2] = sourcePoint3d[2];
            }
            else
            {
                bool succeeded = Transformation3dTo3d(calibration, sourcePoint3d, sourceCamera, targetCamera, ref targetPoint3d);
                if (!succeeded)
                {
                    return false;
                }
            }

            if (targetCamera == CalibrationType.Depth)
            {
                return IntrinsicTransformation.Project(calibration.DepthCameraCalibration, targetPoint3d, ref targetPoint2d, ref valid);
            }
            else if (targetCamera == CalibrationType.Color)
            {
                return IntrinsicTransformation.Project(calibration.ColorCameraCalibration, targetPoint3d, ref targetPoint2d, ref valid);
            }

            return true;
        }
    }
}
