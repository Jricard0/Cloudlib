using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.ResourceManager;
using Cloudlib.Compute.Services;
using Cloudlib.Models;
using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Compute;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Resources;
using System.Linq;
using System.Collections;

namespace Cloudlib.Compute
{
    public class AzureComputeInstances : IComputeInstance
    {
        private readonly ResourceGroupResource _resourceGroup;
        private readonly ArmClient _armClient;
        private readonly VirtualMachineCollection _virtualMachineResources;

        public AzureComputeInstances(string resourceGroup,
            string tenantId,
            string subscriptionId,
            bool excludePowerShellCredentials = true,
            bool excludeSharedTokenCredentials = true,
            bool excludeVisualStudioCredentials = true,
            bool excludeVisualStudioCodeCredential = true,
            bool excludeManagedIdendityCredential = true
        )
        {
            _armClient = new ArmClient(new DefaultAzureCredential(new DefaultAzureCredentialOptions()
            {
                TenantId = tenantId,
                ExcludeAzurePowerShellCredential = excludePowerShellCredentials,
                ExcludeSharedTokenCacheCredential = excludeSharedTokenCredentials,
                ExcludeVisualStudioCredential = excludeVisualStudioCredentials,
                ExcludeManagedIdentityCredential = excludeManagedIdendityCredential,
                ExcludeVisualStudioCodeCredential = excludeVisualStudioCodeCredential
            }));

            _resourceGroup = _armClient.GetSubscriptionResource(new ResourceIdentifier($"/subscriptions/{subscriptionId}")).GetResourceGroup(resourceGroup);
            _virtualMachineResources = _resourceGroup.GetVirtualMachines();
        }

        public List<VirtualMachine> List()
        {
            var virtualMachines = _virtualMachineResources.GetAll();
            List<VirtualMachine> machines = new List<VirtualMachine>();

            foreach (var virtualMachine in virtualMachines)
            {
                var tags = GetTagsFromInstance(virtualMachine);
                VirtualMachine machine = new()
                {
                    Id = virtualMachine.Id,
                    Name = virtualMachine.Data.Name,
                    Tags = tags,
                    Location = new Location()
                    {
                        Region = virtualMachine.Data.Location.Name,
                        Zone = virtualMachine.Data.Zones.FirstOrDefault()
                    }
                };

                machines.Add(machine);
            }

            return machines;
        }

        private List<Tag> GetTagsFromInstance(VirtualMachineResource virtualMachine)
        {
            var tagList = virtualMachine.GetTagResource().Get().Value.Data.TagValues;
            List<Tag> instanceTags = tagList.Select(x => new Tag() { Key = x.Key, Value = x.Value }).ToList<Tag>();
            return instanceTags;
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
            var instance = _virtualMachineResources.Get(name);
            var operationState = instance.Value.PowerOn(WaitUntil.Completed);
            return operationState.HasCompleted;
        }

        public bool Stop(string name, string zone)
        {
            var instance = _virtualMachineResources.Get(name);
            var operationState = instance.Value.PowerOff(WaitUntil.Completed);
            return operationState.HasCompleted;
        }

        public string GetName()
        {
            return "Microsoft Azure Compute Services";
        }

        public Task<bool> StartAsync(string name, string zone)
        {
            throw new NotImplementedException();
        }

        public Task<bool> StopAsync(string name, string zone)
        {
            throw new NotImplementedException();
        }
    }
}

