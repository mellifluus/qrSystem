using UnityEngine;
using System.Collections.Generic;
using System.IO.Ports;
using Microsoft.Win32;

public class FindScannerInCOMPorts : MonoBehaviour
{

    #pragma warning disable 649

    [SerializeField]
    private string VID, PID, friendlyName;

    #pragma warning restore 649

    public string AutodetectScannerPort()
    {
        List<string> comports = new List<string>();
        RegistryKey rk1 = Registry.LocalMachine;
        RegistryKey rk2 = rk1.OpenSubKey("SYSTEM\\CurrentControlSet\\Enum\\USB\\VID_" + VID + "&PID_" + PID);
        string temp;
        foreach (string s2 in rk2.GetSubKeyNames())
        {
            RegistryKey rk3 = rk2.OpenSubKey(s2);
            if ((temp = (string)rk3.GetValue("FriendlyName")) != null && temp.Contains(friendlyName))
            {
                RegistryKey rk4 = rk3.OpenSubKey("Device Parameters");
                if (rk4 != null && (temp = (string)rk4.GetValue("PortName")) != null)
                {
                    comports.Add(temp);
                }
            }
        }
        if (comports.Count > 0)
        {
            foreach (string s in SerialPort.GetPortNames())
            {
                if (comports.Contains(s))
                {
                    return s;
                }
            }
        }
        return null;
    }
}