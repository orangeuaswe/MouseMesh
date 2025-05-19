using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using MouseMesh.Core.Models;
using MouseMesh.Core.Input;
namespace MouseMesh.Core.Services
{
    public class DeviceManager
    {
        private List<DeviceInfo> connectedDevices = new List<DeviceInfo>();
        private Dictionary<string, NetworkCommunication> deviceConnections = new Dictionary<string, NetworkCommunication>();
        private string configFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MouseMesh", "devices.xml");
        public DeviceInfo currentTargetDevice { get; private set; }
        public bool isMouseOnRemoteDevice { get; private set; }
        public event EventHandler<DeviceEventArgs>? deviceConnected;
        public event EventHandler<DeviceEventArgs>? deviceDisconnected;
        public event EventHandler<DeviceEventArgs>? devicePosChanged;
        public DeviceManager()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));
        }
        public IEnumerable<DeviceInfo> getConnectedDevices()
        {
            return connectedDevices.AsReadOnly();
        }
        public async void connectToDevice(DiscoveredDevice device)
        {
            
        }
    }
}