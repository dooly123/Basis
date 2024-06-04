# Meshing Feature Sample

Demonstrates how to author an OpenXR Feature which sets up a custom XR Meshing Subsystem.

The [XR SDK Meshing Subsystem](https://docs.unity3d.com/2020.2/Documentation/Manual/xrsdk-meshing.html) allows for one to surface procedurally-generated meshes within Unity.  Within OpenXR this functionality can be exposed by using creating an `OpenXRFeature` to manage the subsystem.  This sample uses a native plugin to provide a teapot mesh through a [XRMeshingSubsystem](https://docs.unity3d.com/2020.2/Documentation/ScriptReference/XR.XRMeshSubsystem.html) that is managed by an OpenXR feature.

See the [XR SDK Meshing Subsystem](https://docs.unity3d.com/2020.2/Documentation/Manual/xrsdk-meshing.html) for more information about the meshing subsystem.

See the [Unity OpenXR Documentation](https://docs.unity3d.com/Packages/com.unity.xr.openxr@latest) for more information on developing a custom feature.
