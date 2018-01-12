using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace EmbAppViewer.Core
{
    /// <summary>
    /// Extension methods for <see cref="Process"/>.
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// Gets all direct child processes of the given process.
        /// </summary>
        public static IEnumerable<Process> GetChildProcesses(this Process process)
        {
            var mos = new ManagementObjectSearcher($"Select * From Win32_Process Where ParentProcessID={process.Id}");
            return mos.Get()
                .Cast<ManagementBaseObject>()
                .Select(mo => Process.GetProcessById(Convert.ToInt32(mo["ProcessID"]))).ToList();
        }
    }
}
