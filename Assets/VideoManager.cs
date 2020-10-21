using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections;
using RenderHeads.Media.AVProVideo;

public class VideoManager : MonoBehaviour
{
    #pragma warning disable 649

    [SerializeField]
    private GameObject videoPrefab;

    #pragma warning restore 649

    private MediaPlayer[] videos;
    private BarcodeScanner scanner;
    private int currentlyPlaying = 0;

    private void Awake() 
    {
        scanner = GameObject.Find("BarcodeScanner").GetComponent<BarcodeScanner>();
        initVideos(); 
    }

    private void Update() 
    {
        if(currentlyPlaying != scanner.currentQR && scanner.currentQR != 0)
            swapVideos(currentlyPlaying, scanner.currentQR);
    }

    private void initVideos()
    {
        string[] videoNames = (from file in new DirectoryInfo(Application.streamingAssetsPath).GetFiles("*.mp4") select file.Name).ToArray();

        videos = new MediaPlayer[videoNames.Length];

        for(int i = 0; i < videoNames.Length; i++)
        {
            GameObject currentObject = Instantiate(videoPrefab, gameObject.transform);
            MediaPlayer currentPlayer = currentObject.GetComponent<MediaPlayer>();
            currentObject.name = "VideoPlayer_" + i.ToString("00");
            currentPlayer.Events.AddListener(gameObject.GetComponent<VideoEventManager>().OnVideoEvent);
            currentPlayer.OpenVideoFromFile(MediaPlayer.FileLocation.RelativeToStreamingAssetsFolder, videoNames[i], false);
            videos[i] = currentPlayer; 
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
