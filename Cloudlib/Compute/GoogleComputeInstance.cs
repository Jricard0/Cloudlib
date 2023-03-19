using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cloudlib.Compute.Services;
using Cloudlib.Models;
using Google.Api.Gax;
using Google.Cloud.Compute.V1;
using Google.Rpc;
using System.Linq;
using System.Net;
using System.Text;

namespace Cloudlib.Compute
{
    public class GoogleComputeInstance : IComputeInstance
    {
        private string Project { get; set; }
        private readonly InstancesClient _instancesClient;

        public GoogleComputeInstance(string project)
        {
            ArgumentException.ThrowIfNullOrEmpty(project);
            Project = project;
            _instancesClient = InstancesClient.Create();
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
                    Tags = new(),
                    PrivateIP = IPAddress.Parse(instance.NetworkInterfaces.Where(i => i.Ipv6Address != null).FirstOrDefault().NetworkIP)
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
                        Tags = instance?.Labels.Select(x => new Tag { Key = x.Key, Value = x.Value }).ToList(),
                        PrivateIP = IPAddress.Parse(instance.NetworkInterfaces[0].NetworkIP),
                        PublicIP = IPAddress.Parse(instance.NetworkInterfaces[0].AccessConfigs[0].NatIP)
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
                            PrivateIP = new(Encoding.UTF8.GetBytes(instance.NetworkInterfaces[0].NetworkIP))
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

        public async Task<bool> StartAsync(string name, string zone)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentException.ThrowIfNullOrEmpty(zone);

            StartInstanceRequest request = new StartInstanceRequest() { Project = Project, Instance = name, Zone = zone };
            var operation = await _instancesClient.StartAsync(request);
            return operation.Result.Status == Operation.Types.Status.Pending;
        }

        public async Task<bool> StopAsync(string name, string zone)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentException.ThrowIfNullOrEmpty(zone);

            StopInstanceRequest request = new StopInstanceRequest() { Project = Project, Instance = name, Zone = zone };
            var operation = await _instancesClient.StopAsync(request);
            return operation.Result.Status == Operation.Types.Status.Pending;
        }

        public async Task<bool> DeleteAsync(string name, string location)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentException.ThrowIfNullOrEmpty(location);

            DeleteInstanceRequest request = new()
            {
                Instance = name,
                Project = Project,
                Zone = location
            };

            var operation = await _instancesClient.DeleteAsync(request);
            return operation.Result.Status == Operation.Types.Status.Pending;
        }

        public bool Delete(string name, string location)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);
            ArgumentException.ThrowIfNullOrEmpty(location);

            DeleteInstanceRequest request = new()
            {
                Instance = name,
                Project = Project,
                Zone = location
            };

            var operation = _instancesClient.Delete(request);
            var result = operation.PollUntilCompleted();
            return result.Result.Status == Operation.Types.Status.Done;
        }
    }
}
