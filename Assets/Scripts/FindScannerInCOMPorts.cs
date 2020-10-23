using UnityEngine;
using System.Collections.Generic;
using System.IO.Ports;
using Microsoft.Win32;

public class FindScannerInCOMPorts : MonoBehaviour
{
    #pragma warning disable 649
    [SerializeField]
    private string VID, PID;
    #pragma warning restore 649

    public string AutodetectScannerPort()
    {
        List<string> comPorts = new List<string>();
        RegistryKey baseKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\USB\\VID_" + VID + "&PID_" + PID);

        foreach (string subKey in baseKey.GetSubKeyNames())
        {
            RegistryKey paramKey = baseKey.OpenSubKey(subKey).OpenSubKey("Device Parameters");
            if (paramKey != null)
            {
                string tmpPort = (string)paramKey.GetValue("PortName");

                if(tmpPort != null)
                    comPorts.Add(tmpPort);
            }
        }

        if (comPorts.Count > 0)
            foreach (string s in SerialPort.GetPortNames())
                if (comPorts.Contains(s))
                    return s;

        return null;
    }
}