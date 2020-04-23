//
// The original source code is available on GitHub.
// https://github.com/hanseuljun/kinect-to-hololens/blob/master/unity/KinectViewer/Assets/Scripts/AzureKinectCalibration.cs
//

//
// Copyright 2019 Hanseul Jun
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

public class AzureKinectCalibration
{
    public class Intrinsics
    {
        public float cx;
        public float cy;
        public float fx;
        public float fy;
        public float k1;
        public float k2;
        public float k3;
        public float k4;
        public float k5;
        public float k6;
        public float codx;
        public float cody;
        public float p2;
        public float p1;
        public float metricRadius;
    }

    public class Extrinsics
    {
        // 3x3 rotation matrix stored in row major order.
        public float[] rotation = new float[9];
        // Translation vector (x, y, z), in millimeters.
        public float[] translation = new float[3];
    }
}
