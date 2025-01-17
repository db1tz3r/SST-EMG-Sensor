using System;
using Waveplus.DaqSys;
using Waveplus.DaqSysInterface;
using CyUSB;

public class DataCapture
{
    private DaqSystem daqSystem;

    public event Action<float[]> OnDataCaptured;

    public DataCapture()
    {
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

        // Sensor 1 aktivieren
        int sensorId = 1;
        int installedSensors = daqSystem.InstalledSensors;
        if (sensorId <= installedSensors)
        {
            daqSystem.EnableSensor(sensorId);
            Console.WriteLine($"Sensor {sensorId} aktiviert.");
        }
        else
        {
            throw new Exception("Sensor 1 ist nicht installiert.");
        }

        // Datenerfassung konfigurieren
        ICaptureConfiguration captureConfig = new CaptureConfiguration
        {
            SamplingRate = SamplingRate.Hz_2000, // Beispiel: 2000 Hz
            ExternalTriggerEnabled = false
        };
        daqSystem.ConfigureCapture(captureConfig);
        Console.WriteLine("Datenerfassung konfiguriert.");
    }

    public void StartCapture()
    {
        daqSystem.StartCapturing(DataAvailableEventPeriod.ms_100);
        Console.WriteLine("Datenerfassung gestartet.");

        daqSystem.DataAvailable += (sender, e) =>
        {
            float[] samples = new float[e.Samples.GetLength(1)];
            for (int i = 0; i < e.Samples.GetLength(1); i++)
            {
                samples[i] = e.Samples[0, i];
            }
            OnDataCaptured?.Invoke(samples);
        };
    }

    public void StopCapture()
    {
        daqSystem.StopCapturing();
        Console.WriteLine("Datenerfassung gestoppt.");
    }
}
