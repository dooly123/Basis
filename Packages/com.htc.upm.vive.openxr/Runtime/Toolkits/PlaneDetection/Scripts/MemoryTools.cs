using System;
using System.Runtime.InteropServices;

namespace VIVE.OpenXR
{
	public class MemoryTools
	{
		public static IntPtr ToIntPtr<T>(T[] array) where T : Enum
		{
			int size = Marshal.SizeOf(typeof(T)) * array.Length;
			IntPtr ptr = Marshal.AllocHGlobal(size);
			int[] intArray = new int[array.Length];
			for (int i = 0; i < array.Length; i++)
				intArray[i] = (int)(object)array[i];
			Marshal.Copy(intArray, 0, ptr, array.Length);
			return ptr;
		}

		// Make the same size raw buffer from input array.
		public static IntPtr MakeRawMemory<T>(T[] refArray)
		{
			int size = Marshal.SizeOf(typeof(T)) * refArray.Length;
			return Marshal.AllocHGlobal(size);
		}

		// Make the same size raw buffer from input array.
		public static void CopyFromRawMemory<T>(T[] array, IntPtr raw)
		{
			int step = Marshal.SizeOf(typeof(T));
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Marshal.PtrToStructure<T>(IntPtr.Add(raw, i * step));
			}
		}

		// Make the same size raw buffer from input array.  Make sure the raw has enough size.
		public static void CopyToRawMemory<T>(IntPtr raw, T[] array)
		{
			int step = Marshal.SizeOf(typeof(T));
			for (int i = 0; i < array.Length; i++)
			{
				Marshal.StructureToPtr<T>(array[i], IntPtr.Add(raw, i * step), false);
			}
		}

		public static void ReleaseRawMemory(IntPtr ptr)
		{
			Marshal.FreeHGlobal(ptr);
		}
	}
}