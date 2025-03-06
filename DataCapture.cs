using System;
using Waveplus.DaqSys;
using Waveplus.DaqSysInterface;
using System.Collections.Generic;
using System.Linq;
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
    daqSystem.StartCapturing(DataAvailableEventPeriod.ms_100);
    Console.WriteLine("Datenerfassung gestartet.");

    daqSystem.DataAvailable += (sender, e) =>
    {
        List<string> sensorDataList = new List<string>();
        
        for (int i = 0; i < sensorIds.Count; i++)
        {
            string sensorData = string.Join(".", Enumerable.Range(0, e.Samples.GetLength(1)).Select(j => e.Samples[i, j].ToString()));
            sensorDataList.Add(sensorData);
        }
        
        string output = string.Join(" | ", sensorDataList);
        OnDataCaptured?.Invoke(output);
    };
    }



    public void StopCapture()
    {
        daqSystem.StopCapturing();
        Console.WriteLine("Datenerfassung gestoppt.");
    }
}