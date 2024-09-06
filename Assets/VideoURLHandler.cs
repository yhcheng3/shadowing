using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// This script handles changes to the URL input field and displays an error message for invalid URLs.
/// It also updates the MRL in the VLCStreamer component, which determines where to retrieve the video stream.
/// </summary>
public class VideoURLHandler : MonoBehaviour
{
    public TMP_InputField urlInputField;
    // public MjpegTexture mjpegTexture;
    public TextMeshProUGUI invalidUrlDisplay;
    public VLCStreamer VLCStreamer;

    private void Start()
    {
        urlInputField.onEndEdit.AddListener(OnInputFieldEndEdit);
        urlInputField.onSubmit.AddListener(OnInputFieldEndEdit);
        urlInputField.onDeselect.AddListener(OnInputFieldEndEdit);
        VLCStreamer.url = urlInputField.text;
    }

    private void OnInputFieldEndEdit(string url)
    {
        if (IsValidUrl(url))
        {
            Debug.Log("URL: " + url);
            VLCStreamer.url = url;
            VLCStreamer.Restart();
            invalidUrlDisplay.text = "";
        }
        else
        {
            Debug.Log("Invalid URL: " + url);
            invalidUrlDisplay.text = "Invalid URL!";
        }
    }

    public static bool IsValidUrl(string url)
    {
        Debug.Log(url);
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
        {
            Debug.Log(uriResult.ToString());
            return (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == "rtsp");
        }
        return false;
    }
}
