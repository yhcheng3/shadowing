using UnityEngine;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System;

/// <summary>
/// Sends orientation data to a server at a URL specified by ControlURLHandler, at a specified frequency. 
/// For manual mode, the direction is changed by ButtonHandler.cs.
/// </summary>
public class OrientationSender : MonoBehaviour
{
    public string serverUrl = "";
    public bool isManual = false;
    public Direction direction = Direction.nil;
    private HttpClient httpClient;
    private float sendFrequency = 0.04f; // Lower frequency in seconds
    private float timer = 0f;

    void Start()
    {
        httpClient = new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true, UseDefaultCredentials = true });
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= sendFrequency)
        {
            timer = 0f;

            if (isManual)
            {
                // Create JSON payload
                string jsonPayload = JsonUtility.ToJson(new DirectionData(isManual, direction));

                // Send data to server
                SendData(jsonPayload);
            }
            else
            {
                // Get the device's rotation
                Quaternion rotation = Camera.main.transform.rotation;

                // Convert the rotation to Euler angles
                Vector3 eulerAngles = rotation.eulerAngles;

                // Calculate roll, pitch, and yaw
                float roll = eulerAngles.z;
                float pitch = eulerAngles.x;
                float yaw = eulerAngles.y;

                // Create JSON payload
                string jsonPayload = JsonUtility.ToJson(new OrientationData(isManual, roll, pitch, yaw));

                // Send data to server
                SendData(jsonPayload);
            }
        }
    }

    async void SendData(string jsonPayload)
    {
        try
        {
            serverUrl = serverUrl.TrimEnd('\u200B', '\u200C', '\u200D', ' ', '\n'); // Remove zero width space, space, and enter from end of serverUrl

            StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            
            Debug.Log(serverUrl);
            Debug.Log(jsonPayload);
            
            HttpResponseMessage response = await httpClient.PostAsync(serverUrl, content);

            if (response is not null) 
            {
                Debug.Log($"HTTP Error Code: {(int)response.StatusCode} {response.StatusCode}");
                Debug.Log($"Content: {await response.Content.ReadAsStringAsync()}");
            }

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"Request error message: {e.Message}");
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

    [System.Serializable]
    public class OrientationData
    {
        public bool isManual;
        public float roll;
        public float pitch;
        public float yaw;

        public OrientationData(bool isManual, float roll, float pitch, float yaw)
        {
            this.isManual = isManual;
            this.roll = roll;
            this.pitch = pitch;
            this.yaw = yaw;
        }
    }

    public class DirectionData
    {
        public bool isManual;
        public Direction direction;

        public DirectionData(bool isManual, Direction direction)
        {
            this.isManual = isManual;
            this.direction = direction;
        }
    }
    public enum Direction
    {
        up,
        down,
        left,
        right,
        nil
    }
}
