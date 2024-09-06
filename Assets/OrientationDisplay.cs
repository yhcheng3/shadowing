using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro namespace

/// <summary>
/// This script is responsible for displaying the orientation of the HoloLens in the user interface.
/// It updates the UI text elements with the roll, pitch, and yaw values of the device's rotation.
/// </summary>
public class OrientationDisplay : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI rollText;
    [SerializeField] private TextMeshProUGUI pitchText;
    [SerializeField] private TextMeshProUGUI yawText;

    void Update()
    {
        // Get the device's rotation
        Quaternion rotation = Camera.main.transform.rotation;

        // Convert the rotation to Euler angles
        Vector3 eulerAngles = rotation.eulerAngles;

        // Calculate roll, pitch, and yaw
        float roll = eulerAngles.z;
        float pitch = eulerAngles.x;
        float yaw = eulerAngles.y;

        // Update the UI text elements
        rollText.text = $"Local Roll: {roll:F2}";
        pitchText.text = $"Local Pitch: {pitch:F2}";
        yawText.text = $"Local Yaw: {yaw:F2}";
    }
}
