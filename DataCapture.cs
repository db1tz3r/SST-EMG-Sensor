using System;
using Waveplus.DaqSys;
using Waveplus.DaqSysInterface;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using CyUSB;

public class DataCapture
{
    private DaqSystem daqSystem;
    private List<int> sensorIds;
    private TcpClient client;
    private NetworkStream stream;

    public event Action<string>? OnDataCaptured;

    public DataCapture(List<int> sensorIds)
    {
        this.sensorIds = sensorIds;
        daqSystem = new DaqSystem();
        Console.WriteLine("DaqSystem initialisiert.");
    }

    public void Initialize()
    {
        USBDeviceList usbDevices = new USBDeviceList(CyConst.DEVICES_CYUSB);
        CyUSBDevice myDevice = usbDevices[0x04B4, 0x1002] as CyUSBDevice;

        if (myDevice != null)
        {
            Console.WriteLine("Gerät gefunden.");
        }
        else
        {
            throw new Exception("Gerät nicht verbunden.");
        }

        int installedSensors = daqSystem.InstalledSensors;
        foreach (int sensorId in sensorIds)
        {
            if (sensorId <= installedSensors)
            {
                daqSystem.EnableSensor(sensorId);
                Console.WriteLine($"Sensor {sensorId} aktiviert.");
            }
            else
            {
                throw new Exception($"Sensor {sensorId} ist nicht installiert.");
            }
        }

        ICaptureConfiguration captureConfig = new CaptureConfiguration
        {
            SamplingRate = SamplingRate.Hz_2000,
            ExternalTriggerEnabled = false
        };
        daqSystem.ConfigureCapture(captureConfig);
        Console.WriteLine("Datenerfassung konfiguriert.");
    }

    public void StartCapture()
    {
        // **Nur einmal die TCP-Verbindung aufbauen**
        try
        {
            client = new TcpClient("127.0.0.1", 12345);
            stream = client.GetStream();
            Console.WriteLine("TCP-Verbindung hergestellt.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Verbinden mit Socket: {ex.Message}");
            return;
        }

        daqSystem.StartCapturing(DataAvailableEventPeriod.ms_10);
        Console.WriteLine("Datenerfassung gestartet.");

        daqSystem.DataAvailable += (sender, e) =>
        {
            int samplesCount = e.Samples.GetLength(1); // Anzahl der Messzeitpunkte

            for (int j = 0; j < samplesCount; j++) // Über alle Messzeitpunkte iterieren
            {
                List<string> sensorValues = new List<string>();

                for (int i = 0; i < sensorIds.Count; i++) // Über alle Sensoren iterieren
                {
                    sensorValues.Add(e.Samples[i, j].ToString()); // Werte sammeln
                }

                string finalOutput = string.Join(" | ", sensorValues);
                OnDataCaptured?.Invoke(finalOutput);

                // **Daten an den Socket senden**
                SendData(finalOutput);
            }
        };
    }

    private void SendData(string data)
    {
        try
        {
            if (stream != null && client.Connected)
            {
                byte[] message = Encoding.ASCII.GetBytes(data + "\n");
                stream.Write(message, 0, message.Length);
                Console.WriteLine($"Daten gesendet: {data}");
            }
            else
            {
                Console.WriteLine("Socket nicht verbunden.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Senden der Daten: {ex.Message}");
        }
    }

    public void StopCapture()
    {
        daqSystem.StopCapturing();
        Console.WriteLine("Datenerfassung gestoppt.");

        // **Socket Verbindung sauber schließen**
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
