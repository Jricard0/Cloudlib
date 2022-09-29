using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cloudlib.Models;

namespace Cloudlib.Compute.Services
{
    public interface IComputeInstance
    {
        public Task<List<VirtualMachine>> ListAsync();
        public Task<List<VirtualMachine>> ListAsync(string location);
        public List<VirtualMachine> List();
        public List<VirtualMachine> List(string location);
        public bool Start(string name, string zone);
        public bool Stop(string name, string zone);
    }
}
