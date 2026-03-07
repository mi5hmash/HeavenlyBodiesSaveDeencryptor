using System.Security.Cryptography;
using heavenly_bodies_save_deencryptor_cli.Structs;

namespace heavenly_bodies_save_deencryptor_cli.Helpers;

/// <summary>
/// Provides methods for encrypting and decrypting data using AES-128 encryption with a specified initialization vector.
/// </summary>
public class Es3Deencryptor(string password = "ooh-heaven-is-a-place-on-earth", bool compressionEnabled = true)
{
    private const byte KeyDerivationIterations = 0x64;

    /// <summary>
    /// Creates and configures a new instance of the Aes class for symmetric encryption using Cipher Block Chaining
    /// (CBC) mode and PKCS7 padding.
    /// </summary>
    /// <returns>A new Aes instance configured to use CBC mode and PKCS7 padding.</returns>
    private static Aes GetAes()
    {
        var aes = Aes.Create();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        return aes;
    }

    /// <summary>
    /// Derives a 128-bit cryptographic key from the specified initialization vector using the PBKDF2 algorithm.
    /// </summary>
    /// <param name="iv">The initialization vector to use as the salt for key derivation. This value must not be null.</param>
    /// <returns>A byte array containing the derived 128-bit key.</returns>
    private byte[] DeriveKey128(Iv128 iv)
        => Rfc2898DeriveBytes.Pbkdf2(password, iv.Value.ToArray(), KeyDerivationIterations, HashAlgorithmName.SHA1, 16);

    /// <summary>
    /// Decrypts the specified input data using AES-128 decryption with the provided initialization vector and returns
    /// the decompressed result.
    /// </summary>
    /// <param name="inputData">The encrypted data to decrypt, represented as a read-only span of bytes.</param>
    /// <param name="iv">The 128-bit initialization vector to use for the decryption process.</param>
    /// <returns>A byte array containing the decrypted and decompressed data. The array is empty if decryption fails or if the
    /// decrypted data is empty after decompression.</returns>
    /// <exception cref="Exception">Thrown if the decrypted data is empty after the decryption process.</exception>
    public byte[] Decrypt(ReadOnlySpan<byte> inputData, Iv128 iv)
    {
        // Initialize AES
        using var aes = GetAes();
        aes.IV = iv.Value.ToArray();
        aes.Key = aes.Key = DeriveKey128(iv);

        // Decrypt data
        using MemoryStream msi = new(inputData.ToArray());
        using var decryptor = aes.CreateDecryptor();
        using CryptoStream cs = new(msi, decryptor, CryptoStreamMode.Read);
        using MemoryStream mso = new();
        cs.CopyTo(mso);

        // Validate decryption result and return decompressed data if not empty
        var decryptedData = mso.ToArray();
        if (decryptedData.Length == 0) throw new Exception("Decrypted data should not be empty");
        if (decryptedData.IsGzip()) decryptedData = decryptedData.GzipDecompress();
        return decryptedData;
    }

    /// <summary>
    /// Encrypts the specified data using AES encryption with a 128-bit initialization vector after compressing the data with Gzip.
    /// </summary>
    /// <param name="inputData">The data to encrypt, provided as a read-only span of bytes. Must not be empty.</param>
    /// <param name="iv">The 128-bit initialization vector to use for AES encryption. The value must be exactly 16 bytes in length.</param>
    /// <returns>A byte array containing the encrypted representation of the compressed input data.</returns>
    public byte[] Encrypt(ReadOnlySpan<byte> inputData, Iv128 iv)
    {
        // Initialize AES
        using var aes = GetAes();
        aes.IV = iv.Value.ToArray();
        aes.Key = DeriveKey128(iv);

        var data = inputData.ToArray();

        // Compress data
        if (compressionEnabled) data = data.GzipCompress();

        // Encrypt data
        using MemoryStream ms = new();
        using var encryptor = aes.CreateEncryptor();
        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write, true);
        cs.Write(data, 0, data.Length);
        cs.FlushFinalBlock();
        
        return ms.ToArray();
    }
}