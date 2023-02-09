using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cloudlib.Compute.Services;
using Cloudlib.Models;
using Google.Api.Gax;
using Google.Cloud.Compute.V1;
using Google.Rpc;

namespace Cloudlib.Compute
{
    public class GoogleComputeInstance : IComputeInstance
    {
        private string Project { get; set; }
        private readonly InstancesClient _instancesClient;

        public GoogleComputeInstance(string project)
        {
            _instancesClient = InstancesClient.Create();
            Project = project;
        }

        public List<VirtualMachine> List()
        {
            ListInstancesRequest request = new() { Project = Project };
            return ListInstancesHandler(request);
        }

        public List<VirtualMachine> List(string zone)
        {
            if (string.IsNullOrWhiteSpace(zone))
            {
                throw new Exception("A Google Zone is required to perform this action.");
            }

            ListInstancesRequest request = new() { Project = Project, Zone = zone };
            return ListInstancesHandler(request);
        }

        private List<VirtualMachine> ListInstancesHandler(ListInstancesRequest request)
        {
            List<VirtualMachine> virtualMachines = new();
            foreach (var instance in _instancesClient.List(request))
            {
                VirtualMachine virtualMachine = new()
                {
                    Id = instance.Id.ToString(),
                    Location = new Location()
                    {
                        Region = instance.Zone.Split("zones/")[1].Split("-")[0],
                        Zone = instance.Zone.Split("zones/")[1]
                    },
                    Name = instance.Name,
                    Tags = new()
                };

                virtualMachines.Add(virtualMachine);
            }

            return virtualMachines;
        }

        public async Task<List<VirtualMachine>> ListAsync()
        {
            List<VirtualMachine> virtualMachines = new List<VirtualMachine>();

            await foreach (var instancesByZone in _instancesClient.AggregatedListAsync(Project))
            {
                foreach (var instance in instancesByZone.Value.Instances)
                {
                    VirtualMachine virtualMachine = new VirtualMachine()
                    {
                        Id = instance.Id.ToString(),
                        Name = instance.Name,
                        Location = new Location()
                        {
                            Region = instance.Zone.Split("zones/")[1].Split("-")[0],
                            Zone = instance.Zone.Split("zones/")[1]
                        },
                    };

                    virtualMachines.Add(virtualMachine);
                }
            }

            return virtualMachines;
        }

        public async Task<List<VirtualMachine>> ListAsync(string zone)
        {
            if (string.IsNullOrWhiteSpace(zone))
            {
                throw new Exception("A Google Zone is required to perform this action.");
            }

            List<VirtualMachine> virtualMachines = new List<VirtualMachine>();
            AggregatedListInstancesRequest request = new AggregatedListInstancesRequest()
            {
                Project = Project,
            };

            await foreach (var instancesByZone in _instancesClient.AggregatedListAsync(request))
            {
                if (instancesByZone.Key == $"zones/{zone}")
                {
                    foreach (var instance in instancesByZone.Value.Instances)
                    {
                        VirtualMachine virtualMachine = new VirtualMachine()
                        {
                            Id = instance.Id.ToString(),
                            Name = instance.Name,
                            Location = new Location()
                            {
                                Region = Regex.Replace(instance.Zone.Split("zones/")[1], "/[a-z]$/gm", ""),
                                Zone = instance.Zone.Split("zones/")[1]
                            },
                        };

                        virtualMachines.Add(virtualMachine);
                    }
                }
            }
            return virtualMachines;
        }

        public bool Start(string name, string zone)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(zone);

            StartInstanceRequest request = new StartInstanceRequest() { Project = Project, Instance = name, Zone = zone };
            var operation = _instancesClient.Start(request).PollUntilCompleted();
            return operation.Result.Status == Operation.Types.Status.Done;
        }

        public bool Stop(string name, string zone)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(zone))
            {
                throw new Exception("Instance name and Zone are required to perform this action");
            }

            StopInstanceRequest request = new StopInstanceRequest() { Project = Project, Instance = name, Zone = zone };
            var operation = _instancesClient.Stop(request).PollUntilCompleted();

            return operation.Result.Status == Operation.Types.Status.Done;
        }

        public string GetName()
        {
            return "Google Cloud Compute";
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
