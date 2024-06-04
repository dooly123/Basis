# 12.2. XR_HTC_vive_xr_tracker_interaction
## Name String
    XR_HTC_vive_xr_tracker_interaction
## Revision
    1
## Overview
    This extension provides an XrPath for getting device input from VIVE XR tracker to enable its interactions. VIVE XR tracker is the tracked device which can be bound to the physical objects to make them trackable. VIVE XR tracker is a generic name means the series of various tracked devices, not just meaning one specific type of hardware. For example, VIVE XR tracker can be bound to user¡¦s hands or feet to track the motion of human body. It also can be bound to any other objects that the user wants to track and interact with.

## VIVE Wrist Tracker input
### Interaction profile path:
- /interaction_profiles/htc/vive_xr_tracker

### Valid for user paths:
- /user/xr_tracker_htc/vive_self_tracker_0
- /user/xr_tracker_htc/vive_self_tracker_1
- /user/xr_tracker_htc/vive_self_tracker_2
- /user/xr_tracker_htc/vive_self_tracker_3
- /user/xr_tracker_htc/vive_self_tracker_4

### Supported input source
- ¡K/input/entity_htc/pose

The entity_htc pose allows the applications to recognize the origin of a tracked input device, especially for the wearable devices which are not held in the user¡¦s hand. The entity_htc pose is defined as follows:

- The entity position: The center position of the tracked device.
- The entity orientation: Oriented with +Y up, +X to the right, and -Z forward.

## Vive Plugin

After adding the "VIVE XR Tracker" to "Project Settings > XR Plugin-in Management > OpenXR > Android Tab > Interaction Profiles", you can use the following Input Action Pathes.

- <ViveXRTracker>{TrackerN}/devicePose: Tracker N's pose.
- <ViveXRTracker>{TrackerN}/isTracked: Tracker N's pose is tracked.
- <ViveXRTracker>{TrackerN}/trackingState: Tracker N's tracking state.
- <ViveXRTracker>{TrackerN}/devicePosition: Tracker N's position.
- <ViveXRTracker>{TrackerN}/deviceRotation: Tracker N's rotation.

Refer to the <Vive XR OpenXR sample path>/Toolkits/Commons/ActionMap/InputActions.inputActions about the "Input Action Path" usage and the sample <Vive XR OpenXR sample path>/Toolkits/Input/OpenXRInput.unity.
