using System.Text;

namespace Api.Security;

public static class Base64Url
{
    public static string Encode(byte[] bytes) => Convert.ToBase64String(bytes)
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

    public static byte[] Decode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + (4 - padded.Length % 4) % 4, '=');
        return Convert.FromBase64String(padded);
    }

    public static string EncodeText(string value) => Encode(Encoding.UTF8.GetBytes(value));

    public static string DecodeText(string value) => Encoding.UTF8.GetString(Decode(value));
}
