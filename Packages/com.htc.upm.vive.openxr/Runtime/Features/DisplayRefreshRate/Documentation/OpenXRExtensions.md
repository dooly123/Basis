# 12.54. XR_FB_display_refresh_rate
## Name String
    XR_FB_display_refresh_rate
## Revision
1
## Overview

On platforms which support dynamically adjusting the display refresh rate, application developers may request a specific display refresh rate in order to improve the overall user experience, examples include:

A video application may choose a display refresh rate which better matches the video content playback rate in order to achieve smoother video frames.

An application which can support a higher frame rate may choose to render at the higher rate to improve the overall perceptual quality, for example, lower latency and less flicker.

This extension allows:

An application to identify what display refresh rates the session supports and the current display refresh rate.

An application to request a display refresh rate to indicate its preference to the runtime.

An application to receive notification of changes to the display refresh rate which are delivered via events.

In order to enable the functionality of this extension, the application must pass the name of the extension into xrCreateInstance via the XrInstanceCreateInfo::enabledExtensionNames parameter as indicated in the Extensions section.

New Object Types

New Flag Types

New Enum Constants

XrStructureType enumeration is extended with:

XR_TYPE_EVENT_DATA_DISPLAY_REFRESH_RATE_CHANGED_FB

XrResult enumeration is extended with:

XR_ERROR_DISPLAY_REFRESH_RATE_UNSUPPORTED_FB

New Enums

New Structures

Receiving the XrEventDataDisplayRefreshRateChangedFB event structure indicates that the display refresh rate has changed.

The XrEventDataDisplayRefreshRateChangedFB structure is defined as:

// Provided by XR_FB_display_refresh_rate
typedef struct XrEventDataDisplayRefreshRateChangedFB {
    XrStructureType    type;
    const void*        next;
    float              fromDisplayRefreshRate;
    float              toDisplayRefreshRate;
} XrEventDataDisplayRefreshRateChangedFB;
Member Descriptions
type is the XrStructureType of this structure.

next is NULL or a pointer to the next structure in a structure chain. No such structures are defined in core OpenXR or this extension.

fromDisplayRefreshRate is the previous display refresh rate.

toDisplayRefreshRate is the new display refresh rate.

Valid Usage (Implicit)
The XR_FB_display_refresh_rate extension must be enabled prior to using XrEventDataDisplayRefreshRateChangedFB

type must be XR_TYPE_EVENT_DATA_DISPLAY_REFRESH_RATE_CHANGED_FB

next must be NULL or a valid pointer to the next structure in a structure chain

New Functions

The xrEnumerateDisplayRefreshRatesFB function is defined as:

// Provided by XR_FB_display_refresh_rate
XrResult xrEnumerateDisplayRefreshRatesFB(
    XrSession                                   session,
    uint32_t                                    displayRefreshRateCapacityInput,
    uint32_t*                                   displayRefreshRateCountOutput,
    float*                                      displayRefreshRates);
Parameter Descriptions
session is the session that enumerates the supported display refresh rates.

displayRefreshRateCapacityInput is the capacity of the displayRefreshRates, or 0 to retrieve the required capacity.

displayRefreshRateCountOutput is a pointer to the count of float displayRefreshRates written, or a pointer to the required capacity in the case that displayRefreshRateCapacityInput is insufficient.

displayRefreshRates is a pointer to an array of float display refresh rates, but can be NULL if displayRefreshRateCapacityInput is 0.

See Buffer Size Parameters chapter for a detailed description of retrieving the required displayRefreshRates size.

xrEnumerateDisplayRefreshRatesFB enumerates the display refresh rates supported by the current session. Display refresh rates must be in order from lowest to highest supported display refresh rates. Runtimes must always return identical buffer contents from this enumeration for the lifetime of the session.

Valid Usage (Implicit)
The XR_FB_display_refresh_rate extension must be enabled prior to calling xrEnumerateDisplayRefreshRatesFB

session must be a valid XrSession handle

displayRefreshRateCountOutput must be a pointer to a uint32_t value

If displayRefreshRateCapacityInput is not 0, displayRefreshRates must be a pointer to an array of displayRefreshRateCapacityInput float values

Return Codes
Success
XR_SUCCESS

XR_SESSION_LOSS_PENDING

Failure
XR_ERROR_FUNCTION_UNSUPPORTED

XR_ERROR_VALIDATION_FAILURE

XR_ERROR_RUNTIME_FAILURE

XR_ERROR_HANDLE_INVALID

XR_ERROR_INSTANCE_LOST

XR_ERROR_SESSION_LOST

XR_ERROR_SIZE_INSUFFICIENT

The xrGetDisplayRefreshRateFB function is defined as:

// Provided by XR_FB_display_refresh_rate
XrResult xrGetDisplayRefreshRateFB(
    XrSession                                   session,
    float*                                      displayRefreshRate);
Parameter Descriptions
session is the XrSession to query.

displayRefreshRate is a pointer to a float into which the current display refresh rate will be placed.

xrGetDisplayRefreshRateFB retrieves the current display refresh rate.

Valid Usage (Implicit)
The XR_FB_display_refresh_rate extension must be enabled prior to calling xrGetDisplayRefreshRateFB

session must be a valid XrSession handle

displayRefreshRate must be a pointer to a float value

Return Codes
Success
XR_SUCCESS

XR_SESSION_LOSS_PENDING

Failure
XR_ERROR_FUNCTION_UNSUPPORTED

XR_ERROR_VALIDATION_FAILURE

XR_ERROR_RUNTIME_FAILURE

XR_ERROR_HANDLE_INVALID

XR_ERROR_INSTANCE_LOST

XR_ERROR_SESSION_LOST

The xrRequestDisplayRefreshRateFB function is defined as:

// Provided by XR_FB_display_refresh_rate
XrResult xrRequestDisplayRefreshRateFB(
    XrSession                                   session,
    float                                       displayRefreshRate);
Parameter Descriptions
session is a valid XrSession handle.

displayRefreshRate is 0.0f or a supported display refresh rate. Supported display refresh rates are indicated by xrEnumerateDisplayRefreshRatesFB.

xrRequestDisplayRefreshRateFB provides a mechanism for an application to request the system to dynamically change the display refresh rate to the application preferred value. The runtime must return XR_ERROR_DISPLAY_REFRESH_RATE_UNSUPPORTED_FB if displayRefreshRate is not either 0.0f or one of the values enumerated by xrEnumerateDisplayRefreshRatesFB. A display refresh rate of 0.0f indicates the application has no preference.

Note that this is only a request and does not guarantee the system will switch to the requested display refresh rate.

Valid Usage (Implicit)
The XR_FB_display_refresh_rate extension must be enabled prior to calling xrRequestDisplayRefreshRateFB

session must be a valid XrSession handle

Return Codes
Success
XR_SUCCESS

XR_SESSION_LOSS_PENDING

Failure
XR_ERROR_FUNCTION_UNSUPPORTED

XR_ERROR_VALIDATION_FAILURE

XR_ERROR_RUNTIME_FAILURE

XR_ERROR_HANDLE_INVALID

XR_ERROR_INSTANCE_LOST

XR_ERROR_SESSION_LOST

XR_ERROR_FEATURE_UNSUPPORTED

XR_ERROR_DISPLAY_REFRESH_RATE_UNSUPPORTED_FB

Issues

Changing the display refresh rate from its system default does not come without trade-offs. Increasing the display refresh rate puts more load on the entire system and can lead to thermal degradation. Conversely, lowering the display refresh rate can provide better thermal sustainability but at the cost of more perceptual issues, like higher latency and flickering.
