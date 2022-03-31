using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using www.SoLaNoSoft.com.BearChess.BearChessCommunication;

namespace www.SoLaNoSoft.com.BearChessBTLETools
{
    // ReSharper disable once IdentifierTypo
    public class BTLEComPort : IComPort
    {
        private readonly string _pegasusService           = "6E400001-B5A3-F393-E0A9-E50E24DCCA9E";
        private readonly string _mChessLinkService        = "49535343-FE7D-4AE5-8FA9-9FAFD205E455";
        private readonly string _mChessLinkServiceWrite   = "49535343-8841-43F4-A8D4-ECBE34729BB3";
        private readonly string _mChessLinkServiceRead    = "49535343-1E4D-4BD9-BA61-23C647249616";
        private readonly string _pegasusServiceWrite      = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E";
        private readonly string _pegasusServiceRead       = "6E400003-B5A3-F393-E0A9-E50E24DCCA9E";
        private readonly string _squareOffProServiceWrite = "6E400002-B5A3-F393-E0A9-E50E24DCCA9E";
        private readonly string _squareOffProServiceRead  = "6E400003-B5A3-F393-E0A9-E50E24DCCA9E";
        private readonly string _deviceId;
        private BluetoothLEDevice _bluetoothLeDevice = null;
        private GattCharacteristic _writeCharacteristic = null;
        private GattCharacteristic _readCharacteristic = null;
        private GattDeviceService _service = null;
        private readonly ConcurrentQueue<byte[]> _byteArrayQueue = new ConcurrentQueue<byte[]>();
        private readonly ConcurrentQueue<byte> _byteQueue = new ConcurrentQueue<byte>();

        private static readonly Guid ResultCharacteristicUuid = Guid.Parse("6E400003-B5A3-F393-E0A9-E50E24DCCA9E");

        #region Error Codes
        readonly int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        readonly int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        readonly int E_ACCESSDENIED = unchecked((int)0x80070005);
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        #endregion

        private List<string> _allServices;
        private List<string> _allReadCharServices;
        private List<string> _allWriteCharServices;

        public string PortName => "BTE";

        public BTLEComPort(string deviceId)
        {
            _deviceId = deviceId;
            _allServices = new List<string>() { _pegasusService, _mChessLinkService };
            _allReadCharServices = new List<string>() { _mChessLinkServiceRead, _pegasusServiceRead, _squareOffProServiceRead };
            _allWriteCharServices = new List<string>() { _mChessLinkServiceWrite, _pegasusServiceWrite, _squareOffProServiceWrite };

        }

        private static ushort ParseHeartRateValue(byte[] data)
        {
            // Heart Rate profile defined flag values
            const byte heartRateValueFormat = 0x01;

            byte flags = data[0];
            bool isHeartRateValueSizeLong = ((flags & heartRateValueFormat) != 0);

            if (isHeartRateValueSizeLong)
            {
                return BitConverter.ToUInt16(data, 1);
            }

            return data[1];
        }

        public void Open()
        {

            _bluetoothLeDevice = AsyncHelper.RunSync(async () => await BluetoothLEDevice.FromIdAsync(_deviceId));
            if (_bluetoothLeDevice != null)
            {
                
                GattDeviceServicesResult result = AsyncHelper.RunSync(async () => await _bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Cached));
                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    foreach (GattDeviceService service in services)
                    {
                        var serviceUuid = service.Uuid.ToString("D").ToUpper();
                        if (_allServices.Contains(serviceUuid))
                        {
                            _service = service;
                            _readCharacteristic = null;
                            _writeCharacteristic = null;
                            GattCharacteristicsResult readOnlyList = AsyncHelper.RunSync(async () => await service.GetCharacteristicsAsync(
                                BluetoothCacheMode.Cached));
                            foreach (var gattCharacteristic in readOnlyList.Characteristics)
                            {
                                var gattChar = gattCharacteristic.Uuid.ToString("D").ToUpper();
                                if (_allReadCharServices.Contains(gattChar))
                                {
                                    _readCharacteristic = gattCharacteristic;
                                }
                                if (_allWriteCharServices.Contains(gattChar))
                                {
                                    _writeCharacteristic = gattCharacteristic;
                                }
                            }
                            if (_readCharacteristic!=null && _writeCharacteristic != null)
                              break;
                        }
                    }
                }
            }

            if (_readCharacteristic != null)
            {
                _readCharacteristic.ValueChanged += _readCharacteristic_ValueChanged;
                GattCommunicationStatus status = AsyncHelper.RunSync(async () => await _readCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(
                                                                         GattClientCharacteristicConfigurationDescriptorValue.Notify));
            }

