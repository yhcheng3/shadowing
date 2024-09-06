using UnityEngine;
using LibVLCSharp;
using System;
using System.Diagnostics.Tracing;

/// <summary>
/// Streams video from an MRL using VLC.
/// </summary>
public class VLCStreamer : MonoBehaviour
{
    public string url = "";
    public Renderer targetRenderer; // The renderer to display the video

    private Texture2D tex;
    private LibVLC libvlc { get; set; }
    private MediaPlayer mediaPlayer;

    void Start()
    {
        Core.Initialize(UnityEngine.Application.dataPath);

        libvlc = new LibVLC(enableDebugLogs: true);
        mediaPlayer = new MediaPlayer(libvlc);
        tex = new Texture2D(1920, 1080);

        url = url.TrimEnd('\u200B', '\u200C', '\u200D', ' ', '\n'); // Remove zero width space, space, and enter from end of url
        var media = new Media(new Uri(url));

        while (tex == null || mediaPlayer == null || media == null)
        {
            // Wait until both tex and mediaPlayer are allocated
        }

        mediaPlayer.Play(media);
        targetRenderer.material.mainTexture = tex;
    }

    void Update()
    {
        if (mediaPlayer != null && tex != null)
        {
            try
            {
                var texptr = mediaPlayer.GetTexture(1920, 1080, out _);
                tex.UpdateExternalTexture(texptr);
            }
            catch (Exception e)
            {
                Debug.LogError("Error updating texture: " + e.Message);
                Debug.LogError($"Data: {e.Data}");
                Debug.LogError($"Helplink: {e.HelpLink}");
                Debug.LogError($"HResult: {e.HResult}");
                Debug.LogError($"InnerException: {e.InnerException}");
                Debug.LogError($"Source: {e.Source}");
                Debug.LogError($"StackTrace: {e.StackTrace}");
                Debug.LogError($"TargetSite: {e.TargetSite}");
                Debug.LogError(message: $"ToString: {e.ToString()}");

                Exception innerException = e.InnerException;

                while (innerException != null)
                {
                    Debug.LogError($"InnerException: {innerException.Message}");
                    Debug.LogError($"Source: {innerException.Source}");
                    Debug.LogError($"StackTrace: {innerException.StackTrace}");
                    Debug.LogError($"TargetSite: {innerException.TargetSite}");
                    Debug.LogError(message: $"ToString: {innerException.ToString()}");

                    innerException = innerException.InnerException;
                }
            }
        }
    }

    public void Restart()
    {
        // De-allocation
        mediaPlayer.Stop();
        libvlc = null;
        mediaPlayer = null;
        tex = null;

        // Check if the url property has changed
        if (Application.isPlaying && !string.IsNullOrEmpty(url))
        {
            Start();
        }
    }
}
