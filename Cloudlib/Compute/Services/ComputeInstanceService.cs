using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cloudlib.Models;
using Cloudlib.Exceptions;
using Microsoft.Extensions.Logging;

namespace Cloudlib.Compute.Services
{
    public class ComputeInstanceService
    {
        public IComputeInstance computeInstanceClient;

        public ComputeInstanceService(string cloudProvider, string projectName = "", string resourceGroup = "", string tenantId = "", string subscriptionId = "", string accessKey = "", string secretAccessKey = "")
        {
            switch (cloudProvider)
            {
                case "gcloud":
                    computeInstanceClient = new GoogleComputeInstance(projectName);
                    break;
                case "azure":
                    computeInstanceClient = new AzureComputeInstances(resourceGroup, tenantId, subscriptionId);
                    break;
                case "amazon":
                    if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretAccessKey))
                        computeInstanceClient = new AmazonComputeInstance(accessKey, secretAccessKey);
                    else
                        computeInstanceClient = new AmazonComputeInstance();
                    break;
                default:
                    throw new InvalidCloudProviderException($"Cloud provider '{cloudProvider}' not supported.");
            }
        }
    }
}

