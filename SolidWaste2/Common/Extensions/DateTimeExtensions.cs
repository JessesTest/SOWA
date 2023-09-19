namespace Common.Extensions;

public static class DateTimeExtensions
{
    public static DateOnly ToDateOnly(this DateTime dateTime) => DateOnly.FromDateTime(dateTime);

    public static DateOnly? ToDateOnly(this string str)
    {
        return string.IsNullOrWhiteSpace(str) ? null : DateOnly.Parse(str);
    }

    public static TimeOnly? ToTimeOnly(this string str)
    {
        return string.IsNullOrWhiteSpace(str) ? null : TimeOnly.Parse(str);
    }
}
