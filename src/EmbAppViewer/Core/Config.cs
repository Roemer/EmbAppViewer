using System.Collections.Generic;

namespace EmbAppViewer.Core
{
    public class Config
    {
        public Config()
        {
            Items = new List<Item>();
        }

        public List<Item> Items { get; set; }
    }
}
