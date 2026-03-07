using heavenly_bodies_save_deencryptor_cli.Helpers;
using heavenly_bodies_save_deencryptor_cli.Structs;

namespace heavenly_bodies_save_deencryptor_cli.Models;

public class SaveDataFile
{
    /// <summary>
    /// Gets or sets the decryptor used to process encrypted save data files.
    /// </summary>
    public Es3Deencryptor Deencryptor { get; set; }

    /// <summary>
    /// Gets the initialization vector used for cryptographic operations.
    /// </summary>
    public Iv128 Iv { get; set; }
    
    /// <summary>
    /// Data of the <see cref="SaveDataFile"/>.
    /// </summary>
    public byte[] Data { get; private set; } = [];
    
    /// <summary>
    /// Stores the encryption state of the current file.
    /// </summary>
    public bool IsEncrypted { get; private set; }

    /// <summary>
    /// Initializes a new instance of the SaveDataFile class using the specified decryption method and file data.
    /// </summary>
    /// <param name="deencryptor">The Es3Deencryptor instance used to decrypt the file data. This parameter cannot be null.</param>
    /// <param name="data">A byte array containing the file data to be set. If <paramref name="isEncrypted"/> is <see langword="true"/>, the first portion of the array is used as the initialization vector.</param>
    /// <param name="isEncrypted">A value indicating whether the provided data is encrypted.</param>
    public SaveDataFile(Es3Deencryptor deencryptor, byte[] data, bool isEncrypted = false)
    {
        Deencryptor = deencryptor;
        SetFileData(data, isEncrypted);
    }

    /// <summary>
    /// Sets the file data and updates the encryption status for the file.
    /// </summary>
    /// <param name="data">A byte array containing the file data to be set. If <paramref name="isEncrypted"/> is <see langword="true"/>, the first portion of the array is used as the initialization vector.</param>
    /// <param name="isEncrypted">A value indicating whether the provided data is encrypted.
    /// If <see langword="true"/>, the method expects the initialization vector to be included at the start of <paramref name="data"/>.</param>
    public void SetFileData(byte[] data, bool isEncrypted = false)
    {
        if (isEncrypted)
        {
            Iv = new Iv128(data.AsSpan(0, Iv128.Length));
            Data = data[Iv128.Length..];
        }
        else
        {
            Iv = new Iv128();
            Data = data;
        }

        IsEncrypted = isEncrypted;
    }

    /// <summary>
    /// Retrieves the file data as a byte array, including the initialization vector if the data is encrypted.
    /// </summary>
    /// <returns>An array of bytes containing the file data. If the data is not encrypted, the array contains the raw data;
    /// otherwise, it contains the initialization vector followed by the encrypted data.</returns>
    public byte[] GetFileData()
    {
        if (!IsEncrypted) return Data;
        var dataWithIv = new byte[Iv128.Length + Data.Length];
        Iv.Value.CopyTo(dataWithIv);
        Data.CopyTo(dataWithIv, Iv128.Length);
        return dataWithIv;
    }
    
    /// <summary>
    /// Decrypts the file data and updates the encryption state to reflect the current status.
    /// </summary>
    public void DecryptFile()
    {
        if(!IsEncrypted) return;
        // Decrypt Data
        Data = Deencryptor.Decrypt(Data, Iv);
        // update encryption state
        IsEncrypted ^= true;
    }

    /// <summary>
    /// Encrypts the file data and updates the encryption state.
    /// </summary>
    public void EncryptFile()
    {
        if (IsEncrypted) return;
        // Encrypt Data
        Data = Deencryptor.Encrypt(Data, Iv);
        // update encryption state
        IsEncrypted ^= true;
    }
}