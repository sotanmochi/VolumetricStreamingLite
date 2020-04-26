//
// Licensed under the MIT License.
//
// This code is a ported version of a part of intrinsic_transformation.c in Azure Kinect Sensor SDK (K4A).
//
// The original source code is available on GitHub.
// https://github.com/microsoft/Azure-Kinect-Sensor-SDK/blob/develop/src/transformation/intrinsic_transformation.c
//

namespace K4A
{
    public class IntrinsicTransformation
    {
        public static bool Project(in CalibrationCamera cameraCalibration, in float[] point3d, ref float[] point2d, ref int valid)
        {
            if (point3d[2] <= 0.0f)
            {
                point2d[0] = 0.0f;
                point2d[1] = 0.0f;
                valid = 0;
                return true;
            }

            float[] xy = new float[2];
            xy[0] = point3d[0] / point3d[2];
            xy[1] = point3d[1] / point3d[2];

            float[] J_xy = null;
            bool succeeded = ProjectInternal(cameraCalibration, xy, ref point2d, ref valid, ref J_xy);
            if (!succeeded)
            {
                return false;
            }

            return true;
        }

        public static bool Unproject(in CalibrationCamera cameraCalibration, in float[] point2d, in float depth, ref float[] point3d, ref int valid)
        {
            if (depth == 0.0f)
            {
                point3d[0] = 0.0f;
                point3d[1] = 0.0f;
                point3d[2] = 0.0f;
                valid = 0;
                return true;
            }

            bool succeeded = UnprojectInternal(cameraCalibration, point2d, ref point3d, ref valid);
            if (!succeeded)
            {
                return false;
            }

            point3d[0] *= depth;
            point3d[1] *= depth;
            point3d[2] = depth;

            return true;
        }

        public static bool UnprojectInternal(in CalibrationCamera cameraCalibration, in float[] uv, ref float[] xy, ref int valid)
        {
            CalibrationIntrinsics intrinsics = cameraCalibration.intrinsics;

            float cx = intrinsics.cx;
            float cy = intrinsics.cy;
            float fx = intrinsics.fx;
            float fy = intrinsics.fy;
            float k1 = intrinsics.k1;
            float k2 = intrinsics.k2;
            float k3 = intrinsics.k3;
            float k4 = intrinsics.k4;
            float k5 = intrinsics.k5;
            float k6 = intrinsics.k6;
            float codx = intrinsics.codx;
            float cody = intrinsics.cody;
            float p1 = intrinsics.p1;
            float p2 = intrinsics.p2;

            if (!(fx > 0.0f && fy > 0.0f))
            {
                return false;
            }

            // correction for radial distortion
            float xp_d = (uv[0] - cx) / fx - codx;
            float yp_d = (uv[1] - cy) / fy - cody;

            float rs = xp_d * xp_d + yp_d * yp_d;
            float rss = rs * rs;
            float rsc = rss * rs;
            float a = 1.0f + k1 * rs + k2 * rss + k3 * rsc;
            float b = 1.0f + k4 * rs + k5 * rss + k6 * rsc;
            float ai;
            if (a != 0.0f)
            {
                ai = 1.0f / a;
            }
            else
            {
                ai = 1.0f;
            }
            float di = ai * b;

            xy[0] = xp_d * di;
            xy[1] = yp_d * di;

            // approximate correction for tangential params
            float two_xy = 2.0f * xy[0] * xy[1];
            float xx = xy[0] * xy[0];
            float yy = xy[1] * xy[1];

            xy[0] -= (yy + 3.0f * xx) * p2 + two_xy * p1;
            xy[1] -= (xx + 3.0f * yy) * p1 + two_xy * p2;

            // add on center of distortion
            xy[0] += codx;
            xy[1] += cody;

            return IterativeUnproject(cameraCalibration, uv, ref xy, ref valid, 20);
        }

        public static bool IterativeUnproject(in CalibrationCamera cameraCalibration, in float[] uv, ref float[] xy, ref int valid, int maxPasses)
        {
            valid = 1;
            float[] Jinv = new float[2 * 2];
            float[] best_xy = new float[2] { 0.0f, 0.0f };
            float best_err = float.MaxValue;

            for (int pass = 0; pass < maxPasses; pass++)
            {
                float[] p = new float[2];
                float[] J = new float[2 * 2];

                if (!ProjectInternal(cameraCalibration, xy, ref p, ref valid, ref J))
                {
                    return false;
                }
                if (valid == 0)
                {
                    return true;
                }

                float err_x = uv[0] - p[0];
                float err_y = uv[1] - p[1];
                float err = err_x * err_x + err_y * err_y;
                if (err >= best_err)
                {
                    xy[0] = best_xy[0];
                    xy[1] = best_xy[1];
                    break;
                }

                best_err = err;
                best_xy[0] = xy[0];
                best_xy[1] = xy[1];
                Invert2x2(J, ref Jinv);
                if(pass + 1 == maxPasses || best_err < 1e-22f)
                {
                    break;
                }

                float dx = Jinv[0] * err_x + Jinv[1] * err_y;
                float dy = Jinv[2] * err_x + Jinv[3] * err_y;

                xy[0] += dx;
                xy[1] += dy;
            }

            if (best_err > 1e-6f)
            {
                valid = 0;
            }

            return true;
        }

