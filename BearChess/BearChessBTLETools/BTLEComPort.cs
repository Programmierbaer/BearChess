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
        private readonly string _deviceId;
        private BluetoothLEDevice _bluetoothLeDevice = null;
        private GattCharacteristic _writeCharacteristic = null;
        private GattCharacteristic _readCharacteristic = null;
        private GattDeviceService _service = null;
        private ConcurrentQueue<byte[]> _bytesQueue = new ConcurrentQueue<byte[]>();

        private static readonly Guid ResultCharacteristicUuid = Guid.Parse("6E400003-B5A3-F393-E0A9-E50E24DCCA9E");

        #region Error Codes
        readonly int E_BLUETOOTH_ATT_WRITE_NOT_PERMITTED = unchecked((int)0x80650003);
        readonly int E_BLUETOOTH_ATT_INVALID_PDU = unchecked((int)0x80650004);
        readonly int E_ACCESSDENIED = unchecked((int)0x80070005);
        readonly int E_DEVICE_NOT_AVAILABLE = unchecked((int)0x800710df); // HRESULT_FROM_WIN32(ERROR_DEVICE_NOT_AVAILABLE)
        #endregion

        public string PortName => "BTE";

        public BTLEComPort(string deviceId)
        {
            _deviceId = deviceId;
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

        public  void Open()
        {


            _bluetoothLeDevice = AsyncHelper.RunSync(async () => await BluetoothLEDevice.FromIdAsync(_deviceId));
            if (_bluetoothLeDevice != null)
            {
                GattDeviceServicesResult result = AsyncHelper.RunSync(async () => await _bluetoothLeDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached));
                if (result.Status == GattCommunicationStatus.Success)
                {
                    var services = result.Services;
                    foreach (GattDeviceService service in services)
                    {
                      
                        if (service.Uuid.ToString("D")
                                   .Equals("6E400001-B5A3-F393-E0A9-E50E24DCCA9E", StringComparison.OrdinalIgnoreCase))
                        {
                            _service = service;

                            GattCharacteristicsResult readOnlyList = AsyncHelper.RunSync(async () => await service.GetCharacteristicsAsync(
                                BluetoothCacheMode.Uncached));
                            foreach (var gattCharacteristic in readOnlyList.Characteristics)
                            {
                                if (gattCharacteristic.Uuid.ToString("D").Equals(
                                        "6E400002-B5A3-F393-E0A9-E50E24DCCA9E", StringComparison.OrdinalIgnoreCase))
                                {
                                    _writeCharacteristic = gattCharacteristic;
                                }
                                if (gattCharacteristic.Uuid.ToString("D").Equals(
                                        "6E400003-B5A3-F393-E0A9-E50E24DCCA9E", StringComparison.OrdinalIgnoreCase))
                                {
                                    _readCharacteristic = gattCharacteristic;
                                }
                            }
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

            while (_bytesQueue.TryDequeue(out byte[] _)) ;
        }

        private void _readCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            CryptographicBuffer.CopyToByteArray(args.CharacteristicValue, out var data);
            if (data != null)
            {
                _bytesQueue.Enqueue(data);
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
            while (_bytesQueue.TryDequeue(out byte[] _)) ;
            SerialBTLECommunicationTools.Clear();
            _service.Dispose();
            _bluetoothLeDevice.Dispose();
        }

        public bool IsOpen => _readCharacteristic != null && _writeCharacteristic != null;

        public string ReadLine()
        {
            string r = string.Empty;
            if (_bytesQueue.TryDequeue(out byte[] bArray))
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
            if (_bytesQueue.TryDequeue(out byte[] bArray))
            {
                return bArray;
            }

            return Array.Empty<byte>();
        }

        public int ReadByte()
        {
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
