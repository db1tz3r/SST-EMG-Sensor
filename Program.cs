using System;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            List<int> sensorIds = new List<int> {5}; // Sensoren angeben
            DataCapture dataCapture = new DataCapture(sensorIds);
            dataCapture.Initialize();

            DataSender dataSender = new DataSender("localhost", 12345);

            // Event-Handler anpassen (Empfängt jetzt einen String statt Dictionary)
            dataCapture.OnDataCaptured += async (string dataString) =>
            {
                await dataSender.SendDataAsync(dataString); // Direkt den String senden
                Console.WriteLine("Daten gesendet: " + dataString);
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