        public static bool ProjectInternal(in CalibrationCamera cameraCalibration, in float[] xy, ref float[] uv, ref int valid, ref float[] J_xy)
        {
            CalibrationIntrinsics intrinsics = cameraCalibration.intrinsics;

            float cx = intrinsics.cx;
            float cy = intrinsics.cy;
            float fx = intrinsics.fx;
            float fy = intrinsics.fy;
            float k1 = intrinsics.k1;
            float k2 = intrinsics.k2;
            float k3 = intrinsics.k3;
            float k4 = intrinsics.k4;
            float k5 = intrinsics.k5;
            float k6 = intrinsics.k6;
            float codx = intrinsics.codx;
            float cody = intrinsics.cody;
            float p1 = intrinsics.p1;
            float p2 = intrinsics.p2;
            float max_radius_for_projection = cameraCalibration.metricRadius;

            if (!((fx > 0.0f && fy > 0.0f)))
            {
                return false;
            }

            valid = 1;

            float xp = xy[0] - codx;
            float yp = xy[1] - cody;

            float xp2 = xp * xp;
            float yp2 = yp * yp;
            float xyp = xp * yp;
            float rs = xp2 + yp2;
            if (rs > max_radius_for_projection * max_radius_for_projection)
            {
                valid = 0;
                return true;
            }
            float rss = rs * rs;
            float rsc = rss * rs;
            float a = 1.0f + k1 * rs + k2 * rss + k3 * rsc;
            float b = 1.0f + k4 * rs + k5 * rss + k6 * rsc;
            float bi;
            if (b != 0.0f)
            {
                bi = 1.0f / b;
            }
            else
            {
                bi = 1.0f;
            }
            float d = a * bi;

            float xp_d = xp * d;
            float yp_d = yp * d;

            float rs_2xp2 = rs + 2.0f * xp2;
            float rs_2yp2 = rs + 2.0f * yp2;

            xp_d += rs_2xp2 * p2 + 2.0f * xyp * p1;
            yp_d += rs_2yp2 * p1 + 2.0f * xyp * p2;

            float xp_d_cx = xp_d + codx;
            float yp_d_cy = yp_d + cody;

            uv[0] = xp_d_cx * fx + cx;
            uv[1] = yp_d_cy * fy + cy;

            if (J_xy == null)
            {
                return true;
            }

            // compute Jacobian matrix
            float dudrs = k1 + 2.0f * k2 * rs + 3.0f * k3 * rss;
            // compute d(b)/d(r^2)
            float dvdrs = k4 + 2.0f * k5 * rs + 3.0f * k6 * rss;
            float bis = bi * bi;
            float dddrs = (dudrs * b - a * dvdrs) * bis;

            float dddrs_2 = dddrs * 2.0f;
            float xp_dddrs_2 = xp * dddrs_2;
            float yp_xp_dddrs_2 = yp * xp_dddrs_2;

            J_xy[0] = fx * (d + xp * xp_dddrs_2 + 6.0f * xp * p2 + 2.0f * yp * p1);
            J_xy[1] = fx * (yp_xp_dddrs_2 + 2.0f * yp * p2 + 2.0f * xp * p1);
            J_xy[2] = fy * (yp_xp_dddrs_2 + 2.0f * xp * p1 + 2.0f * yp * p2);
            J_xy[3] = fy * (d + yp * yp * dddrs_2 + 6.0f * yp * p1 + 2.0f * xp * p2);

            return true;
        }

        // Size of J and Jinv should be 4.
        public static void Invert2x2(in float[] J, ref float[] Jinv)
        {
            float detJ = J[0] * J[3] - J[1] * J[2];
            float inv_detJ = 1.0f / detJ;

            Jinv[0] = inv_detJ * J[3];
            Jinv[3] = inv_detJ * J[0];
            Jinv[1] = -inv_detJ * J[1];
            Jinv[2] = -inv_detJ * J[2];
        }
    }
}
