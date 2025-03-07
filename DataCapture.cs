using System;
using Waveplus.DaqSys;
using Waveplus.DaqSysInterface;
using System.Collections.Generic;
using CyUSB;

public class DataCapture
{
    private DaqSystem daqSystem;
    private List<int> sensorIds;

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
            Console.WriteLine("Kein Gerät gefunden.");
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

        // **Abtastrate auf 1000 Hz setzen**
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
        daqSystem.StartCapturing(DataAvailableEventPeriod.ms_10);
        Console.WriteLine("Datenerfassung gestartet.");

        daqSystem.DataAvailable += (sender, e) =>
        {
            int samplesCount = e.Samples.GetLength(1); // Anzahl der Messzeitpunkte
            int maxSensors = e.Samples.GetLength(0);  // Anzahl der tatsächlich gelieferten Sensorwerte

            for (int j = 0; j < samplesCount; j++) // Über alle Messzeitpunkte iterieren
            {
                List<string> sensorValues = new List<string>();

                foreach (int sensorId in sensorIds) // Durch alle Sensoren in der Liste iterieren
                {
                    int sensorIndex = sensorId - 1; // Berechnet den Index basierend auf Sensor-ID

                    if (sensorIndex >= 0 && sensorIndex < maxSensors)
                    {
                        sensorValues.Add(e.Samples[sensorIndex, j].ToString()); // Werte sammeln
                    }
                    else
                    {
                        sensorValues.Add("N/A"); // Falls Sensor nicht existiert oder keine Werte liefert
                    }
                }

                string finalOutput = string.Join(" | ", sensorValues);
                OnDataCaptured?.Invoke(finalOutput);
            }
        };
    }

    public void StopCapture()
    {
        daqSystem.StopCapturing();
        Console.WriteLine("Datenerfassung gestoppt.");
    }
}
