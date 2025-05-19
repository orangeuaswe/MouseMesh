using System;
using System.IO;

namespace MouseMesh.Core.Models
{
    public class NetworkPacket
    {
       
        public const byte PacketType_deviceInfo = 1;
        public const byte PacketType_mouseUpdate = 2;
        public const byte PacketType_keyboardUpdate = 3;
        public const byte PacketType_clipboardData = 4;
        public const byte PacketType_fileTransfer = 5;
        public byte Type { get; set; }
        public byte[]? Data { get; set; } = Array.Empty<byte>();
        public static NetworkPacket createMouseUpdatePacket(MousePacket mousePacket)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(mousePacket.x);
                    writer.Write(mousePacket.y);
                    writer.Write(mousePacket.buttons);
                    writer.Write(mousePacket.wheelDelta);
                }
                
                NetworkPacket packet = new NetworkPacket();
                packet.Type = PacketType_mouseUpdate;
                packet.Data = ms.ToArray();
                return packet;
            }
        }
        
        public static NetworkPacket createKeyboardUpdatePacket(KeyboardPacket keyboardPacket)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(keyboardPacket.key);
                    writer.Write(keyboardPacket.isDown);
                    writer.Write(keyboardPacket.isExtended);
                    writer.Write(keyboardPacket.modKeys);
                }
                
                NetworkPacket packet = new NetworkPacket();
                packet.Type = PacketType_keyboardUpdate;
                packet.Data = ms.ToArray();
                return packet;
            }
        }
        
        public static NetworkPacket createDeviceInfoPacket(DeviceInfo deviceInfo)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(deviceInfo.deviceId);
                    writer.Write(deviceInfo.name);
                    writer.Write(deviceInfo.screenWidth);
                    writer.Write(deviceInfo.screenHeight);
                }
                
                NetworkPacket packet = new NetworkPacket();
                packet.Type = PacketType_deviceInfo;
                packet.Data = ms.ToArray();
                return packet;
            }
        }
        
        public static NetworkPacket createFileTransferPacket(FileTransferInfo fileTransferInfo)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write(fileTransferInfo.fileName);
                    writer.Write(fileTransferInfo.fileSize);
                    writer.Write(fileTransferInfo.chunkIndex);
                    writer.Write(fileTransferInfo.totalChunks);
                    writer.Write(fileTransferInfo.data.Length);
                    writer.Write(fileTransferInfo.data);
                }
                
                NetworkPacket packet = new NetworkPacket();
                packet.Type = PacketType_fileTransfer;
                packet.Data = ms.ToArray();
                return packet;
            }
        }
        
        public static NetworkPacket createClipboardDataPacket(String clipboardData)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(clipboardData);
            
            NetworkPacket packet = new NetworkPacket();
            packet.Type = PacketType_clipboardData;
            packet.Data = data;
            return packet;
        }
        
        public static MousePacket parseMouseUpdatePacket(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(ms))
                {
                    MousePacket mousePacket = new MousePacket();
                    mousePacket.x = reader.ReadInt32();
                    mousePacket.y = reader.ReadInt32();
                    mousePacket.buttons = reader.ReadInt32();
                    mousePacket.wheelDelta = reader.ReadInt32();
                    return mousePacket;
                }
            }
        }
        
        public static KeyboardPacket parseKeyboardUpdatePacket(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(ms))
                {
                    KeyboardPacket keyboardPacket = new KeyboardPacket();
                    keyboardPacket.key = reader.ReadInt32();
                    keyboardPacket.isDown = reader.ReadBoolean();
                    keyboardPacket.isExtended = reader.ReadBoolean();
                    keyboardPacket.modKeys = reader.ReadInt32();
                    return keyboardPacket;
                }
            }
        }
        
        public static DeviceInfo parseDeviceInfoPacket(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(ms))
                {
                    DeviceInfo deviceInfo = new DeviceInfo();
                    deviceInfo.deviceId = reader.ReadString();
                    deviceInfo.name = reader.ReadString();
                    deviceInfo.screenWidth = reader.ReadInt32();
                    deviceInfo.screenHeight = reader.ReadInt32();
                    return deviceInfo;
                }
            }
        }
        
        public static string parseClipboardDataPacket(byte[] data)
        {
            return System.Text.Encoding.UTF8.GetString(data);
        }
        
        public static FileTransferInfo parseFileTransferInfo(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(ms))
                {
                    string fileName = reader.ReadString();
                    long fileSize = reader.ReadInt64();
                    int chunkIndex = reader.ReadInt32();
                    int totalChunks = reader.ReadInt32();
                    int dataLength = reader.ReadInt32();
                    byte[] fileData = reader.ReadBytes(dataLength);
                    
                    FileTransferInfo fileInfo = new FileTransferInfo();
                    fileInfo.fileName = fileName;
                    fileInfo.fileSize = fileSize;
                    fileInfo.chunkIndex = chunkIndex;
                    fileInfo.totalChunks = totalChunks;
                    fileInfo.data = fileData;
                    return fileInfo;
                }
            }
        }
    }
}