// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;

namespace Microsoft.DotNet.Interactive.PowerShell.Host
{
    using System.Management.Automation;

    internal static class DollarProfileHelper
    {
        private const string _profileName = "Microsoft.dotnet-interactive_profile.ps1";

        private static readonly string _allUsersCurrentHost = DollarProfileHelper.GetFullProfileFilePath(forCurrentUser: false);
        private static readonly string _currentUserCurrentHost = DollarProfileHelper.GetFullProfileFilePath(forCurrentUser: true);

        private static bool _haveRunProfiles;

        private static string GetFullProfileFilePath(bool forCurrentUser)
        {
            if (!forCurrentUser)
            {
                string pshome = Path.GetDirectoryName(typeof(PSObject).Assembly.Location);
                return Path.Combine(pshome, _profileName);
            }

            string configPath = Platform.IsWindows
                ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "PowerShell")
                : Platform.SelectProductNameForDirectory(Platform.XDG_Type.CONFIG);

            return Path.Combine(configPath, _profileName);
        }

        public static void SetDollarProfile(PowerShell pwsh)
        {
            PSObject dollarProfile = new PSObject(_currentUserCurrentHost);
            dollarProfile.Properties.Add(new PSNoteProperty("AllUsersCurrentHost", _allUsersCurrentHost));
            dollarProfile.Properties.Add(new PSNoteProperty("CurrentUserCurrentHost", _currentUserCurrentHost));
            // TODO: Decide on whether or not we want to support running the AllHosts profiles

            pwsh.Runspace.SessionStateProxy.SetVariable("PROFILE", dollarProfile);
        }

        public static void RunProfilesIfNeeded(PowerShell pwsh, PowerShellKernel pwshKernel)
        {
            if (pwshKernel.HasRunProfiles)
            {
                return;
            }

            // Run the PROFILE scripts if they exist.
            if (File.Exists(_allUsersCurrentHost))
            {
                pwshKernel.RunSubmitCodeLocally(pwsh, _allUsersCurrentHost);
            }

            if (File.Exists(_currentUserCurrentHost))
            {
                pwshKernel.RunSubmitCodeLocally(pwsh, _currentUserCurrentHost);
            }
        }
    }
}
