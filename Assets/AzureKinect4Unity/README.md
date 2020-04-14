# Azure Kinect for Unity

## Examples
SimpleVisualizer  
<img src="./Images/AzureKinect4Unity_SimpleVisualizer.png" width="60%">

## Tested environment
- Unity 2018.4.21f1
- Azure Kinect SDK 1.4.0
- Windows 10

## Third party assets
以下のアセットをプロジェクトに含む必要があります。  
You need to include the following assets in your Unity project.

- Microsoft Azure Kinect Sensor SDK (1.4.0) 

- System.Buffers.4.4.0  

- System.Memory.4.5.3  

- System.Numerics.Vectors.4.5.0  

- System.Runtime.CompilerServices.Unsafe.4.5.2  

## Initial setup for a new project
### 1. Install Azure Kinect SDK
https://github.com/microsoft/Azure-Kinect-Sensor-SDK/blob/develop/docs/usage.md

### 2. Setup a Unity project
2.1. Create a new Unity project  

2.2. Download and import NuGetForUnity (NuGetForUnity.2.0.0.unitypackage)  
https://github.com/GlitchEnzo/NuGetForUnity/releases/tag/v2.0.0

2.3. Install Microsoft.Azure.Kinect.Sensor package

<img src="./Images/AzureKinect4Unity_NuGet.png" width="60%">

2.4. Copy dll files from the SDK folder  
Copy "depthengine_2_0.dll" and "k4a.dll" from the SDK installation folder to Plugins folder.

SDK installation folder  
C:\Program Files\Azure Kinect SDK v1.4.0\sdk\windows-desktop\amd64\release\bin

Plugins folder

<img src="./Images/AzureKinect4Unity_DLL.png">

2.5. Import AzureKinect4Unity

<img src="./Images/AzureKinect4Unity_ImportPackage.png" width="60%">

## License
- MIT License
