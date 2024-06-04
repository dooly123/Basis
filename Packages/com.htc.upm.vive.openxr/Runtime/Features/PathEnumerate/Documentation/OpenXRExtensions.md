# 12.1. XR_HTC_path_enumeration
## Name String
    XR_HTC_path_enumeration
## Revision
    1
## Overview
    The XR devices may offer the diversity and versatility in the practical use cases. For instance, some tracking devices can be bound to the hand-held objects and the application can locate the object via the offset between them. Another instance is to bind the tracking devices to the body parts, e.g. wrist, knee, and etc., to track the movement of body. Such XR devices cannot define all user paths beforehand due to they may be used by the multiple XR devices with one type or multiple types of hardware simultaneously.

    This extension allows the application dynamically obtaining the user paths and input/output source paths associated with an interaction profile of XR device that the runtime has supported. When this extension is enabled, the application can get supported user paths for the interaction profile if setting user path XR_NULL_PATH. And if the application inputs the valid user path as well, the output paths will be the supported input/output source paths.
