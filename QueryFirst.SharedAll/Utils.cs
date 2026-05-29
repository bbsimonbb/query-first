using System;

namespace QueryFirst.VSExtension
{
    public static class Utils
    {
        private static readonly string n = Environment.NewLine;
        public static string TellMeEverything(this Exception ex, string indent = "")
        {
            return $@"{n}{indent}{ex?.Message}
{indent}{ex.StackTrace.Replace(n, n + indent)}
{ex?.InnerException?.TellMeEverything(indent + "  ")}";
        }
    }
}
