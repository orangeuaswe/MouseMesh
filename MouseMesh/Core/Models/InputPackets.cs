namespace MouseMesh.Core.Models
{
    public class MousePacket
    {
        public int x { get; set; }
        public int y { get; set; }
        public int buttons { get; set; }
        public int wheelDelta { get; set; }
    }
    public class KeyboardPacket
    {
        public int key { get; set; }
        public bool isDown { get; set; }
        public bool isExtended { get; set; }
        public int modKeys{ get; set; }
    }
}