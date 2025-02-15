using System;
using System.IO.Ports;
using www.SoLaNoSoft.com.BearChessToolsCore;

namespace BearChessCoreConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var serialPort = new SerialPort("COM8", 38400, Parity.None) { ReadTimeout = 1000};
            try
            {
               // var btComPort = SerialCommunicationTools.GetBTComPort();
               
                
                serialPort.Open();
                if (serialPort.IsOpen)
                {
                    Console.WriteLine("Is aleady open");
                    Console.ReadLine();
                    return;
                }


                serialPort.Open();
                if (serialPort.IsOpen)
                {
                    Console.WriteLine("Bingo");




                }
                else
                {
                    Console.WriteLine("Failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed: {ex.Message}");
            }

            Console.ReadLine();
            return;

        }
    }
}
