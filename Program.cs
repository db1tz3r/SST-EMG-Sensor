using System;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            // Initialisiere die Datenerfassung
            DataCapture dataCapture = new DataCapture();
            dataCapture.Initialize();

            // Initialisiere die Datenübertragung
            DataSender dataSender = new DataSender("localhost", 12345);

            // Event-Handler für die Datenübertragung
            dataCapture.OnDataCaptured += async (float[] data) =>
            {
                await dataSender.SendDataAsync(data);
                Console.WriteLine("Daten gesendet.");
            };

            // Starte die Datenerfassung
            dataCapture.StartCapture();
            Console.WriteLine("Drücke die Eingabetaste, um das Programm zu beenden.");
            Console.ReadLine();

            // Stoppe die Datenerfassung
            dataCapture.StopCapture();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler: {ex.Message}");
        }
    }
}
