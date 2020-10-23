using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using RenderHeads.Media.AVProVideo;

public class VideoManager : MonoBehaviour
{
    #pragma warning disable 649

    [SerializeField]
    private GameObject videoPrefab;

    #pragma warning restore 649

    private Regex videoRegex = new Regex(@"^\d+_[a-z0-9\s]+\.(?:mp4|avi|mov|webm|mkv)$", RegexOptions.IgnoreCase);
    private Regex loopRegex = new Regex(@"^loop_[a-z0-9\s]+\.(?:mp4|avi|mov|webm|mkv)$", RegexOptions.IgnoreCase);
    private Dictionary<int, MediaPlayer> videos = new Dictionary<int, MediaPlayer>();
    private int currentlyPlaying = -1;
    private bool loopMode = false;
    private BarcodeScanner scanner;

    private void Awake() 
    {
        scanner = GameObject.Find("BarcodeScanner").GetComponent<BarcodeScanner>();
        initVideos(); 
        debugLoaded();
    }

    private void Update() 
    {
        // if(currentlyPlaying != scanner.currentQR && scanner.currentQR != -1)
        if(currentlyPlaying != scanner.currentQR)
            swapVideos(currentlyPlaying, scanner.currentQR);
    }

    private void initVideos()
    {  
        List<string> fileNames = (from file in new DirectoryInfo(Application.streamingAssetsPath).GetFiles() select file.Name).ToList();

        foreach(string file in fileNames)
        {
            if(loopRegex.IsMatch(file))
            {
                GameObject currentObject = Instantiate(videoPrefab, gameObject.transform);
                MediaPlayer currentPlayer = currentObject.GetComponent<MediaPlayer>();
                currentPlayer.m_Idle = true;
                videos.Add(-1, currentPlayer);
                currentObject.name = "Loop_" + file.Split('_')[1];
                currentPlayer.Events.AddListener(gameObject.GetComponent<VideoEventManager>().OnVideoEvent);
                currentPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, file, true);
                loopMode = true;
                break;
            }
        }

        if(!loopMode)
            videos.Add(-1, null);

        for(int i = 0; i < fileNames.Count; i++)
        {
            if(videoRegex.IsMatch(fileNames[i]))
            {
                List<string> tmpSplit = fileNames[i].Split('_').ToList();
                if(Int32.TryParse(tmpSplit[0], out int qrValue))
                {
                    GameObject currentObject = Instantiate(videoPrefab, gameObject.transform);
                    MediaPlayer currentPlayer = currentObject.GetComponent<MediaPlayer>();
                    videos.Add(qrValue, currentPlayer);
                    currentObject.name = "VideoPlayer_" + tmpSplit[1];
                    currentPlayer.Events.AddListener(gameObject.GetComponent<VideoEventManager>().OnVideoEvent);
                    currentPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, fileNames[i], false);
                }       
            } 
        }
    }

    public void videoIsFinished()
    {
        if(scanner.currentQR == currentlyPlaying)
        {
            if(videos.TryGetValue(currentlyPlaying, out MediaPlayer cur))
                cur.Rewind(false);
        }    
        else
        {
            if(scanner.currentQR != -1)
                swapVideos(currentlyPlaying, scanner.currentQR);
            else
                swapVideos(currentlyPlaying, -1);
        }
    }

    private void swapVideos(int oldVideo, int newVideo)
    {
        if(videos.TryGetValue(oldVideo, out MediaPlayer oldM) && videos.TryGetValue(newVideo, out MediaPlayer newM))
        {
            if(oldM != null)
            {
                oldM.Rewind(true);
                oldM.gameObject.SetActive(false);
            }
            if(newM != null)
            {
                newM.gameObject.SetActive(true); 
                newM.Play();  
            }
            currentlyPlaying = newVideo;   
        }
        else
        {
            Debug.LogError("Old: " + oldVideo + " New: " + newVideo + " ERROR");
        } 
    }

    private void debugLoaded()
    {
        Debug.LogWarning("Initialized videos:");
        foreach(KeyValuePair<int, MediaPlayer> t in videos)
        {
            if(t.Key == -1)
            {
                if(t.Value != null)
                    Debug.LogWarning("IDLE: " + t.Value.gameObject.name.Split('_')[1]);
            }
            else
            {
                Debug.LogWarning("QRValue: " + t.Key + " VideoName: " + t.Value.gameObject.name.Split('_')[1]);
            }
        }
    }
}
