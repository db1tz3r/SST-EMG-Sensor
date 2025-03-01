using System;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            List<int> sensorIds = new List<int> { 1, 2, 3 }; // Sensoren angeben
            DataCapture dataCapture = new DataCapture(sensorIds);
            dataCapture.Initialize();

            DataSender dataSender = new DataSender("localhost", 12345);

            dataCapture.OnDataCaptured += async (Dictionary<int, float[]> data) =>
            {
                await dataSender.SendDataAsync(data);
                Console.WriteLine("Daten gesendet.");
            };

            dataCapture.StartCapture();
            Console.WriteLine("Drücke die Eingabetaste, um das Programm zu beenden.");
            Console.ReadLine();

            dataCapture.StopCapture();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
        }
    }
}
