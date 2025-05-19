using System;
namespace MouseMesh.Core.Models
{
    public class FileTransferInfo
    {
        public String fileName { get; set; }
        public long fileSize { get; set; }
        public int chunkIndex { get; set; }
        public int totalChunks { get; set; }
        public byte[] data { get; set; }
        
    }
}