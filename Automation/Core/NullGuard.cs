using System;

namespace Automation.Core
{
    public static class NullGuard
    {
        // Turns a "not found" null into a clear failure, instead of a NullReferenceException on the next line
        public static T OrThrow<T>(this T? value, string what) where T : class
        {
            return value ?? throw new InvalidOperationException($"{what} not found.");
        }
    }
}
