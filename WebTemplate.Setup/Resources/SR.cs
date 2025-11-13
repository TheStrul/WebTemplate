using System;
using System.Resources;

namespace WebTemplate.Setup.Resources;

public static class SR
{
    private static readonly ResourceManager _rm = new("WebTemplate.Setup.Resources.Strings", typeof(SR).Assembly);

    public static string Get(string name)
    {
        var s = _rm.GetString(name);
        if (s is null)
        {
            // Fail fast: do not fallback silently
            throw new InvalidOperationException($"Missing resource string: {name}");
        }
        return s;
    }
}
