using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The base class for implementing the GAMEhud API interface in Unity.
/// </summary>
public class GameHud : MonoBehaviour
{
    /// <summary>
    /// Unique identifier of the GameHUD game.  This key is supplied to you when you register the game on the GAMEhud site.  
	/// </summary>
    public string gameApiKey;
	/// <summary>
	/// Determines if you want to send Unity information logs to GAMEhud (i.e., Debug.Log )
	/// </summary>
    public bool sendUnityLogInfo = false;
	/// <summary>
	/// Determines if you want to send Unity log warnings to GAMEhud (i.e., Debug.LogWarning )
	/// </summary>
    public bool sendUnityLogWarnings = true;
	/// <summary>
	/// Determines if you want to send Unity log errors to GAMEhud (i.e., Debug.LogError ) 
	/// </summary>
    public bool sendUnityLogErrors = true;
	/// <summary>
	/// Determines if you want to send Unity log exceptions to GAMEhud. 
	/// </summary>
    public bool sendUnityLogExceptions = true;
	/// <summary>
	/// Determines if you want to send log / event messages while you are using the Unity Editor. 
	/// </summary>
    public bool sendUnityEditorLogs = true;
	/// <summary>
	/// Determines how frequently to send game events to GAMEhud.  Enter the time in seconds.
	/// </summary>
    public int sendEventDelay = 30;

    private string devicePrefsKey = "gamehud_device_id_";
    private bool sendEvents = true;
	
	/// <summary>
	/// Singleton instance of this class 
	/// </summary>
    public static GameHud Instance;
	/// <summary>
	/// Unique identifier for this device.
	/// </summary>
    public static string DeviceIdentifier { get; private set; }
	/// <summary>
	/// Unique identifier for each Unity game session.
	/// </summary>
    public static string GameSessionIdentifier { get; private set; }
	/// <summary>
	/// Stores the version of your game to send to GAMEhud.  This is optional, but encouraged.
	/// </summary>
    public static string Version { get; set; }

    #region Unity Callbacks

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        if (String.IsNullOrEmpty(gameApiKey))
        {
            Debug.LogError("GAMEhud not configured.  Please set your game_api_key that you received from GAMEhud on the GAMEhud object.");
            return;
        }

        Application.RegisterLogCallback(GameHudEventQueue.UnityLog);
    }

    void Start()
    {
		//PlayerPrefs.DeleteKey(devicePrefsKey);  // Only used by the GAMEhud team 
        if (PlayerPrefs.HasKey(devicePrefsKey))
        {
            DeviceIdentifier = PlayerPrefs.GetString(devicePrefsKey);
        }
        else
		{
			DeviceIdentifier = System.Guid.NewGuid().ToString();
			PlayerPrefs.SetString(devicePrefsKey, DeviceIdentifier);
		}
        
		GameSessionIdentifier = System.Guid.NewGuid().ToString();
		SendDeviceInfo();
        StartCoroutine(StartSendEventLoop());
    }

    void OnLevelWasLoaded()
    {
        SendEventQueue();
    }

    void OnApplicationQuit()
    {
        SendEventQueue();
    }

    #endregion

    IEnumerator StartSendEventLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds((float)sendEventDelay);

            //Debug.LogWarning("Timer Loop");

            if (sendEvents)
                SendEventQueue();
        }
    }

    public void PauseSendingEvents()
    {
        sendEvents = false;
    }

    public void ResumeSendingEvents()
    {
        sendEvents = true;
    }

    private void SendDeviceInfo()
    {
	    var form = new WWWForm();
		
        form.AddField("operating_system", SystemInfo.operatingSystem);
        form.AddField("processor_type", SystemInfo.processorType);
        form.AddField("processor_count", SystemInfo.processorCount);
        form.AddField("system_memory_size", SystemInfo.systemMemorySize);
        form.AddField("graphics_memory_size", SystemInfo.graphicsMemorySize);
        form.AddField("graphics_device_name", SystemInfo.graphicsDeviceName);
        form.AddField("graphics_device_vendor", SystemInfo.graphicsDeviceVendor);
        form.AddField("graphics_device_id", SystemInfo.graphicsDeviceID);
        form.AddField("graphics_device_vendor_id", SystemInfo.graphicsDeviceVendorID);
        form.AddField("graphics_device_version", SystemInfo.graphicsDeviceVersion);
        form.AddField("graphics_shader_level_id", SystemInfo.graphicsShaderLevel);
        form.AddField("graphics_pixel_fillrate", SystemInfo.graphicsPixelFillrate);
        form.AddField("supports_shadows", SystemInfo.supportsShadows ? "True" : "False");
        form.AddField("supports_render_textures", SystemInfo.supportsRenderTextures ? "True" : "False");
        form.AddField("supports_image_effects", SystemInfo.supportsImageEffects ? "True" : "False");
        form.AddField("system_language", Application.systemLanguage.ToString());
	
	    StartCoroutine(Send("devices", form, new GameHudEvent()));//, RegisterSession));
    }

    private void SendEventQueue()
    {
        if (GameHudEventQueue.Events.Count == 0)
        {
            return;
        }
		
		for (int i = 0; i < GameHudEventQueue.Events.Count; i++) 
		{
	        var form = new WWWForm();
			
			form.AddField("gh_session_identifier", GameSessionIdentifier);
			form.AddField("version", Version);
			
			form.AddField("gh_name", GameHudEventQueue.Events[i]._Name);
			form.AddField("gh_recorded_at", GameHudEventQueue.Events[i]._RecordedAt);
			if (GameHudEventQueue.Events[i]._StackTrace != "") form.AddField("gh_bucket", GameHudEventQueue.Events[i]._StackTrace);
			if (GameHudEventQueue.Events[i]._Level != "") form.AddField("level", GameHudEventQueue.Events[i]._Level);
			if (GameHudEventQueue.Events[i]._LogType != "") form.AddField("log_type", GameHudEventQueue.Events[i]._LogType);
			if (GameHudEventQueue.Events[i]._Occurences != 0) form.AddField("occurences", GameHudEventQueue.Events[i]._Occurences);
			
			if (GameHudEventQueue.Events[i]._EventProperties != null)
			{
				foreach (var pair in GameHudEventQueue.Events[i]._EventProperties)
				{
					form.AddField(pair.Key, pair.Value);
				}
			}
			
			StartCoroutine(Send("events", form, GameHudEventQueue.Events[i]));
		}
    }

    IEnumerator Send(string method, WWWForm form, GameHudEvent gameHudEvent)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
            yield break;

        var url = "https://www.mygamehud.com/api/v2/";
        url += method;

        form.AddField("gh_api_key", gameApiKey);
		form.AddField("gh_device_identifier", DeviceIdentifier);
		form.AddField("gh_submitted_at", DateTime.Now.ToString("O"));

        WWW www = new WWW(url, form);
        yield return www;

        // WWW does not react to HTTP status codes, only transport errors?
        if (www.error != null || www.text.Substring(0, 1) != "0")
        {
            Debug.LogError(www.text + " - " + www.error);
        }
        else 
        {
            //if (method == "devices") Debug.Log("GameHUD-Device Sent");
            if (method == "events")
            {
                GameHudEventQueue.Events.Remove(gameHudEvent);
                //Debug.Log("GameHUD-Events Sent");
            }
        }
    }
}


