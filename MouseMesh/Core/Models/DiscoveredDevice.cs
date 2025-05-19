using System;
namespace MouseMesh.Core.Models
{
    public class DiscoveredDevice
    {
        public String deviceId { get; set; }
        public String name { get; set; }
        public String ipAddress { get; set; }
        public int screenWidth { get; set; }
        public int screenHeight { get; set; }
        
    }
}