using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class DataSender
{
    private string serverAddress;
    private int serverPort;
    private TcpClient client;
    private NetworkStream stream;

    public DataSender(string serverAddress, int serverPort)
    {
        this.serverAddress = serverAddress;
        this.serverPort = serverPort;
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        try
        {
            client = new TcpClient(serverAddress, serverPort);
            stream = client.GetStream();
            Console.WriteLine("TCP-Verbindung hergestellt.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Verbinden mit dem Server: {ex.Message}");
        }
    }

    public async Task SendDataAsync(string data)
    {
        try
        {
            if (client == null || !client.Connected)
            {
                Console.WriteLine("Verbindung verloren. Versuche Neuverbindung...");
                InitializeConnection();
            }

            if (stream != null && client.Connected)
            {
                byte[] messageBytes = Encoding.UTF8.GetBytes(data + "\n");
                await stream.WriteAsync(messageBytes, 0, messageBytes.Length);
            }
            else
            {
                Console.WriteLine("Keine aktive Verbindung.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Senden der Daten: {ex.Message}");
        }
    }

    public void CloseConnection()
    {
        try
        {
            stream?.Close();
            client?.Close();
            Console.WriteLine("TCP-Verbindung geschlossen.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Schließen der Verbindung: {ex.Message}");
        }
    }
}
