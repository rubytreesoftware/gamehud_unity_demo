using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;

/// <summary>
/// The base class for implementing the GAMEhud API interface in Unity.
/// </summary>
public class GameHud : MonoBehaviour
{
    /// <summary>
    /// Unique identifier of the GameHUD game.  This id is supplied to you when you register the game on the GAMEhud site. 
    /// </summary>
    public int gameId;
	/// <summary>
	/// Secret Key used in conjunction with the gameID to authenticate your communicate with the GAMEhud API. 
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
    public int sendEventDelay = 5;

    private string machinePrefsKey = "gamehud_machine_id_";
    private int eventsSent;
    private bool sendEvents = true;
	
	/// <summary>
	/// Singleton instance of this class 
	/// </summary>
    public static GameHud Instance;
	/// <summary>
	/// Unique identifier for this machine.  The id is obtained from the gameHUD service. 
	/// </summary>
    public static int MachineId { get; private set; }
	/// <summary>
	/// Unique identifier for each Unity game session.  This id is obtained from the GAMEhud service.
	/// </summary>
    public static int GameSessionId { get; private set; }
	/// <summary>
	/// Stores the version of your game to send to GAMEhud.  This is optional, but encouraged.
	/// </summary>
    public static string Version { get; private set; }

    #region Unity Callbacks

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);

        if (gameId == 0 || String.IsNullOrEmpty(gameApiKey))
        {
            Debug.LogError("GameHUD not configured.  Please set your game_id and game_api_key that you received from GameHUD on the GameHUD object.");
            return;
        }

        Application.RegisterLogCallback(GameHudEventQueue.UnityLog);
    }

    void Start()
    {
        // Values to be removed, inserted for testing //
        Version = "1.0";
        PlayerPrefs.DeleteKey(machinePrefsKey);
        //////////////////////////////////////////////////

        if (PlayerPrefs.HasKey(machinePrefsKey))
        {
            MachineId = PlayerPrefs.GetInt(machinePrefsKey);
            RegisterSession();
        }
        else
            RegisterMachine();

        Debug.LogError("Log test 1");
        Debug.LogWarning("Log test 2");

        StartCoroutine(StartSendEventLoop());
    }

    void OnLevelWasLoaded()
    {
        SendEventQueue();
    }

    void OnApplicationQuit()
    {
        SendEventQueue();
        CloseSession();
    }

    //OnApplicationPause

    #endregion

    IEnumerator StartSendEventLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds((float)sendEventDelay);

            Debug.LogWarning("Timer Loop");

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

    private void RegisterMachine()
    {
        var form = new WWWForm();

#if !UNITY_IPHONE
        if (Application.platform != RuntimePlatform.IPhonePlayer)
        {
            form.AddField("machine[operating_system]", SystemInfo.operatingSystem);
            form.AddField("machine[processor_type]", SystemInfo.processorType);
            form.AddField("machine[processor_count]", SystemInfo.processorCount);
            form.AddField("machine[system_memory_size]", SystemInfo.systemMemorySize);
            form.AddField("machine[graphics_memory_size]", SystemInfo.graphicsMemorySize);
            form.AddField("machine[graphics_device_name]", SystemInfo.graphicsDeviceName);
            form.AddField("machine[graphics_device_vendor]", SystemInfo.graphicsDeviceVendor);
            form.AddField("machine[graphics_device_id]", SystemInfo.graphicsDeviceID);
            form.AddField("machine[graphics_device_vendor_id]", SystemInfo.graphicsDeviceVendorID);
            form.AddField("machine[graphics_device_version]", SystemInfo.graphicsDeviceVersion);
            form.AddField("machine[graphics_shader_level_id]", SystemInfo.graphicsShaderLevel);
            form.AddField("machine[graphics_pixel_fillrate]", SystemInfo.graphicsPixelFillrate);
            form.AddField("machine[supports_shadows]", SystemInfo.supportsShadows ? 1 : 0);
            form.AddField("machine[supports_render_textures]", SystemInfo.supportsRenderTextures ? 1 : 0);
            form.AddField("machine[supports_image_effects]", SystemInfo.supportsImageEffects ? 1 : 0);
            form.AddField("machine[system_language]", Application.systemLanguage.ToString());
        }
#endif

        StartCoroutine(Send("machines", form, RegisterSession));
    }

    private void RegisterSession()
    {
        var form = new WWWForm();

        if (MachineId != 0)
            form.AddField("game_session[machine_id]", MachineId);
        if (Version != null)
            form.AddField("game_session[version]", Version);

        StartCoroutine(Send("game_sessions", form, null));
    }

    private void CloseSession()
    {
        var form = new WWWForm();

        form.AddField("_method", "put");

        StartCoroutine(Send("game_sessions/" + GameSessionId, form, null));
    }

    private void SendEventQueue()
    {
        if (GameHudEventQueue.Events.Count == 0)
        {
            return;
        }

        var form = new WWWForm();

        if (GameSessionId != 0)
            form.AddField("game_session_id", GameSessionId);

        var json = JsonWriter.Serialize(GameHudEventQueue.Events);
        eventsSent = GameHudEventQueue.Events.Count;

        form.AddField("game_events", json);

        StartCoroutine(Send("game_events", form, null));
    }

    IEnumerator Send(string method, WWWForm form, Action callBack) //Func<string, int>
    {
        //Debug.Log("Sending");
        if (Application.internetReachability == NetworkReachability.NotReachable)
            yield break;

        int tempId;
        var url = "http://mygamehud.herokuapp.com/api/v1/";
        url += method;

        form.AddField("game_id", gameId);
        form.AddField("game_api_key", gameApiKey);

        WWW www = new WWW(url, form);
        yield return www;

        // WWW does not react to HTTP status codes, only transport errors?
        if (www.error != null || www.text.Substring(0, 1) == "E")
        {
            Debug.LogError(www.text + " - " + www.error);
        }
        else if (int.TryParse(www.text, out tempId))
        {
            //Debug.Log("Parsing response");
            if (method == "machines")
            {
                MachineId = tempId;
                PlayerPrefs.SetInt(machinePrefsKey, MachineId);
                Debug.Log("GameHUD-MachineId: " + tempId);
            }
            else if (method == "game_sessions")
            {
                GameSessionId = tempId;
                Debug.Log("GameHUD-GameSessionId: " + tempId);
            }
            else if (method == "game_events")
            {
                GameHudEventQueue.Events.RemoveRange(0, eventsSent);
                Debug.Log("GameHUD-QueuedGameEventsId: " + tempId);
            }
        }

        if (callBack != null)
            callBack();
    }

}


