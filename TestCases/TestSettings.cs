using System;

namespace Test_Cases
{
    // Project under test. TestOps passes these to TestRunner as environment variables;
    // the defaults apply when tests are run straight from Visual Studio.
    public static class TestSettings
    {
        public const string ProjectNameVariable = "MM_STUDIO_PROJECT";
        public const string VersionVariable = "MM_STUDIO_VERSION";
        public const string InstallerVersionVariable = "MM_STUDIO_INSTALLER_VERSION";

        public static string ProjectName => Read(ProjectNameVariable, "OX01G10");

        public static string Version => Read(VersionVariable, "v1");

        // techFlowJazz installer build under test
        public static string InstallerVersion => Read(InstallerVersionVariable, "1.0");

        private static string Read(string variable, string fallback)
        {
            string value = Environment.GetEnvironmentVariable(variable);

            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
