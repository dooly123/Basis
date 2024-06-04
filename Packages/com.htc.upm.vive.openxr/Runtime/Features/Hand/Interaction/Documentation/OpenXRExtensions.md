# 12.67 XR_HTC_hand_interaction
## Name String
    XR_HTC_hand_interaction
## Revision
    1
## Hand Interaction Profile
### Interaction profile path:
- /interaction_profiles/htc/hand_interaction

### Valid for user paths:
- /user/hand_htc/left
- /user/hand_htc/right

### Supported input source
- ¡K/input/select/value
- ¡K/input/aim/pose

The application should use ¡K/input/aim/pose path to aim at objects in the world and use ¡K/input/select/value path to decide user selection from pinch shape strength which the range of value is 0.0f to 1.0f, with 1.0f meaning pinch fingers touched.

## VIVE Plugin

After adding the "VIVE Focus3 Hand Interaction" to "Project Settings > XR Plugin-in Management > OpenXR > Android Tab > Interaction Profiles", you can use the following Input Action Pathes.

### Left Hand
- <ViveHandInteraction>{LeftHand}/selectValue: Presents the left hand pinch strength.
- <ViveHandInteraction>{LeftHand}/pointerPose: Presents the left hand pinch pose.

### Right Hand
- <ViveHandInteraction>{RightHand}/selectValue: Presents the right hand pinch strength.
- <ViveHandInteraction>{RightHand}/pointerPose: Presents the right hand pinch pose.

Refer to the <VIVE OpenXR sample path>/Plugin/Input/ActionMap/InputActions.inputActions about the "Input Action Path" usage and the sample <VIVE OpenXR sample path>/Plugin/Input/OpenXRInput.unity.
