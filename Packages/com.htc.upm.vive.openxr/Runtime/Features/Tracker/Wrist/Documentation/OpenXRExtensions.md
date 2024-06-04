# 12.70. XR_HTC_vive_wrist_tracker_interaction
## Name String
    XR_HTC_vive_wrist_tracker_interaction
## Revision
    1
## Overview
    This extension provides an XrPath for getting device input from a VIVE wrist tracker to enable its interactions. VIVE wrist tracker is a tracked device mainly worn on user¡¦s wrist for pose tracking. Besides this use case, user also can tie it to a physical object to track its object pose, e.g. tie on a gun.

## VIVE Wrist Tracker input
### Interaction profile path:
- /interaction_profiles/htc/vive_wrist_tracker

### Valid for user paths:
- /user/wrist_htc/left
- /user/wrist_htc/right

### Supported input source
- On /user/wrist_htc/left only:
  - ¡K/input/menu/click
  - ¡K/input/x/click
- On /user/wrist_htc/right only:
  - ¡K/input/system/click (may not be available for application use) 
  - ¡K/input/a/click
- ¡K/input/entity_htc/pose

The entity_htc pose allows the applications to recognize the origin of a tracked input device, especially for the wearable devices which are not held in the user¡¦s hand. The entity_htc pose is defined as follows:

- The entity position: The center position of the tracked device.
- The entity orientation: Oriented with +Y up, +X to the right, and -Z forward.

## VIVE Plugin

After adding the "VIVE Focus3 Wrist Tracker" to "Project Settings > XR Plugin-in Management > OpenXR > Android Tab > Interaction Profiles", you can use the following Input Action Pathes.

### Left Hand
- <ViveWristTracker>{LeftHand}/primaryButton: Left tracker primary button pressed state.
- <ViveWristTracker>{LeftHand}/menu: Left tracker menu button pressed state.
- <ViveWristTracker>{LeftHand}/devicePose: Left tracker pose.
- <ViveWristTracker>{LeftHand}/devicePose/isTracked: Left tracker tracking state.

### Right Hand
- <ViveWristTracker>{RightHand}/primaryButton: Right tracker primary button pressed state.
- <ViveWristTracker>{RightHand}/menu: Right tracker menu button pressed state.
- <ViveWristTracker>{RightHand}/devicePose: Right tracker pose.
- <ViveWristTracker>{RightHand}/devicePose/isTracked: Right tracker tracking state.

Refer to the <VIVE OpenXR sample path>/Plugin/Input/ActionMap/InputActions.inputActions about the "Input Action Path" usage and the sample <VIVE OpenXR sample path>/Plugin/Input/OpenXRInput.unity.
