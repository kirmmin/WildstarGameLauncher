using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildStarGameLauncher.Model
{
    class ServerEntry
    {
        public string Name { get; private set; }
        public string Hostname { get; private set; }

        public ServerEntry(string name, string hostname)
        {
            Name = name;
            Hostname = hostname;
        }
    }
}
