using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MouseMesh.Core.Models;

namespace MouseMesh.Core.Services
{
    public class NetworkDiscovery : IDisposable
    {
        private const int discoveryPort = 12345;
        private const int broadcastInterval = 5000;
        private UdpClient broadcastClient;
        private UdpClient listenClient;
        private Thread broadcastThread;
        private Thread listenThread;
        private bool isRunning = false;
        private bool disposed = false;
        
        public event EventHandler<DeviceDiscoveredEventArgs>? deviceDiscovered;
        public event EventHandler<DeviceDiscoveredEventArgs>? deviceLost;
        
        public NetworkDiscovery()
        {
            initializeUDPClients();
        }
        
        private void initializeUDPClients()
        {
            try
            {
                broadcastClient = new UdpClient();
                broadcastClient.EnableBroadcast = true;
                listenClient = new UdpClient(discoveryPort);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error initializing UDP clients: {e.Message}");
            }
        }
        
        public void startDiscovery()
        {
            if (isRunning)
            {
                return;
            }
            isRunning = true;
            
            broadcastThread = new Thread(BroadcastThread);
            broadcastThread.IsBackground = true;
            broadcastThread.Start();
            
            listenThread = new Thread(ListenThread);
            listenThread.IsBackground = true;
            listenThread.Start();
        }
        
        public void stopDiscovery()
        {
            isRunning = false;
            broadcastThread?.Join(1000);
            listenThread?.Join(1000);
        }
        
        private void BroadcastThread()
        {
            try
            {
                var endpoint = new IPEndPoint(IPAddress.Broadcast, discoveryPort);
                while (isRunning)
                {
                    try
                    {
                        var deviceInfo = new BroadcastMessage
                        {
                            DeviceId = getDeviceId(),
                            Name = Environment.MachineName,
                            IpAddress = getLocalIpAddress(),
                            ScreenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width,
                            ScreenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height
                        };
                        
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(deviceInfo);
                        byte[] data = Encoding.UTF8.GetBytes(json);
                        
                        broadcastClient.Send(data, data.Length, endpoint);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error broadcasting: {e.Message}");
                    }
                    
                    Thread.Sleep(broadcastInterval);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Broadcast thread error: {e.Message}");
            }
        }
        
        private void ListenThread()
        {
            try
            {
                var discoveredDevices = new System.Collections.Concurrent.ConcurrentDictionary<string, Tuple<DiscoveredDevice, DateTime>>();
                
                var timeoutThread = new Thread(() =>
                {
                    while (isRunning)
                    {
                        foreach (var deviceId in discoveredDevices.Keys)
                        {
                            if (discoveredDevices.TryGetValue(deviceId, out var deviceTuple))
                            {
                                if ((DateTime.Now - deviceTuple.Item2).TotalSeconds > 15)
                                {
                                    if (discoveredDevices.TryRemove(deviceId, out var removedDevice))
                                    {
                                        deviceLost?.Invoke(this, new DeviceDiscoveredEventArgs(removedDevice.Item1));
                                    }
                                }
                            }
                        }
                        Thread.Sleep(5000);
                    }
                });
                timeoutThread.IsBackground = true;
                timeoutThread.Start();
                
                while (isRunning)
                {
                    try
                    {
                        var endpoint = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = listenClient.Receive(ref endpoint);
                        
                        if (endpoint.Address.Equals(IPAddress.Parse(getLocalIpAddress())))
                        {
                            continue;
                        }
                        
                        string json = Encoding.UTF8.GetString(data);
                        var deviceInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<BroadcastMessage>(json);
                        
                        var device = new DiscoveredDevice
                        {
                            deviceId = deviceInfo.DeviceId,
                            name = deviceInfo.Name,
                            ipAddress = deviceInfo.IpAddress,
                            screenWidth = deviceInfo.ScreenWidth,
                            screenHeight = deviceInfo.ScreenHeight
                        };
                        
                        bool isNew = !discoveredDevices.ContainsKey(device.deviceId);
                        discoveredDevices[device.deviceId] = new Tuple<DiscoveredDevice, DateTime>(device, DateTime.Now);
                        
                        if (isNew)
                        {
                            deviceDiscovered?.Invoke(this, new DeviceDiscoveredEventArgs(device));
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error listening: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Listen thread error: {e.Message}");
            }
        }
        
        private string getDeviceId()
        {
            return Guid.NewGuid().ToString();
        }
        
        private string getLocalIpAddress()
        {
            string localIP = "127.0.0.1";
            
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        localIP = ip.ToString();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error getting local IP: {e.Message}");
            }
            
            return localIP;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    stopDiscovery();
                    
                    broadcastClient?.Close();
                    listenClient?.Close();
                }
                
                disposed = true;
            }
        }
        
        ~NetworkDiscovery()
        {
            Dispose(false);
        }
    }
}

class BroadcastMessage
{
    public string DeviceId { get; set; }
    public string Name { get; set; }
    public string IpAddress { get; set; }
    public int ScreenWidth { get; set; }
    public int ScreenHeight { get; set; }
}