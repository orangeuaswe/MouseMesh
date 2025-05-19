using System;
using MouseMesh.Core.Models;
namespace MouseMesh.Core.Services
{
    public class DeviceDiscoveredEventArgs : EventArgs
    {
        public DiscoveredDevice device { get; }
        public DeviceDiscoveredEventArgs(DiscoveredDevice device)
        {
            this.device = device;
        }
    }
}