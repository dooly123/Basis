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
    const int chunkSize = 16 * 1024 * 1024; // 16 MB
    public static async Task<byte[]> EncryptDataAsync(byte[] dataToEncrypt, string password, ProgressReport reportProgress = null)
    {
        reportProgress?.Invoke(0f);
        var encryptedData = await Encrypt(password, dataToEncrypt);
        reportProgress?.Invoke(100f);
        return encryptedData;
    }

    public static async Task<byte[]> DecryptDataAsync(byte[] dataToDecrypt, string password, ProgressReport reportProgress = null)
    {
        reportProgress?.Invoke(0f);
        var decryptedData = await Decrypt(password, dataToDecrypt);
        reportProgress?.Invoke(100f);
        return decryptedData.Item1;
    }

    private static async Task<byte[]> Encrypt(string password, byte[] dataToEncrypt)
    {
        byte[] salt = new byte[SaltSize]; // Generate a random salt
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
                    await msEncrypt.WriteAsync(salt, 0, salt.Length);
                    await msEncrypt.WriteAsync(iv, 0, iv.Length);

                    using (var cryptoStream = new CryptoStream(msEncrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        await cryptoStream.WriteAsync(dataToEncrypt, 0, dataToEncrypt.Length);
                    }

                    // Get the encrypted data from the memory stream
                    return msEncrypt.ToArray();
                }
            }
        }
    }

    public static async Task<(byte[], byte[], byte[])> Decrypt(string password, byte[] dataToDecrypt)
    {
        using (var msDecrypt = new MemoryStream(dataToDecrypt))
        {
            // Read the salt and IV from the memory stream
            byte[] salt = new byte[SaltSize];
            await msDecrypt.ReadAsync(salt, 0, SaltSize);

            byte[] iv = new byte[IvSize];
            await msDecrypt.ReadAsync(iv, 0, IvSize);

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
                            // Copy the decrypted data directly to the output memory stream
                            await cryptoStream.CopyToAsync(msOutput);

                            byte[] Output = msOutput.ToArray();
                            Debug.Log("Total was " + dataToDecrypt.Length + " without salt and iv its " + Output.Length);
                            // Return just the decrypted data as a byte array
                            return (Output, salt, iv);
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
        var buffer = new byte[chunkSize]; // Read in 4 KB chunks
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
        long totalWritten = 0; // Write in 4 KB chunks

        using (var fs = new FileStream(filePath, fileMode, FileAccess.Write))
        {
            for (int offset = 0; offset < data.Length; offset += chunkSize)
            {
                int bytesToWrite = Math.Min(chunkSize, data.Length - offset);
                await fs.WriteAsync(data, offset, bytesToWrite);
                totalWritten += bytesToWrite;
                reportProgress?.Invoke((float)totalWritten / data.Length * 100f);
            }
        }
        reportProgress?.Invoke(100f);
    }

    public static async Task EncryptFileAsync(string password, string inputFilePath, string outputFilePath, ProgressReport reportProgress)
    {
        // Read the entire data from the input file
        byte[] dataToEncrypt = await ReadAllBytesAsync(inputFilePath, reportProgress);

        // Encrypt the data
        var encryptedData = await EncryptDataAsync(dataToEncrypt, password, reportProgress);

        // Write the encrypted data to the output file
        await WriteFileAsync(outputFilePath, encryptedData, FileMode.Create, reportProgress);
    }

    public static async Task DecryptFileAsync(string password, string inputFilePath, string outputFilePath, ProgressReport reportProgress)
    {
        // Read the entire encrypted data from the input file
        byte[] dataToDecrypt = await ReadAllBytesAsync(inputFilePath, reportProgress);

        // Decrypt the data
        var decryptedData = await DecryptDataAsync(dataToDecrypt, password, reportProgress);

        // Write the decrypted data to the output file
        await WriteFileAsync(outputFilePath, decryptedData, FileMode.Create, reportProgress);
    }

    // Helper method to read all bytes from a file asynchronously
    private static async Task<byte[]> ReadAllBytesAsync(string filePath, ProgressReport reportProgress)
    {
        reportProgress?.Invoke(0f);
        var fileInfo = new FileInfo(filePath);
        var data = new byte[fileInfo.Length];

        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            int totalRead = 0;
            int bytesRead;
            while ((bytesRead = await fs.ReadAsync(data, totalRead, data.Length - totalRead)) > 0)
            {
                totalRead += bytesRead;
                reportProgress?.Invoke((float)totalRead / fileInfo.Length * 100f);
            }
        }

        reportProgress?.Invoke(100f);
        return data;
    }
}