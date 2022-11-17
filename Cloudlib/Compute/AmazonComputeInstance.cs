using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cloudlib.Compute.Services;
using Cloudlib.Models;

namespace Cloudlib.Compute
{
    public class AmazonComputeInstance : IComputeInstance
    {
        public AmazonComputeInstance()
        {
        }

        public string GetName()
        {
            return "Amazon AWS Compute Instances";
        }

        public List<VirtualMachine> List()
        {
            throw new NotImplementedException();
        }

        public List<VirtualMachine> List(string location)
        {
            throw new NotImplementedException();
        }

        public Task<List<VirtualMachine>> ListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<VirtualMachine>> ListAsync(string location)
        {
            throw new NotImplementedException();
        }

        public bool Start(string name, string zone)
        {
            throw new NotImplementedException();
        }

        public bool Stop(string name, string zone)
        {
            throw new NotImplementedException();
        }
    }
}

