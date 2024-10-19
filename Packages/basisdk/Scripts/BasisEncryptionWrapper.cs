using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System;
using System.Threading.Tasks;
using static BasisProgressReport;

public static class BasisEncryptionWrapper
{
    private const int SaltSize = 16; // Size of the salt in bytes
    private const int KeySize = 32; // Size of the key in bytes (256 bits)
    private const int IvSize = 16; // Size of the IV in bytes (128 bits)
    private const int chunkSize = 16 * 1024; // 16 KiB

    public static async Task<byte[]> EncryptDataAsync(byte[] dataToEncrypt, string password, ProgressReport reportProgress = null)
    {
        reportProgress?.Invoke(0f);
        var encryptedData = await Task.Run(() => Encrypt(password, dataToEncrypt)); // Run encryption on a separate thread
        reportProgress?.Invoke(100f);
        return encryptedData;
    }

    public static async Task<byte[]> DecryptDataAsync(byte[] dataToDecrypt, string password, ProgressReport reportProgress = null)
    {
        reportProgress?.Invoke(0f);
        var decryptedData = await Task.Run(() => Decrypt(password, dataToDecrypt)); // Run decryption on a separate thread
        reportProgress?.Invoke(100f);
        return decryptedData.Item1;
    }

    private static byte[] Encrypt(string password, byte[] dataToEncrypt)
    {
        byte[] salt = new byte[SaltSize];
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(salt); // Fill the salt with random bytes
        }

        using (var key = new Rfc2898DeriveBytes(password, salt, 10000))
        {
            var keyBytes = key.GetBytes(KeySize);
            var iv = new byte[IvSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv); // Generate a random IV
            }

            using (var aes = Aes.Create())
            {
                aes.Key = keyBytes;
                aes.IV = iv;

                using (var msEncrypt = new MemoryStream())
                {
                    // Write the salt and IV to the memory stream
                    msEncrypt.Write(salt, 0, salt.Length);
                    msEncrypt.Write(iv, 0, iv.Length);

                    using (var cryptoStream = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
                    }

                    // Get the encrypted data from the memory stream
                    return msEncrypt.ToArray();
                }
            }
        }
    }

    private static (byte[], byte[], byte[]) Decrypt(string password, byte[] dataToDecrypt)
    {
        using (var msDecrypt = new MemoryStream(dataToDecrypt))
        {
            // Read the salt and IV from the memory stream
            byte[] salt = new byte[SaltSize];
            msDecrypt.Read(salt, 0, SaltSize);

            byte[] iv = new byte[IvSize];
            msDecrypt.Read(iv, 0, IvSize);

            // Generate the key using the password and salt
            using (var key = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                var keyBytes = key.GetBytes(KeySize);

                using (var aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = iv;

                    // Use a CryptoStream to decrypt the remaining data
                    using (var cryptoStream = new CryptoStream(msDecrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (var msOutput = new MemoryStream())
                        {
                            cryptoStream.CopyTo(msOutput);
                            byte[] output = msOutput.ToArray();
                            Debug.Log($"Total was {dataToDecrypt.Length}, without salt and IV it's {output.Length}");

                            return (output, salt, iv);
                        }
                    }
                }
            }
        }
    }

    public static async Task ReadFileAsync(string filePath, Func<byte[], Task> processChunk, ProgressReport reportProgress = null)
    {
        reportProgress?.Invoke(0f);
        var fileSize = new FileInfo(filePath).Length;
        var buffer = new byte[chunkSize];
        long totalRead = 0;

        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            int bytesRead;
            while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                totalRead += bytesRead;
                await processChunk(buffer[..bytesRead]);
                reportProgress?.Invoke((float)totalRead / fileSize * 100f);
            }
        }
        reportProgress?.Invoke(100f);
    }

    public static async Task WriteFileAsync(string filePath, byte[] data, FileMode fileMode, ProgressReport reportProgress = null)
    {
        reportProgress?.Invoke(0f);

        const int bufferSize = 4194304; // 4 MB buffer
        long totalWritten = 0;

        using (var fs = new FileStream(filePath, fileMode, FileAccess.Write, FileShare.None, bufferSize, useAsync: true))
        {
            int offset = 0;
            while (offset < data.Length)
            {
                int bytesToWrite = Math.Min(bufferSize, data.Length - offset);
                await fs.WriteAsync(data, offset, bytesToWrite);
                totalWritten += bytesToWrite;
                offset += bytesToWrite;

                // Report progress periodically
                reportProgress?.Invoke((float)totalWritten / data.Length * 100f);
            }
        }

        // Report 100% completion
        reportProgress?.Invoke(100f);
    }
    public static async Task EncryptFileAsync(string password, string inputFilePath, string outputFilePath, ProgressReport reportProgress)
    {
        byte[] dataToEncrypt = await Task.Run(() => ReadAllBytesAsync(inputFilePath, reportProgress));
        var encryptedData = await EncryptDataAsync(dataToEncrypt, password, reportProgress);
        await WriteFileAsync(outputFilePath, encryptedData, FileMode.Create, reportProgress);
    }

    public static async Task DecryptFileAsync(string password, string inputFilePath, string outputFilePath, ProgressReport reportProgress)
    {
        byte[] dataToDecrypt = await Task.Run(() => ReadAllBytesAsync(inputFilePath, reportProgress));
        if (dataToDecrypt == null || dataToDecrypt.Length == 0)
        {
            new Exception("Data Requsted was null or empty");
        }
        var decryptedData = await DecryptDataAsync(dataToDecrypt, password, reportProgress);
        await WriteFileAsync(outputFilePath, decryptedData, FileMode.Create, reportProgress);
    }

    public static async Task<byte[]> DecryptFileAsync(string password, string inputFilePath, ProgressReport reportProgress)
    {
        byte[] dataToDecrypt = await Task.Run(() => ReadAllBytesAsync(inputFilePath, reportProgress));
        if(dataToDecrypt == null || dataToDecrypt.Length == 0)
        {
            Debug.LogError("Data Requsted was null or empty");
            return null;
        } 
        var decryptedData = await DecryptDataAsync(dataToDecrypt, password, reportProgress);
        return decryptedData;
    }

    private static async Task<byte[]> ReadAllBytesAsync(string filePath, ProgressReport reportProgress)
    {
        reportProgress?.Invoke(0f);

        const int bufferSize = 81920; // Standard buffer size (80 KB)
        var fileInfo = new FileInfo(filePath);
        byte[] data = new byte[fileInfo.Length];

        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, true))
        {
            int totalRead = 0;
            int bytesRead;
            byte[] buffer = new byte[bufferSize];

            while ((bytesRead = await fs.ReadAsync(buffer, 0, Math.Min(bufferSize, data.Length - totalRead))) > 0)
            {
                Buffer.BlockCopy(buffer, 0, data, totalRead, bytesRead);
                totalRead += bytesRead;
                reportProgress?.Invoke((float)totalRead / fileInfo.Length * 100f);
            }
        }

        reportProgress?.Invoke(100f);
        return data;
    }
}