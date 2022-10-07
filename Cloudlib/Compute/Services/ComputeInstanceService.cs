using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cloudlib.Models;
using Cloudlib.Exceptions;

namespace Cloudlib.Compute.Services
{
    public class ComputeInstanceService
    {
        public IComputeInstance computeInstaceClient;

        public ComputeInstanceService(string cloudProvider, string projectName = "", string resourceGroup = "", string tenantId = "", string subscriptionId = "")
        {
            switch (cloudProvider)
            {
                case "gcloud":
                    computeInstaceClient = new GoogleComputeInstance(projectName);
                    break;
                case "azure":
                    computeInstaceClient = new AzureComputeInstances(resourceGroup, tenantId, subscriptionId);
                    break;
                default:
                    throw new InvalidCloudProviderException($"Cloud provider '{cloudProvider}' is unsupported.");
            }
        }
    }
}

