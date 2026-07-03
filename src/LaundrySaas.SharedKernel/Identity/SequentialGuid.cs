using System;
using System.Security.Cryptography;

namespace LaundrySaas.SharedKernel.Identity;

public static class SequentialGuid
{
    public static Guid NewGuid()
    {
        // Generate a sequential GUID for PostgreSQL (UUID v7/COMB style)
        // PostgreSQL likes the sequential part at the beginning (Microsoft SQL Server at the end).
        // For Npgsql/PostgreSQL, we generate bytes where the timestamp comes first.
        var randomBytes = new byte[10];
        RandomNumberGenerator.Fill(randomBytes);

        var timestamp = DateTime.UtcNow.Ticks;
        var timestampBytes = BitConverter.GetBytes(timestamp);

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timestampBytes);
        }

        var guidBytes = new byte[16];
        // Timestamp (first 6 bytes of ticks is enough for millions of years with ms resolution)
        Array.Copy(timestampBytes, 2, guidBytes, 0, 6);
        // Random bytes (remaining 10 bytes)
        Array.Copy(randomBytes, 0, guidBytes, 6, 10);

        return new Guid(guidBytes);
    }
}
