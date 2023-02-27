using System.Text;

namespace Common.Extensions;

public static class ByteArrayExtensions
{
    public static string ToBase64String(this byte[] bytes)
    {
        return System.Convert.ToBase64String(bytes);
    }

    public static byte[] ToBytes(this string str)
    {
        return Encoding.UTF8.GetBytes(str);
    }

    public static string ToString(this byte[] bytes)
    {
        return Encoding.UTF8.GetString(bytes);
    }
}
