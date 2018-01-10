using System;

namespace EmbAppViewer.Core
{
    /// <summary>
    /// Represents an item from which applications to embedd can be started.
    /// </summary>
    public class ApplicationItem
    {
        public ApplicationItem(string name, string executablePath)
        {
            Name = name;
            ExecutablePath = executablePath;
        }

        /// <summary>
        /// The display name of the application.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The path of the executable of the application.
        /// </summary>
        public string ExecutablePath { get; set; }

        /// <summary>
        /// The command line arguments used when starting the application.
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// Flag to indicate if the embedded application should resize to the containers size or just keep its original size.
        /// </summary>
        public bool Resize { get; set; } = true;

        /// <summary>
        /// Flag to indicate if the application can be started multiple times.
        /// </summary>
        public bool AllowMultiple { get; set; }

        /// <summary>
        /// The maximum time to wait until a process window can be found.
        /// </summary>
        public TimeSpan MaxLoadTime { get; set; } = TimeSpan.FromSeconds(5);

        public override string ToString()
        {
            return Name;
        }
    }
}
