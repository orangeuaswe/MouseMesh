using System;
namespace MouseMesh.Core.Models
{
    [Serializable]
    public class DeviceInfo
    {
        public String deviceId { get; set; }
        public String name { get; set; }
        public String ipAddress { get; set; }
        public int screenWidth { get; set; }
        public int screenHeight { get; set; }
        public int posX { get; set; }
        public int posY{ get; set; }
    }
}
