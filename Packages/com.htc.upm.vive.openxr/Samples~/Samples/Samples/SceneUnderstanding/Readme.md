# VIVE OpenXR SceneUnderstanding Unity Feature

Demonstrate configuring, calculating and generating mesh of surrounding environments by the OpenXR SceneUnderstanding extension [XR_MSFT_scene_understanding](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_MSFT_scene_understanding).

## Load sample code
**Window** > **Package Manager** > **VIVE OpenXR Plugin - Windows** > **Samples** > Click to import **SceneUnderstanding Example**.
## Play the sample scene    
1. **Edit** > **Project Settings** > **XR Plug-in Management** > Select **OpenXR** , click Exclamation mark next to it > choose **Fix All**.
2. **Edit** > **Project Settings** > **XR Plug-in Management** > **OpenXR** > Add Interaction profiles for your device.
3. **Edit** > **Project Settings** > **XR Plug-in Management** > **OpenXR** > Select **Scene UnderStanding**  and **Meshing Subsystem** under **VIVE OpenXR** Feature Groups.
4. In the Unity Project window, select the sample scene file in **Assets** > **Samples** > **VIVE OpenXR Plugin - Windows** > **1.0.13** > **SceneUnderstanding Example** > **Meshing Subsystem Feature** > **MeshingFeature.unity** then click Play.

## How to use VIVE OpenXR SceneUnderstanding Unity Feature
For the available OpenXR SceneUnderstanding functions, please refer to **SceneUnderstanding.cs**.
1. Refer to **MeshingTeapotFeature.cs** which is modified from **Meshing Subsystem Feature** sample code provided by **OpenXR Plugin** for supplying a mesh from native code with OpenXR SceneUnderstanding functions.
2. Refer to **meshing_provider.cpp** under Assets\MeshingFeaturePlugin\Native~\ for generating mesh part.
3. Refer to **MeshingBehaviour.cs** for drawing mesh part.