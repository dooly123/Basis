using System;

namespace BasisNetworkCore.Serializable
{
    public static class PlayerIdUshortArrayToBytes
    {
        public static byte[] Compress(ushort[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            int bitLength = 10;
            int totalBits = values.Length * bitLength;
            int byteLength = (totalBits + 7) / 8; // Round up to nearest byte
            byte[] compressed = new byte[byteLength];

            int bitPosition = 0;
            foreach (var value in values)
            {
                if (value > 1023) throw new ArgumentOutOfRangeException(nameof(values), "Value exceeds 10-bit range (0-1023).");

                for (int bit = 0; bit < bitLength; bit++)
                {
                    if ((value & (1 << bit)) != 0)
                    {
                        int byteIndex = (bitPosition + bit) / 8;
                        int bitIndex = (bitPosition + bit) % 8;
                        compressed[byteIndex] |= (byte)(1 << bitIndex);
                    }
                }
                bitPosition += bitLength;
            }

            return compressed;
        }

        public static ushort[] Decompress(byte[] compressed, int count)
        {
            if (compressed == null) throw new ArgumentNullException(nameof(compressed));

            int bitLength = 10;
            ushort[] values = new ushort[count];

            int bitPosition = 0;
            for (int i = 0; i < count; i++)
            {
                ushort value = 0;
                for (int bit = 0; bit < bitLength; bit++)
                {
                    int byteIndex = (bitPosition + bit) / 8;
                    int bitIndex = (bitPosition + bit) % 8;
                    if ((compressed[byteIndex] & (1 << bitIndex)) != 0)
                    {
                        value |= (ushort)(1 << bit);
                    }
                }
                values[i] = value;
                bitPosition += bitLength;
            }

            return values;
        }
    }
}
