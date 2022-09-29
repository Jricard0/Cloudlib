using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cloudlib.Models;
using Cloudlib.Exceptions;

namespace Cloudlib.Compute.Services
{
    public class ComputeIntanceService
    {
        public IComputeInstance computeInstaceClient;

        public ComputeIntanceService(string cloudProvider, string projectName = "")
        {
            switch (cloudProvider)
            {
                case "gcloud":
                    computeInstaceClient = new GoogleComputeInstance(projectName);
                    break;
                default:
                    throw new InvalidCloudProviderException($"Cloud provider '{cloudProvider}' is unsupported.");
            }
        }
    }
}

