# 12.7. XR_KHR_composition_layer_cylinder
## Name String
    XR_KHR_composition_layer_cylinder
## Revision
    4
## New Object Types
## New Enum Constants
[XrStructureType](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrStructureType) enumeration is extended with:
- XR_TYPE_COMPOSITION_LAYER_CYLINDER_KHR
## New Enums
## New Structures
- [XrCompositionLayerCylinderKHR ](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrCompositionLayerCylinderKHR)
## New Functions

## VIVE Plugin

Enable "VIVE XR Composition Layer" in "Project Settings > XR Plugin-in Management > OpenXR > Android Tab > OpenXR Feature Groups" in order to use the Composition Layer feature provided by the VIVE plugin.

The plugin provides scripts and resources for setting up Composition Layers quickly and easily.
There are two scripts which can be attached to **GameObjects**:
- CompositionLayer: For setting up a basic Composition Layer. Located at: *\<VIVE OpenXR path\>/Runtime/CompositionLayer/Scripts/CompositionLayer.cs*
- CompositionLayerUICanvas: For setting up a Composition Layer which renders a Unity UI Canvas. Located at: *\<VIVE OpenXR path\>/Runtime/CompositionLayer/Scripts/CompositionLayerUICanvas.cs*

To see how the two scripts above can be used, refer to the samples located at: *\<VIVE OpenXR sample path\>/CompositionLayer/Scenes*