            while (_byteArrayQueue.TryDequeue(out byte[] _)) ;
            while (_byteQueue.TryDequeue(out byte _)) ;
        }

        private void _readCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out var data);
            if (data != null)
            {
                _byteArrayQueue.Enqueue(data);
                foreach (var b in data)
                {
                    _byteQueue.Enqueue(b);
                }
            }
        }

        public void Close()
        {
            if (_readCharacteristic != null)
            {
                _readCharacteristic.ValueChanged -= _readCharacteristic_ValueChanged;
                _readCharacteristic = null;
            }

            _writeCharacteristic = null;
            while (_byteArrayQueue.TryDequeue(out byte[] _));
            while (_byteQueue.TryDequeue(out byte _));
            SerialBTLECommunicationTools.Clear();
            _service.Dispose();
            _bluetoothLeDevice.Dispose();
        }

        public bool IsOpen => _readCharacteristic != null && _writeCharacteristic != null;

        public string ReadLine()
        {
            while (_byteQueue.TryDequeue(out byte _));
            string r = string.Empty;
            if (_byteArrayQueue.TryDequeue(out byte[] bArray))
            {
                foreach (var b in bArray)
                {
                    r = r + b + " ";
                }

            }
            return r;
        }

        public byte[] ReadByteArray()
        {
            while (_byteQueue.TryDequeue(out byte _));
            if (_byteArrayQueue.TryDequeue(out byte[] bArray))
            {
                return bArray;
            }
            
            return Array.Empty<byte>();
        }

        public int ReadByte()
        {
            while (_byteArrayQueue.TryDequeue(out byte[] _));
            if (_byteQueue.TryDequeue(out byte b))
              return b;
            return 0;
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            if (!IsOpen)
            {
                return;
            }
            var fromByteArray = CryptographicBuffer.CreateFromByteArray(buffer);
            try
            {
                AsyncHelper.RunSync(async () =>
                {
                    GattWriteResult result = await _writeCharacteristic.WriteValueWithResultAsync(fromByteArray);
                });
            }
            catch (Exception ex)
            {
                //
            }
        }

        public int ReadTimeout { get; set; }
        public void ClearBuffer()
        {
            while (_byteArrayQueue.TryDequeue(out byte[] _));
            while (_byteQueue.TryDequeue(out byte _));
        }

        private string FormatValueByPresentation(IBuffer buffer, GattPresentationFormat format)
        {
            // BT_Code: For the purpose of this sample, this function converts only UInt32 and
            // UTF-8 buffers to readable text. It can be extended to support other formats if your app needs them.
            byte[] data;
            CryptographicBuffer.CopyToByteArray(buffer, out data);
            if (format != null)
            {
                if (format.FormatType == GattPresentationFormatTypes.UInt32 && data.Length >= 4)
                {
                    return BitConverter.ToInt32(data, 0).ToString();
                }
                else if (format.FormatType == GattPresentationFormatTypes.Utf8)
                {
                    try
                    {
                        return Encoding.UTF8.GetString(data);
                    }
                    catch (ArgumentException)
                    {
                        return "(error: Invalid UTF-8 string)";
                    }
                }
                else
                {
                    // Add support for other format types as needed.
                    return "Unsupported format: " + CryptographicBuffer.EncodeToHexString(buffer);
                }
            }
            else if (data != null)
            {
                // We don't know what format to use. Let's try some well-known profiles, or default back to UTF-8.
                if (_readCharacteristic.Uuid.Equals(GattCharacteristicUuids.HeartRateMeasurement))
                {
                    try
                    {
                        return "Heart Rate: " + ParseHeartRateValue(data).ToString();
                    }
                    catch (ArgumentException)
                    {
                        return "Heart Rate: (unable to parse)";
                    }
                }
                else if (_readCharacteristic.Uuid.Equals(GattCharacteristicUuids.BatteryLevel))
                {
                    try
                    {
                        // battery level is encoded as a percentage value in the first byte according to
                        // https://www.bluetooth.com/specifications/gatt/viewer?attributeXmlFile=org.bluetooth.characteristic.battery_level.xml
                        return "Battery Level: " + data[0].ToString() + "%";
                    }
                    catch (ArgumentException)
                    {
                        return "Battery Level: (unable to parse)";
                    }
                }
                // This is our custom calc service Result UUID. Format it like an Int
                else if (_readCharacteristic.Uuid.Equals(ResultCharacteristicUuid))
                {
                    return BitConverter.ToInt32(data, 0).ToString();
                }
                // No guarantees on if a characteristic is registered for notifications.
                else if (_readCharacteristic != null)
                {
                    // This is our custom calc service Result UUID. Format it like an Int
                    if (_readCharacteristic.Uuid.Equals(ResultCharacteristicUuid))
                    {
                        return BitConverter.ToInt32(data, 0).ToString();
                    }
                }
                else
                {
                    try
                    {
                        return "Unknown format: " + Encoding.UTF8.GetString(data);
                    }
                    catch (ArgumentException)
                    {
                        return "Unknown format";
                    }
                }
            }
            else
            {
                return "Empty data received";
            }
            return "Unknown format";
        }
    }
}
