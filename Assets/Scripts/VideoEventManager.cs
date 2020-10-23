using UnityEngine;
using RenderHeads.Media.AVProVideo;

public class VideoEventManager : MonoBehaviour
{
    private VideoManager manager;

    private void Awake() 
    {
        manager = GetComponent<VideoManager>();
    }
    public void OnVideoEvent(MediaPlayer mp, MediaPlayerEvent.EventType et, ErrorCode errorCode)
    {
        switch(et)
        {
            case MediaPlayerEvent.EventType.FirstFrameReady:
                if(mp.m_Idle)
                {
                    mp.Control.SetLooping(true);
                    mp.Play();
                }   
                else
                {
                    mp.gameObject.SetActive(false);
                }
                break;

            case MediaPlayerEvent.EventType.FinishedPlaying:
                //manager.videoIsFinished();
                break;

            default:
                break;
        }
    }
}
