using System;
using MouseMesh.Core.Models;
namespace MouseMesh.Core.Services
{
    public class DeviceEventArgs : EventArgs
    {
        public DeviceInfo device { get; }
        public DeviceEventArgs(DeviceInfo device)
        {
            this.device = device;
        }
    }
}