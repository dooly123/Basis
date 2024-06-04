// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Runtime.InteropServices;

namespace VIVE.OpenXR
{
    /// <summary>
    /// When developers would like to retrieve the supported user paths from the interactionProfile, userPath should be <see cref="OpenXRHelper.XR_NULL_PATH">XR_NULL_PATH</see>. If the interaction profile for any of the suggested bindings does not exist in the allowlist defined in Interaction Profile Paths, the runtime must return <see cref="XrResult.XR_ERROR_PATH_UNSUPPORTED">XR_ERROR_PATH_UNSUPPORTED</see>.
    /// 
    /// If developers would like to retrieve the input/output paths related to the interactionProfile, userPath should be a valid user path.If userPath is not one of the device input subpaths described in section /user paths, the runtime must return <see cref="XrResult.XR_ERROR_PATH_UNSUPPORTED">XR_ERROR_PATH_UNSUPPORTED</see>.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct XrPathsForInteractionProfileEnumerateInfoHTC
    {
        /// <summary>
        /// The <see cref="XrStructureType">XrStructureType</see> of this structure.
        /// </summary>
        public XrStructureType type;
        /// <summary>
        /// NULL or a pointer to the next structure in a structure chain.
        /// </summary>
        public IntPtr next;
        /// <summary>
        /// The <see cref="XrPath">XrPath</see> of an interaction profile.
        /// </summary>
        public XrPath interactionProfile;
        /// <summary>
        /// The top level user path the application would like to retrieve the interaction profile for. Set <see cref="OpenXRHelper.XR_NULL_PATH">XR_NULL_PATH</see> is used to enumerate all user paths from the input interactionProfile. Set as a valid user path is used to enumerate all input/output source paths.
        /// </summary>
        public XrPath userPath;
    }

    public static class VivePathEnumerationHelper
	{
        /// <summary>
        /// Provided by XR_HTC_path_enumerate.
        /// Developers should call this API with the value of pathCapacityInput equals to 0 to retrieve the size of paths from pathCountOutput. Then developers allocate the array of <see cref="XrPath">XrPath</see> data and assign the pathCapacityInput and call the API in the second time.
        /// 
        /// If the input pathCapacityInput is not sufficient to contain all output indices, the runtime must return <see cref="XrResult.XR_ERROR_SIZE_INSUFFICIENT">XR_ERROR_SIZE_INSUFFICIENT</see> on calls to <see cref="xrEnumeratePathsForInteractionProfileHTC">xrEnumeratePathsForInteractionProfileHTC</see> and not change the content in paths.
        /// </summary>
        /// <param name="instance">An <see cref="XrInstance">XrInstance</see> previously created.</param>
        /// <param name="enumerateInfo">A <see cref="XrPathsForInteractionProfileEnumerateInfoHTC">XrPathsForInteractionProfileEnumerateInfoHTC</see> providing the query information.</param>
        /// <param name="pathCapacityInput">The capacity of the paths array, or 0 to indicate a request to retrieve the required capacity.</param>
        /// <param name="pathCountOutput">A pointer to the count of paths written, or a pointer to the required capacity in the case that pathCapacityInput is insufficient.</param>
        /// <param name="paths">A pointer to an array of <see cref="XrPath">XrPath</see>, but can be NULL if pathCapacityInput is 0.</param>
        /// <returns>XR_SUCCESS for success.</returns>
        public delegate XrResult xrEnumeratePathsForInteractionProfileHTCDelegate(
            XrInstance instance,
            ref XrPathsForInteractionProfileEnumerateInfoHTC enumerateInfo,
            UInt32 pathCapacityInput,
            ref UInt32 pathCountOutput,
            [In, Out] XrPath[] paths);
    }
}
