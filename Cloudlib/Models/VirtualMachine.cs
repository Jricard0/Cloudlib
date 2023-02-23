using System;
using System.Collections.Generic;
using System.Net;

namespace Cloudlib.Models
{
    public class VirtualMachine
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Location Location { get; set; }
        public List<Tag> Tags { get; set; }
        public IPAddress PrivateIP { get; set; }
        public IPAddress PublicIP { get; set; }
    }
}
