using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Captures log events from Unity as well as events to be sent directly to GAMEhud and places them in a queue. 
/// </summary>
public sealed class GameHudEventQueue
{
	/// <summary>
	/// List of events that are in the queue. 
	/// </summary>
    public static List<GameHudEvent> Events = new List<GameHudEvent>();

    GameHudEventQueue()
    {
        // Initialize.
    }
	
	/// <summary>
	/// Captures Unity Log events and stores them to the Event List based upon your preferences in the GameHud class. 
	/// </summary>
	/// <param name="message">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="stackTrace">
	/// A <see cref="System.String"/>
	/// </param>
	/// <param name="logType">
	/// A <see cref="LogType"/>
	/// </param>
    public static void UnityLog(string message, string stackTrace, LogType logType)
    {
        string logName;

        if (logType == LogType.Log && GameHud.Instance.sendUnityLogInfo)
            logName = "Log";
        else if (logType == LogType.Warning && GameHud.Instance.sendUnityLogWarnings)
            logName = "Warning";
        else if (logType == LogType.Error && GameHud.Instance.sendUnityLogErrors)
            logName = "Error";
        else if (logType == LogType.Exception && GameHud.Instance.sendUnityLogExceptions)
            logName = "Exception";
        else
            return;

        if (!string.IsNullOrEmpty(message) && message.StartsWith("GameHUD"))
            return;

        Log(message, logName, stackTrace, null);
    }

    /// <summary>
    /// Captures custom events to be sent to GAMEhud.
    /// </summary>
    /// <param name="name">The event name.</param>
	/// <param name="eventProperties">Event properties dictionary </param>
    public static void Log(string name, Dictionary<string, string> eventProperties)
    {
        Log(name, "", "", eventProperties);
    }	
	
    /// <summary>
    /// Captures custom events to be sent to GAMEhud.
    /// </summary>
    /// <param name="name">The event name.</param>
    public static void Log(string name)
    {
        Log(name, "", "", null);
    }

/// <summary>
/// Captures custom events to be sent to GAMEhud.
/// </summary>
/// <param name="name">Event Name</param>
/// <param name="logType">Unity Log type.</param>
/// <param name="stackTrace">Unity Stack trace.</param>
/// <param name="eventProperties">Event properties dictionary </param>
    public static void Log(string name, string logType, string stackTrace, Dictionary<string, string> eventProperties)
    {
        if (Application.isEditor && !GameHud.Instance.sendUnityEditorLogs)
            return;

        Events.Add(new GameHudEvent
        {
			_Name = name,
			_RecordedAt = System.DateTime.Now.ToString("O"),
			_StackTrace = stackTrace,
			_Level = Application.loadedLevelName,
			_LogType = logType,
			_EventProperties = eventProperties
        });
    }
}
