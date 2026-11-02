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

        // Bin sheet the Bins tests open and edit, so they leave the Default sheet alone
        public const string BinSheetName = "BinAutomation";

        // Pin map sheet the Pin Maps tests open and edit, so they leave the Default sheet alone
        public const string PinMapSheetName = "PinAutomation";

        // techFlowJazz installer build under test
        public static string InstallerVersion => Read(InstallerVersionVariable, "1.0");

        // First two parts of the installer version, e.g. "1.1.0.1073" -> "1.1"
        public static string InstallerRelease
        {
            get
            {
                string[] parts = InstallerVersion.Split('.');

                return parts.Length >= 2 ? $"{parts[0]}.{parts[1]}" : InstallerVersion;
            }
        }

        // Error shown for a wrong username or password; 1.1 added the "Failed to login: " prefix
        public static string InvalidCredentialsMessage
        {
            get
            {
                switch (InstallerRelease)
                {
                    case "1.0":
                        return "Invalid username or password.";
                    case "1.1":
                        return "Failed to login: Invalid username or password.";
                    default:
                        throw new NotSupportedException(
                            $"Unknown installer version '{InstallerVersion}'. Expected 1.0.x or 1.1.x.");
                }
            }
        }

        // Error shown when the username is left blank
        public static string EmptyUsernameMessage
        {
            get
            {
                switch (InstallerRelease)
                {
                    case "1.0":
                        return "Username / password cannot be empty!";
                    case "1.1":
                        return "Failed to login: Username is empty.";
                    default:
                        throw new NotSupportedException(
                            $"Unknown installer version '{InstallerVersion}'. Expected 1.0.x or 1.1.x.");
                }
            }
        }

        private static string Read(string variable, string fallback)
        {
            string value = Environment.GetEnvironmentVariable(variable);

            return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        }
    }
}
