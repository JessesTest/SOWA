using Microsoft.AspNetCore.Mvc;

namespace Common.Web.Extensions.Alerts;

public static class AlertExtensions
{
    private static IActionResult Alert(IActionResult result, string type, string title, string body)
    {
        return new AlertDecoratorResult(result, type, title, body);
    }

    public static IActionResult WithSuccess(this IActionResult result, string title, string body)
    {
        return Alert(result, "success", title, body);
    }

    public static IActionResult WithInfo(this IActionResult result, string title, string body)
    {
        return Alert(result, "info", title, body);
    }

    public static IActionResult WithWarning(this IActionResult result, string title, string body)
    {
        return Alert(result, "warning", title, body);
    }

    public static IActionResult WithDanger(this IActionResult result, string title, string body)
    {
        return Alert(result, "danger", title, body);
    }


    public static IActionResult WithSuccessWhen(this IActionResult result, bool when, string title, string body)
    {
        return when ? result.WithSuccess(title, body) : result;
    }

    public static IActionResult WithInfoWhen(this IActionResult result, bool when, string title, string body)
    {
        return when ? result.WithInfo(title, body) : result;
    }

    public static IActionResult WithWarningWhen(this IActionResult result, bool when, string title, string body)
    {
        return when ? result.WithWarning(title, body) : result;
    }

    public static IActionResult WithDangerWhen(this IActionResult result, bool when, string title, string body)
    {
        return when ? result.WithDanger(title, body) : result;
    }
}
