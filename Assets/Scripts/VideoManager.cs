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

    private Regex fileNameRegex = new Regex(@"^\d+_[a-z0-9\s]+\.(?:mp4|avi|mov|webm|mkv)$", RegexOptions.IgnoreCase);
    private Dictionary<int, MediaPlayer> videos = new Dictionary<int, MediaPlayer>();
    private int currentlyPlaying, idleQR;
    private BarcodeScanner scanner;

    private void Awake() 
    {
        scanner = GameObject.Find("BarcodeScanner").GetComponent<BarcodeScanner>();
        initVideos(); 
    }

    private void Update() 
    {
        if(currentlyPlaying != scanner.currentQR && scanner.currentQR != idleQR)
            swapVideos(currentlyPlaying, scanner.currentQR);
    }

    private void initVideos()
    {  
        List<string> fileNames = (from file in new DirectoryInfo(Application.streamingAssetsPath).GetFiles() select file.Name).ToList();

        for(int i = 0; i < fileNames.Count; i++)
        {
            if(fileNameRegex.IsMatch(fileNames[i]))
            {
                List<string> tmpSplit = fileNames[i].Split('_').ToList();
                if(Int32.TryParse(tmpSplit[0], out int qrValue))
                {
                    GameObject currentObject = Instantiate(videoPrefab, gameObject.transform);
                    MediaPlayer currentPlayer = currentObject.GetComponent<MediaPlayer>();

                    if(videos.Count == 0)
                    {
                        currentPlayer.isFirst = true;
                        idleQR = qrValue;
                        currentlyPlaying = idleQR;
                    }

                    currentObject.name = "VideoPlayer_" + tmpSplit[1];
                    currentPlayer.Events.AddListener(gameObject.GetComponent<VideoEventManager>().OnVideoEvent);
                    currentPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, fileNames[i], false);
                    videos.Add(qrValue, currentPlayer);
                }       
            } 
        }
    }

    public void videoIsFinished()
    {
        if(scanner.currentQR == currentlyPlaying)
            videos[currentlyPlaying].Rewind(false);
        else
            swapVideos(currentlyPlaying, scanner.currentQR);
    }

    private void swapVideos(int oldVideo, int newVideo)
    {
        videos[newVideo].gameObject.SetActive(true);
        videos[oldVideo].Rewind(true);
        videos[oldVideo].gameObject.SetActive(false);
        videos[newVideo].Play();
        currentlyPlaying = newVideo;
    }
}
