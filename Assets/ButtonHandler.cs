using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script handles the manual control panel in the UI, changing the orientation data sent to the server.
/// </summary>
public class ButtonHandler : MonoBehaviour
{
    public OrientationSender orientationSender;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnManualToggled()
    {
        Debug.Log("Manual");
        orientationSender.isManual = true;
    }
    public void OnManualUntoggled()
    {
        Debug.Log("NotManual");
        orientationSender.isManual = false;
    }
    public void OnUpToggled()
    {
        Debug.Log("Up");
        orientationSender.direction = OrientationSender.Direction.up;
    }
    public void OnDownToggled()
    {
        Debug.Log("Down");
        orientationSender.direction = OrientationSender.Direction.down;
    }
    public void OnLeftToggled()
    {
        Debug.Log("Left");
        orientationSender.direction = OrientationSender.Direction.left;
    }
    public void OnRightToggled()
    {
        Debug.Log("Right");
        orientationSender.direction = OrientationSender.Direction.right;
    }

    public void OnArrowUntoggled()
    {
        Debug.Log("Nil");
        orientationSender.direction = OrientationSender.Direction.nil;
    }
}
