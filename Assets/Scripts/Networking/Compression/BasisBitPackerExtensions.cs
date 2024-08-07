using Assets.Scripts.Networking.NetworkedAvatar;
using DarkRift;
using Unity.Collections;
using UnityEngine;

namespace Assets.Scripts.Networking.Compression
{
public static class BasisBitPackerExtensions
{
    public static void WriteUshortFloat(DarkRiftWriter bitPacker, float value, BasisRangedUshortFloatData compressor)
    {
        bitPacker.Write(compressor.Compress(value));
    }
    public static float ReadUshortFloat(this DarkRiftReader bitPacker, BasisRangedUshortFloatData compressor)
    {
        bitPacker.Read(out ushort data);
        return compressor.Decompress(data);
    }
    public static void WriteUshortArrayFloat(DarkRiftWriter bitPacker, float[] values, BasisRangedUshortFloatData compressor, int ArrayLength = 95)
    {
        for (int Index = 0; Index < ArrayLength; Index++)
        {
            ushort Ushorts = compressor.Compress(values[Index]);
            bitPacker.Write(Ushorts);
        }
    }
    public static void ReadUshortArrayFloat(this DarkRiftReader bitPacker, BasisRangedUshortFloatData compressor, ref BasisAvatarData BasisAvatarData, int ArrayLength = 95)
    {
        // Read each ushort value from the bitPacker into the array
        for (int Index = 0; Index < ArrayLength; Index++)
        {
            bitPacker.Read(out ushort ushortdecompression);

            // Decompress the ushort array into a float array
            BasisAvatarData.floatArray[Index] = compressor.Decompress(ushortdecompression);
        }
        BasisAvatarData.Muscles.CopyFrom(BasisAvatarData.floatArray);
    }
    public static void WriteUshortVectorFloat(DarkRiftWriter bitPacker, Vector3 values, BasisRangedUshortFloatData compressor)
    {
        ushort Compressx = compressor.Compress(values.x);
        ushort Compressy = compressor.Compress(values.y);
        ushort Compressz = compressor.Compress(values.z);
        bitPacker.Write(Compressx);
        bitPacker.Write(Compressy);
        bitPacker.Write(Compressz);
    }
    public static void ReadVectorFloat(this DarkRiftReader bitPacker,out float X,out float Y,out float Z)
    {
        bitPacker.Read(out X);
        bitPacker.Read(out Y);
        bitPacker.Read(out Z);
    }
    public static void WriteVectorFloat(DarkRiftWriter bitPacker, Vector3 values)
    {
        bitPacker.Write(values.x);
        bitPacker.Write(values.y);
        bitPacker.Write(values.z);
    }
}
}