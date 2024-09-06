using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// This script handles changes to the URL input field and displays an error message for invalid URLs.
/// It also updates the server URL in the OrientationSender component, which determines where orientation data is sent.
/// </summary>
public class ControlURLHandler : MonoBehaviour
{
    public TMP_InputField urlInputField;
    public TextMeshProUGUI invalidUrlDisplay;
    public OrientationSender orientationSender;

    private void Start()
    {
        urlInputField.onEndEdit.AddListener(OnInputFieldEndEdit);
        urlInputField.onSubmit.AddListener(OnInputFieldEndEdit);
        urlInputField.onDeselect.AddListener(OnInputFieldEndEdit);
        orientationSender.serverUrl = urlInputField.text;
    }

    private void OnInputFieldEndEdit(string url)
    {
        if (IsValidUrl(url))
        {
            Debug.Log("URL: " + url);
            orientationSender.serverUrl = url;
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
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri uriResult))
        {
            return (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
        return false;
    }
}
