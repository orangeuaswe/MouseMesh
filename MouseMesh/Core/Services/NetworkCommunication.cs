using System;
using System.Diagnostics;
using System.Formats.Asn1;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;
using MouseMesh.Core.Models;
namespace MouseMesh.Core.Services
{
    public class NetworkCommunication : IDisposable
    {
        private const int dataBufferSize = 8192;
        private TcpClient client;
        private NetworkStream stream;
        private AesManaged aes;
        private ICryptoTransform encryptor;
        private ICryptoTransform decryptor;
        private byte[] readBuffer;
        public event EventHandler<DataReceivedEventArgs> dataReceived;
        public event EventHandler<ConnectionStatusEventArgs> connectionStatusChanged;
        public NetworkCommunication()
        {
            readBuffer = new byte[dataBufferSize];
            InitializeCrypto();
        }
        public async Task<bool> connectAsync(string ipAddress, int port)
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync(ipAddress, port);
                stream = client.GetStream();
                startReading();
                raiseConnectionStatusChanged(true);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error connecting: {e.Message}");
                raiseConnectionStatusChanged(false);
                return false;
            }
        }
        public byte[] getEncryptionKey()
        {
            return aes.Key;
        }
        public byte[] getEncryptionIV()
        {
            return aes.IV;
        }
        public void setEncryptionParams(byte[] key, byte[] iv)
        {
            encryptor = aes.CreateEncryptor(key, iv);
            decryptor = aes.CreateDecryptor(key, iv);
        }
        private void InitializeCrypto()
        {
            aes = new AesManaged
            {
                KeySize = 256,
                BlockSize = 128,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            };
            aes.GenerateKey();
            aes.GenerateIV();
            encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        }
        private void startReading()
        {
            Task.Run(async () =>
            {
                try
                {
                    while (client.Connected)
                    {
                        int bytesRead = await stream.ReadAsync(readBuffer, 0, readBuffer.Length);
                        if (bytesRead > 0)
                        {
                            processReceivedData(readBuffer, bytesRead);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error reading data: {e.Message}");
                }
                raiseConnectionStatusChanged(false);
            });
        }
        private void processReceivedData(byte[] data, int bytesRead)
        {
            try
            {
                byte[] decryptedData;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, bytesRead);
                        cs.FlushFinalBlock();
                    }
                    decryptedData = ms.ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error processing data: {e.Message}");
            }
        }
        public async Task sendDataAsync(byte[] data)
        {
            if (client == null || !client.Connected)
            {
                throw new InvalidOperationException("Not connected");
            }
            try
            {
                byte[] encryptedData;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }
                    encryptedData = ms.ToArray();
                }
                byte[] lengthPrefix = BitConverter.GetBytes(encryptedData.Length);
                await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                await stream.WriteAsync(encryptedData, 0, encryptedData.Length);
                await stream.FlushAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error sending data: {e.Message}");
                raiseConnectionStatusChanged(false);
            }
        }
        public async Task sendMouseUpdateAsync(MousePacket mouseUpdate)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((byte)PacketType.MouseUpdate);
                    writer.Write(mouseUpdate.x);
                    writer.Write(mouseUpdate.y);
                    writer.Write(mouseUpdate.buttons);
                    writer.Write(mouseUpdate.wheelDelta);
                }
                await sendDataAsync(ms.ToArray());
            }
        }
        public async Task sendKeyboardUpdateAsync(KeyboardPacket keyboardUpdate)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms))
                {
                    writer.Write((byte)PacketType.KeyboardUpdate);
                    writer.Write(keyboardUpdate.key);
                    writer.Write(keyboardUpdate.isDown);
                    writer.Write(keyboardUpdate.isExtended);
                    writer.Write(keyboardUpdate.modKeys);
                }
                await sendDataAsync(ms.ToArray());
            }
        }
        private void raiseConnectionStatusChanged(bool isConnected)
        {
            connectionStatusChanged?.Invoke(this, new ConnectionStatusEventArgs(isConnected));
        }
        public void Dispose()
        {
            stream?.Dispose();
            client?.Dispose();
            encryptor?.Dispose();
            decryptor?.Dispose();
            aes?.Dispose();
        }

    }
    public enum PacketType : byte
    {
        DeviceInfo = 1,
        MouseUpdate = 2,
        KeyboardUpdate = 3,
        ClipboardData = 4,
        FileTransfer = 5
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; }

        public DataReceivedEventArgs(byte[] data)
        {
            Data = data;
        }
    }

    public class ConnectionStatusEventArgs : EventArgs
    {
        public bool IsConnected { get; }

        public ConnectionStatusEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }
    }
}