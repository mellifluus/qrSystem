using UnityEngine;
using System;
using System.IO.Ports;
using System.Threading;
using System.Linq;
using RenderHeads.Media.AVProVideo;

public class BarcodeScanner : MonoBehaviour
{
    private Thread readerThread;
    private SerialPort barcodeStream;

    #pragma warning disable 649

    [HideInInspector]
    public int currentQR;
    [SerializeField]
    private int timeoutMS;

    #pragma warning restore 649

    private void Start()
    {
        currentQR = -1;

        barcodeStream = new SerialPort(gameObject.GetComponent<FindScannerInCOMPorts>().AutodetectScannerPort(), 115200, Parity.None, 8, StopBits.One);
        barcodeStream.ReadTimeout = timeoutMS;
        barcodeStream.Open();

        readerThread = new Thread(new ThreadStart(QRMonitor));
        readerThread.Start();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
            barcodeStream.Write("^_^SCAN.");

        if(Input.GetKeyDown(KeyCode.F2))
            barcodeStream.Write("^_^SLEEP.");

        if(Input.GetKeyDown(KeyCode.F3))
            barcodeStream.Write("^_^BEPSUC0.");
    }

    private void QRMonitor()
    {
        barcodeStream.Write("^_^SCAN.");
        while (readerThread.IsAlive && barcodeStream.IsOpen)
        {
            try
            {
                string QR = barcodeStream.ReadTo("\n");
                string strippedQR = new string(QR.Where(c => !char.IsControl(c)).ToArray());

                if(Int32.TryParse(strippedQR, out int tmpQR))
                {
                    if(tmpQR != currentQR)
                    {
                        currentQR = tmpQR;
                        //Debug.Log("Detected QR: " + currentQR + ".");
                    }
                }
            }
            catch(Exception e)
            {
                if(e is TimeoutException)
                {
                    if(currentQR != -1)
                    {
                        currentQR = -1;
                        //Debug.Log("QR removed.");
                    }
                }
                else if(e is ThreadAbortException)
                {
                    Debug.LogWarning("Closing reader thread.");
                }  
                else
                {
                    Debug.LogError(e);
                }      
            }
        }
    }

    private bool isValidQR(string s)
    {
        return s.All(c => c >= '0' && c <= '9') && s.Length > 0;
    }

    void OnDisable()
    {
        if(readerThread != null)
            readerThread.Abort();
        
        if(barcodeStream != null)
            barcodeStream.Close();
    }

    void OnApplicationQuit()
    {   
        if(readerThread != null)
            readerThread.Abort();
        
        if(barcodeStream != null)
        {
            barcodeStream.Write("^_^SLEEP.");
            barcodeStream.Close();
        }
    }
}