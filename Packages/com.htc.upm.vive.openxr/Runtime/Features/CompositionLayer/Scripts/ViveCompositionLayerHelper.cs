// Copyright HTC Corporation All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace VIVE.OpenXR.CompositionLayer
{
	public struct XrSwapchain : IEquatable<ulong>
	{
		private readonly ulong value;

		public XrSwapchain(ulong u)
		{
			value = u;
		}

		public static implicit operator ulong(XrSwapchain xrBool)
		{
			return xrBool.value;
		}
		public static implicit operator XrSwapchain(ulong u)
		{
			return new XrSwapchain(u);
		}

		public bool Equals(XrSwapchain other)
		{
			return value == other.value;
		}
		public bool Equals(ulong other)
		{
			return value == other;
		}
		public override bool Equals(object obj)
		{
			return obj is XrSwapchain && Equals((XrSwapchain)obj);
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public static bool operator ==(XrSwapchain a, XrSwapchain b) { return a.Equals(b); }
		public static bool operator !=(XrSwapchain a, XrSwapchain b) { return !a.Equals(b); }
		public static bool operator >=(XrSwapchain a, XrSwapchain b) { return a.value >= b.value; }
		public static bool operator <=(XrSwapchain a, XrSwapchain b) { return a.value <= b.value; }
		public static bool operator >(XrSwapchain a, XrSwapchain b) { return a.value > b.value; }
		public static bool operator <(XrSwapchain a, XrSwapchain b) { return a.value < b.value; }
		public static XrSwapchain operator +(XrSwapchain a, XrSwapchain b) { return a.value + b.value; }
		public static XrSwapchain operator -(XrSwapchain a, XrSwapchain b) { return a.value - b.value; }
		public static XrSwapchain operator *(XrSwapchain a, XrSwapchain b) { return a.value * b.value; }
		public static XrSwapchain operator /(XrSwapchain a, XrSwapchain b)
		{
			if (b.value == 0)
			{
				throw new DivideByZeroException();
			}
			return a.value / b.value;
		}

	}
	public struct XrCompositionLayerFlags : IEquatable<UInt64>
	{
		private readonly UInt64 value;

		public XrCompositionLayerFlags(UInt64 u)
		{
			value = u;
		}

		public static implicit operator UInt64(XrCompositionLayerFlags xrBool)
		{
			return xrBool.value;
		}
		public static implicit operator XrCompositionLayerFlags(UInt64 u)
		{
			return new XrCompositionLayerFlags(u);
		}

		public bool Equals(XrCompositionLayerFlags other)
		{
			return value == other.value;
		}
		public bool Equals(UInt64 other)
		{
			return value == other;
		}
		public override bool Equals(object obj)
		{
			return obj is XrCompositionLayerFlags && Equals((XrCompositionLayerFlags)obj);
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public static bool operator ==(XrCompositionLayerFlags a, XrCompositionLayerFlags b) { return a.Equals(b); }
		public static bool operator !=(XrCompositionLayerFlags a, XrCompositionLayerFlags b) { return !a.Equals(b); }
		public static bool operator >=(XrCompositionLayerFlags a, XrCompositionLayerFlags b) { return a.value >= b.value; }
		public static bool operator <=(XrCompositionLayerFlags a, XrCompositionLayerFlags b) { return a.value <= b.value; }
		public static bool operator >(XrCompositionLayerFlags a, XrCompositionLayerFlags b) { return a.value > b.value; }
		public static bool operator <(XrCompositionLayerFlags a, XrCompositionLayerFlags b) { return a.value < b.value; }
		public static XrCompositionLayerFlags operator +(XrCompositionLayerFlags a, XrCompositionLayerFlags b) { return a.value + b.value; }
		public static XrCompositionLayerFlags operator -(XrCompositionLayerFlags a, XrCompositionLayerFlags b) { return a.value - b.value; }
		public static XrCompositionLayerFlags operator *(XrCompositionLayerFlags a, XrCompositionLayerFlags b) { return a.value * b.value; }
		public static XrCompositionLayerFlags operator /(XrCompositionLayerFlags a, XrCompositionLayerFlags b)
		{
			if (b.value == 0)
			{
				throw new DivideByZeroException();
			}
			return a.value / b.value;
		}
	}

	public struct XrSwapchainCreateFlags : IEquatable<UInt64>
	{
		private readonly UInt64 value;

		public XrSwapchainCreateFlags(UInt64 u)
		{
			value = u;
		}

		public static implicit operator UInt64(XrSwapchainCreateFlags xrBool)
		{
			return xrBool.value;
		}
		public static implicit operator XrSwapchainCreateFlags(UInt64 u)
		{
			return new XrSwapchainCreateFlags(u);
		}

		public bool Equals(XrSwapchainCreateFlags other)
		{
			return value == other.value;
		}
		public bool Equals(UInt64 other)
		{
			return value == other;
		}
		public override bool Equals(object obj)
		{
			return obj is XrSwapchainCreateFlags && Equals((XrSwapchainCreateFlags)obj);
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public static bool operator ==(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.Equals(b); }
		public static bool operator !=(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return !a.Equals(b); }
		public static bool operator >=(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value >= b.value; }
		public static bool operator <=(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value <= b.value; }
		public static bool operator >(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value > b.value; }
		public static bool operator <(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value < b.value; }
		public static XrSwapchainCreateFlags operator +(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value + b.value; }
		public static XrSwapchainCreateFlags operator -(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value - b.value; }
		public static XrSwapchainCreateFlags operator *(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b) { return a.value * b.value; }
		public static XrSwapchainCreateFlags operator /(XrSwapchainCreateFlags a, XrSwapchainCreateFlags b)
		{
			if (b.value == 0)
			{
				throw new DivideByZeroException();
			}
			return a.value / b.value;
		}
	}

	public struct XrSwapchainUsageFlags : IEquatable<UInt64>
	{
		private readonly UInt64 value;

		public XrSwapchainUsageFlags(UInt64 u)
		{
			value = u;
		}

		public static implicit operator UInt64(XrSwapchainUsageFlags xrBool)
		{
			return xrBool.value;
		}
		public static implicit operator XrSwapchainUsageFlags(UInt64 u)
		{
			return new XrSwapchainUsageFlags(u);
		}

		public bool Equals(XrSwapchainUsageFlags other)
		{
			return value == other.value;
		}
		public bool Equals(UInt64 other)
		{
			return value == other;
		}
		public override bool Equals(object obj)
		{
			return obj is XrSwapchainUsageFlags && Equals((XrSwapchainUsageFlags)obj);
		}

		public override int GetHashCode()
		{
			return value.GetHashCode();
		}

		public override string ToString()
		{
			return value.ToString();
		}

		public static bool operator ==(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.Equals(b); }
		public static bool operator !=(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return !a.Equals(b); }
		public static bool operator >=(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value >= b.value; }
		public static bool operator <=(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value <= b.value; }
		public static bool operator >(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value > b.value; }
		public static bool operator <(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value < b.value; }
		public static XrSwapchainUsageFlags operator +(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value + b.value; }
		public static XrSwapchainUsageFlags operator -(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value - b.value; }
		public static XrSwapchainUsageFlags operator *(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b) { return a.value * b.value; }
		public static XrSwapchainUsageFlags operator /(XrSwapchainUsageFlags a, XrSwapchainUsageFlags b)
		{
			if (b.value == 0)
			{
				throw new DivideByZeroException();
			}
			return a.value / b.value;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct XrCompositionLayerQuad
	{
		public XrStructureType type;
		public IntPtr next;
		public XrCompositionLayerFlags layerFlags;
		public XrSpace space;
		public XrEyeVisibility eyeVisibility;
		public XrSwapchainSubImage subImage;
		public XrPosef pose;
		public XrExtent2Df size;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct XrCompositionLayerCylinderKHR
	{
		public XrStructureType type;
		public IntPtr next;
		public XrCompositionLayerFlags layerFlags;
		public XrSpace space;
		public XrEyeVisibility eyeVisibility;
		public XrSwapchainSubImage subImage;
		public XrPosef pose;
		public float radius;
		public float centralAngle;
		public float aspectRatio;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct XrSwapchainSubImage
	{
		public XrSwapchain swapchain;
		public XrRect2Di imageRect;
		public uint imageArrayIndex;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct XrCompositionLayerColorScaleBiasKHR
	{
		public XrStructureType type;
		public IntPtr next;
		public XrColor4f colorScale;
		public XrColor4f colorBias;
	}
	public enum GraphicsAPI
	{
		GLES3	= 1,
		Vulkan	= 2
	}
	public enum LayerType
	{
		///<summary> Overlays are composition layers rendered after the projection layer </summary>
		Overlay = 1,
		///<summary> Underlays are composition layers rendered before the projection layer </summary>
		Underlay = 2
	}

	public static class ViveCompositionLayerHelper
	{
		// Flag bits for XrCompositionLayerFlags
		public static XrCompositionLayerFlags XR_COMPOSITION_LAYER_CORRECT_CHROMATIC_ABERRATION_BIT = 0x00000001;
		public static XrCompositionLayerFlags XR_COMPOSITION_LAYER_BLEND_TEXTURE_SOURCE_ALPHA_BIT = 0x00000002;
		public static XrCompositionLayerFlags XR_COMPOSITION_LAYER_UNPREMULTIPLIED_ALPHA_BIT = 0x00000004;

		// Flag bits for XrSwapchainCreateFlags
		public static XrSwapchainCreateFlags XR_SWAPCHAIN_CREATE_PROTECTED_CONTENT_BIT = 0x00000001;
		public static XrSwapchainCreateFlags XR_SWAPCHAIN_CREATE_STATIC_IMAGE_BIT = 0x00000002;

		// Flag bits for XrSwapchainUsageFlags
		public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_COLOR_ATTACHMENT_BIT = 0x00000001;
		public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT = 0x00000002;
		public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_UNORDERED_ACCESS_BIT = 0x00000004;
		public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_TRANSFER_SRC_BIT = 0x00000008;
		public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_TRANSFER_DST_BIT = 0x00000010;
		public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_SAMPLED_BIT = 0x00000020;
		public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_MUTABLE_FORMAT_BIT = 0x00000040;
		public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_INPUT_ATTACHMENT_BIT_MND = 0x00000080;
		public static XrSwapchainUsageFlags XR_SWAPCHAIN_USAGE_INPUT_ATTACHMENT_BIT_KHR = 0x00000080;
	}
}
