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
        string type;

        if (logType == LogType.Log && GameHud.Instance.sendUnityLogInfo)
            type = "Log";
        else if (logType == LogType.Warning && GameHud.Instance.sendUnityLogWarnings)
            type = "Warning";
        else if (logType == LogType.Error && GameHud.Instance.sendUnityLogErrors)
            type = "Error";
        else if (logType == LogType.Exception && GameHud.Instance.sendUnityLogExceptions)
            type = "Exception";
        else
            return;

        if (!string.IsNullOrEmpty(message) && message.StartsWith("GameHUD"))
            return;

        Log(type, message, 0, 0, 0, stackTrace);
    }

    /// <summary>
    /// Captures custom events to be sent to GAMEhud.
    /// </summary>
    /// <param name="type">The event type.</param>
    /// <param name="message">The message.</param>
    public static void Log(string type, string message)
    {
        Log(type, message, 0, 0, 0, "");
    }

    /// <summary>
    /// Captures custom events to be sent to GAMEhud.
    /// </summary>
    /// <param name="type">The event type.</param>
    /// <param name="message">The message.</param>
    public static void Log(string type, string message, string callStack)
    {
        Log(type, message, 0, 0, 0, callStack);
    }

    /// <summary>
    /// Captures custom events to be sent to GAMEhud.
    /// </summary>
    /// <param name="type">The event type.</param>
    /// <param name="message">The message.</param>
    public static void Log(string type, string message, float xPosition, float yPosition, float zPosition)
    {
        Log(type, message, xPosition, yPosition, zPosition, "");
    }

    /// <summary>
    /// Captures custom events to be sent to GAMEhud.
    /// </summary>
    /// <param name="type">The type.</param>
    /// <param name="message">The message.</param>
    /// <param name="xPosition">The x position.</param>
    /// <param name="yPosition">The y position.</param>
    /// <param name="zPosition">The z position.</param>
    /// <param name="callStack">The call stack.</param>
    public static void Log(string type, string message, float xPosition, float yPosition, float zPosition, string callStack)
    {
        if (Application.isEditor && !GameHud.Instance.sendUnityEditorLogs)
            return;

        for (int i = 0; i < Events.Count; i++)
        {
            var eventLog = Events[i];

            if (eventLog.type == type && eventLog.message == message && eventLog.call_stack == callStack)
            {
                eventLog.counter++;
                Events[i] = eventLog;
                return;
            }
        }

        Events.Add(new GameHudEvent
        {
            counter = 1,
            type = type,
            message = message,
            level = Application.loadedLevelName,
            xPosition = xPosition,
            yPosition = yPosition,
            zPosition = zPosition,
            call_stack = callStack
        });

    }

}
