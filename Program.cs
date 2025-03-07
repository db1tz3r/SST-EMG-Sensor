using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        try
        {
            List<int> sensorIds = new List<int> {1,2,4}; 
            DataCapture dataCapture = new DataCapture(sensorIds);
            dataCapture.Initialize();

            DataSender dataSender = new DataSender("127.0.0.1", 12345);

            dataCapture.OnDataCaptured += async (string dataString) =>
            {
                await dataSender.SendDataAsync(dataString);
                Console.WriteLine("Daten gesendet: " + dataString);
            };

            dataCapture.StartCapture();

            Console.WriteLine("Drücke Enter, um das Programm zu beenden.");
            Console.ReadLine();

            dataCapture.StopCapture();
            dataSender.CloseConnection();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
        }
    }
}