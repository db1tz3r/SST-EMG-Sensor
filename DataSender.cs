using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class DataSender
{
    private string serverAddress;
    private int serverPort;

    public DataSender(string serverAddress, int serverPort)
    {
        this.serverAddress = serverAddress;
        this.serverPort = serverPort;
    }

    public async Task SendDataAsync(float[] data)
    {
    try
    {
        using (TcpClient client = new TcpClient(serverAddress, serverPort))
        using (NetworkStream stream = client.GetStream())
        {
            foreach (float value in data)
            {
                // Verwende Punkt als Dezimaltrennzeichen
                string message = value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + "\n";
                byte[] messageBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Fehler beim Senden der Daten: {ex.Message}");
    }
}

}
