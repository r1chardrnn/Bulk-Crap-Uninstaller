/*
    Copyright (c) 2017 Marcin Szeniak (https://github.com/Klocman/)
    Apache License Version 2.0
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Klocman.Native;
using Klocman.Tools;

namespace UninstallTools
{
    public static class RunningProcessTools
    {
        private static readonly int MyProcessId = Environment.ProcessId;

        /// <summary>
        /// Returns the ids of running processes that have a module loaded from any of the given paths.
        /// </summary>
        public static int[] GetRelatedProcessIds(string[] filters, bool doNotKillSteam)
        {
            var idsToCheck = new List<int>();
            foreach (var pr in Process.GetProcesses())
            {
                try
                {
                    if (pr.Id == MyProcessId || pr.HasExited)
                        continue;

                    if (doNotKillSteam && pr.ProcessName.Equals("steam", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (string.IsNullOrEmpty(pr.MainModule?.FileName) ||
                        pr.MainModule.FileName.StartsWith(WindowsTools.GetEnvironmentPath(CSIDL.CSIDL_SYSTEM), StringComparison.OrdinalIgnoreCase))
                        continue;

                    var filenames = pr.Modules.Cast<ProcessModule>()
                        .Select(x => x.FileName)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .Distinct();

                    if (filenames.Any(filename => filters.Any(filter =>
                    {
                        if (string.IsNullOrEmpty(filename))
                            return false;

                        if (!Path.IsPathRooted(filename))
                            return false;

                        return filename.StartsWith(filter, StringComparison.OrdinalIgnoreCase);
                    })))
                    {
                        idsToCheck.Add(pr.Id);
                    }
                }
                catch
                {
                    // Ignore invalid processes
                }
            }

            return idsToCheck.ToArray();
        }
    }
}
