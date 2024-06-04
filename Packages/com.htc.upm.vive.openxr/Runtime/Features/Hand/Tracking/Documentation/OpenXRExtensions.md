# 12.28 XR_EXT_hand_tracking
## Name String
    XR_EXT_hand_tracking
## Revision
    4
## New Object Types
- [XrHandTrackerEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandTrackerEXT)
## New Enum Constants
- [XR_HAND_JOINT_COUNT_EXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XR_HAND_JOINT_COUNT_EXT)
[XrObjectType](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrObjectType) enumeration is extended with:
- XR_OBJECT_TYPE_HAND_TRACKER_EXT
[XrStructureType](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrStructureType) enumeration is extended with:
- XR_TYPE_SYSTEM_HAND_TRACKING_PROPERTIES_EXT
- XR_TYPE_HAND_TRACKER_CREATE_INFO_EXT
- XR_TYPE_HAND_JOINTS_LOCATE_INFO_EXT
- XR_TYPE_HAND_JOINT_LOCATIONS_EXT
- XR_TYPE_HAND_JOINT_VELOCITIES_EXT
## New Enums
- [XrHandEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandEXT)
- [XrHandJointEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandJointEXT)
- [XrHandJointSetEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandJointSetEXT)
## New Structures
- [XrSystemHandTrackingPropertiesEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrSystemHandTrackingPropertiesEXT)
- [XrHandTrackerCreateInfoEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandTrackerCreateInfoEXT)
- [XrHandJointsLocateInfoEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandJointsLocateInfoEXT)
- [XrHandJointLocationEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandJointLocationEXT)
- [XrHandJointVelocityEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandJointVelocityEXT)
- [XrHandJointLocationsEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandJointLocationsEXT)
- [XrHandJointVelocitiesEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandJointVelocitiesEXT)
## New Functions
- [xrCreateHandTrackerEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#xrCreateHandTrackerEXT)
- [xrDestroyHandTrackerEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#xrDestroyHandTrackerEXT)
- [xrLocateHandJointsEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#xrLocateHandJointsEXT)

## VIVE Plugin

After enabling the "VIVE Focus3 Hand Tracking" from "Project Settings > XR Plugin-in Management > OpenXR > Android Tab", you can retrieve the [XrHandJointLocationEXT](https://www.khronos.org/registry/OpenXR/specs/1.0/html/xrspec.html#XrHandJointLocationEXT) by using the following code.

    using VIVE.OpenXR.Hand;

    XrHandJointLocationEXT[] HandjointLocations = new XrHandJointLocationEXT[(int)XrHandJointEXT.XR_HAND_JOINT_MAX_ENUM_EXT];
    var feature = OpenXRSettings.Instance.GetFeature<ViveHandTracking>();
    if (feature && feature.GetJointLocations(isLeft, out HandjointLocations))
    {
        // now you have the hand joint data
    }

Refer to <VIVE OpenXR sample path>/Plugin/Input/Scripts/VIVE/RenderHand.cs about the sample code.
