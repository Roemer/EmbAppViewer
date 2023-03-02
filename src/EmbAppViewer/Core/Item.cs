using System;
using System.Collections.Generic;

namespace EmbAppViewer.Core
{
    /// <summary>
    /// Represents an item in the tree. Can either be a folder or an application.
    /// </summary>
    public class Item
    {
        /// <summary>
        /// The display name of the application.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The path of the executable of the application.
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// The command line arguments used when starting the application.
        /// </summary>
        public string? Arguments { get; set; }

        /// <summary>
        /// The work directory for the application.
        /// </summary>
        public string? WorkDirectory { get; set; }

        /// <summary>
        /// Flag to indicate if the embedded application should resize to the containers size or just keep its original size.
        /// </summary>
        public bool Resize { get; set; } = true;

        /// <summary>
        /// Flag to indicate if the application can be started multiple times.
        /// </summary>
        public bool Multiple { get; set; }

        /// <summary>
        /// The maximum time to wait until a process window can be found.
        /// </summary>
        public TimeSpan MaxLoadTime { get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// The list of nested items.
        /// </summary>
        public List<Item> Items { get; set; } = new List<Item>();

        /// <summary>
        /// Flag to indicate if the item is a folder.
        /// </summary>
        public bool IsFolder => String.IsNullOrWhiteSpace(Path);

        /// <summary>
        /// Flag to indicate if the item is an application.
        /// </summary>
        public bool IsApp => !IsFolder;

        public override string ToString()
        {
            return $"{(IsFolder ? "Dir" : "App")}: {Name}";
        }
    }
}
