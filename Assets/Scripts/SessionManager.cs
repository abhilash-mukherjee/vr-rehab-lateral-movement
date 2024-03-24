using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


public class SessionManager : MonoBehaviour
{
    public delegate void SessionDataHandler(SessionData data);
    public static event SessionDataHandler onSessionStarted, onSessionPaused, onSessionResumed, onSessionEnded;
    void Start()
    {
        StartCoroutine(RecursiveCoroutine());
    }

    IEnumerator RecursiveCoroutine()
    {
        yield return new WaitForSeconds(6.0f);
        GetRequest("http://localhost:3000/current-session");
        yield return StartCoroutine(RecursiveCoroutine());
    }

    void GetRequest(string uri)
    {
        var webRequest = UnityWebRequest.Get(uri);
        webRequest.SendWebRequest().completed += (op) => { ProcessData(webRequest); };
            
    }

    void ProcessData(UnityWebRequest webRequest)
    {
        switch (webRequest.result)
        {
            case UnityWebRequest.Result.ConnectionError:
            case UnityWebRequest.Result.DataProcessingError:
                Debug.LogError(": Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.ProtocolError:
                Debug.LogError(": HTTP Error: " + webRequest.error);
                break;
            case UnityWebRequest.Result.Success:
                {
                    Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                }
                break;
        }
    }
}

public struct SessionData
{
    public string sessionCode;
    public string moduleName;
    public int duration;
    public SessionStatus status;
}

public enum SessionStatus
{
    RUNNING,
    PAUSED,
    ENDED
}