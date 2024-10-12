using System.IO;
using System.Security.Cryptography;
using System;
using System.Threading.Tasks;
using UnityEngine;

public static class BasisEncryptionWrapper
{
    public static readonly int Iterations = 10000; // PBKDF2 iterations
    public const int BufferSize = 1048576; // 1MB buffer

    public static Task EncryptFileAsync(string inputFilePath, string outputFilePath, string password, byte[] sharedBuffer)
    {
        return Task.Run(async () =>
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    var keyAndIv = GenerateKeyAndIvFromPassword(password, aes.KeySize / 8, aes.BlockSize / 8);
                    aes.Key = keyAndIv.Key;
                    aes.IV = keyAndIv.IV;

                    using (FileStream fsOutput = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    using (CryptoStream cs = new CryptoStream(fsOutput, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    using (FileStream fsInput = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        int read;
                        while ((read = await fsInput.ReadAsync(sharedBuffer, 0, sharedBuffer.Length)) > 0)
                        {
                            await cs.WriteAsync(sharedBuffer, 0, read);
                        }
                    }
                }

                Debug.Log("File encrypted successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error encrypting file: " + ex.Message);
            }
        });
    }

    public static Task DecryptFileAsync(string inputFilePath, string outputFilePath, string password)
    {
        return Task.Run(async () =>
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    var keyAndIv = GenerateKeyAndIvFromPassword(password, aes.KeySize / 8, aes.BlockSize / 8);
                    aes.Key = keyAndIv.Key;
                    aes.IV = keyAndIv.IV;

                    using (FileStream fsInput = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (CryptoStream cs = new CryptoStream(fsInput, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    using (FileStream fsOutput = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] buffer = new byte[BufferSize];
                        int read;

                        while ((read = await cs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fsOutput.WriteAsync(buffer, 0, read);
                        }
                    }
                }

                Debug.Log("File decrypted successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError("Error decrypting file: " + ex.Message);
            }
        });
    }

    // Method to derive Key and IV from the password using PBKDF2
    private static (byte[] Key, byte[] IV) GenerateKeyAndIvFromPassword(string password, int keyBytes, int ivBytes)
    {
        using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, ivBytes, Iterations))
        {
            byte[] key = rfc2898DeriveBytes.GetBytes(keyBytes);
            byte[] iv = rfc2898DeriveBytes.GetBytes(ivBytes);
            return (key, iv);
        }
    }
    public static async Task<byte[]> EncryptBytesAsync(byte[] inputBytes, string password, byte[] sharedBuffer)
    {
        try
        {
            using (Aes aes = Aes.Create())
            {
                var keyAndIv = GenerateKeyAndIvFromPassword(password, aes.KeySize / 8, aes.BlockSize / 8);
                aes.Key = keyAndIv.Key;
                aes.IV = keyAndIv.IV;

                using (MemoryStream msOutput = new MemoryStream())
                using (CryptoStream cs = new CryptoStream(msOutput, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    int totalRead = 0;
                    int bytesToRead;

                    while (totalRead < inputBytes.Length)
                    {
                        bytesToRead = Math.Min(sharedBuffer.Length, inputBytes.Length - totalRead);
                        Buffer.BlockCopy(inputBytes, totalRead, sharedBuffer, 0, bytesToRead);

                        await cs.WriteAsync(sharedBuffer, 0, bytesToRead);
                        totalRead += bytesToRead;
                    }

                    await cs.FlushAsync();
                    return msOutput.ToArray(); // Return the encrypted bytes
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error encrypting bytes: " + ex.Message);
            return null;
        }
    }

    public static async Task<byte[]> DecryptBytesAsync(byte[] inputBytes, string password)
    {
        try
        {
            using (Aes aes = Aes.Create())
            {
                var keyAndIv = GenerateKeyAndIvFromPassword(password, aes.KeySize / 8, aes.BlockSize / 8);
                aes.Key = keyAndIv.Key;
                aes.IV = keyAndIv.IV;

                using (MemoryStream msInput = new MemoryStream(inputBytes))
                using (CryptoStream cs = new CryptoStream(msInput, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (MemoryStream msOutput = new MemoryStream())
                {
                    byte[] buffer = new byte[BufferSize];
                    int read;

                    while ((read = await cs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await msOutput.WriteAsync(buffer, 0, read);
                    }

                    return msOutput.ToArray(); // Return the decrypted bytes
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error decrypting bytes: " + ex.Message);
            return null;
        }
    }
}