using System;
using System.Collections.Generic;

namespace Cloudlib.Models
{
    public class VirtualMachine
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public Location Location { get; set; }
        public List<Tag> Tags { get; set; }
    }
}
