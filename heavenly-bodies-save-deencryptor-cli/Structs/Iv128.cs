using System.Security.Cryptography;

namespace heavenly_bodies_save_deencryptor_cli.Structs;

/// <summary>
/// Represents a 128-bit initialization vector (IV) used in cryptographic operations.
/// </summary>
public readonly struct Iv128
{
    public const int Length = 16;

    private readonly byte[] _value;

    /// <summary>
    /// Gets the content as a read-only span of bytes.
    /// </summary>
    public ReadOnlySpan<byte> Value => _value;

    /// <summary>
    /// Initializes a new instance of the Iv128 class with a randomly generated 128-bit value suitable for cryptographic
    /// operations.
    /// </summary>
    public Iv128()
    {
        _value = new byte[Length];
        RandomNumberGenerator.Fill(_value);
    }

    /// <summary>
    /// Initializes a new instance of the Iv128 class using the specified initialization vector (IV).
    /// </summary>
    /// <param name="iv">A read-only span of bytes that represents the initialization vector. The span must be exactly 16 bytes in length.</param>
    /// <exception cref="ArgumentException">Thrown if the length of iv is not equal to 16 bytes.</exception>
    public Iv128(ReadOnlySpan<byte> iv)
    {
        if (iv.Length != Length)
            throw new ArgumentException($"IV must be {Length} bytes long.");

        _value = iv.ToArray();
    }

    /// <summary>
    /// Creates a new instance of the Iv128 structure from the specified read-only span of bytes.
    /// </summary>
    /// <param name="span">The read-only span of bytes that represents the data used to initialize the Iv128 instance.
    /// The span must contain the appropriate number of bytes required for the Iv128 structure.</param>
    /// <returns>An instance of Iv128 initialized with the data from the provided span.</returns>
    public static Iv128 FromSpan(ReadOnlySpan<byte> span) => new(span);

    /// <summary>
    /// Converts the value of this instance to its hexadecimal string representation.
    /// </summary>
    /// <returns>A string that represents the value of this instance in hexadecimal format.</returns>
    public override string ToString() => Convert.ToHexString(_value);
}