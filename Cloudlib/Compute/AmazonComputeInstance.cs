using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cloudlib.Compute.Services;
using Cloudlib.Models;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using System.Text.RegularExpressions;
using System.Linq;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace Cloudlib.Compute
{
    public class AmazonComputeInstance : IComputeInstance
    {
        private readonly string _accessKey;
        private readonly string _secretAccessKey;
        private readonly bool _isCredentialsLoaded = false;
        private AmazonEC2Client _client;

        public AmazonComputeInstance(string accessKey, string secretAccessKey)
        {
            _accessKey = accessKey;
            _secretAccessKey = secretAccessKey;
            _client = new AmazonEC2Client(accessKey, secretAccessKey, RegionEndpoint.USEast1);
        }

        public AmazonComputeInstance()
        {
            _isCredentialsLoaded = true;
            _client = new AmazonEC2Client();
        }

        public string GetName()
        {
            return "Amazon AWS Compute Instances";
        }

        public List<VirtualMachine> List()
        {
            throw new NotSupportedException("This method does not have support, because AWS does not have synchronous operations");
        }

        public List<VirtualMachine> List(string location)
        {
            throw new NotSupportedException("This method does not have support, because AWS does not have synchronous operations");
        }

        public async Task<List<VirtualMachine>> ListAsync()
        {
            List<VirtualMachine> virtualMachines = new();
            foreach (var region in RegionEndpoint.EnumerableAllRegions)
            {
                _client = BuildClient(region.SystemName);//TODO: add support to native RegionEndpoint type, not only string
                virtualMachines.AddRange(await HandleInstanceList(_client));
            }
            return virtualMachines;
        }

        private async Task<List<VirtualMachine>> HandleInstanceList(AmazonEC2Client client)
        {
            var response = await client.DescribeInstancesAsync();

            List<VirtualMachine> virtualMachines = new List<VirtualMachine>();

            foreach (var reservation in response.Reservations)
            {
                foreach (var instance in reservation.Instances)
                {
                    VirtualMachine virtualMachine = new VirtualMachine
                    {
                        Id = instance.InstanceId,
                        Location = new Models.Location
                        {
                            Region = _client.Config.RegionEndpoint.SystemName,
                            Zone = instance.Placement.AvailabilityZone
                        },
                        Name = instance?.Tags.Find(t => t.Key == "Name").Value
                    };

                    virtualMachines.Add(virtualMachine);
                }
            }
            return virtualMachines;
        }

        private AmazonEC2Client BuildClient(string location)
        {
            ArgumentNullException.ThrowIfNull(location);

            var region = RegionEndpoint.GetBySystemName(location);

            if (_isCredentialsLoaded)
                return new AmazonEC2Client(region);
            return new AmazonEC2Client(_accessKey, _secretAccessKey, region);
        }

        public async Task<List<VirtualMachine>> ListAsync(string location)
        {
            ArgumentNullException.ThrowIfNull(location);
            _client = BuildClient(location);
            var response = await _client.DescribeInstancesAsync();

            List<VirtualMachine> virtualMachines = new List<VirtualMachine>();
            virtualMachines.AddRange(await HandleInstanceList(_client));
            return virtualMachines;
        }

        public async Task<bool> StartAsync(string id, string zone)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(zone);

            _client = BuildClient(zone);
            StartInstancesRequest request = new StartInstancesRequest
            {
                InstanceIds = new List<string> { id }
            };

            var response = await _client.StartInstancesAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        public bool Stop(string name, string zone)
        {
            throw new NotSupportedException("This method does not have support, because AWS does not have synchronous operations");
        }

        public bool Start(string name, string zone)
        {
            throw new NotSupportedException("This method does not have support, because AWS does not have synchronous operations");
        }

        public async Task<bool> StopAsync(string id, string zone)
        {
            ArgumentNullException.ThrowIfNull(id);
            ArgumentNullException.ThrowIfNull(zone);

            _client = BuildClient(zone);
            StopInstancesRequest request = new StopInstancesRequest
            {
                InstanceIds = new List<string> { id }
            };

            var response = await _client.StopInstancesAsync(request);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}

