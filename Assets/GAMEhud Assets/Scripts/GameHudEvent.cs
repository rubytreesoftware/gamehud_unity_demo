using System;
using System.Collections.Generic;

/// <summary>
/// Struct stores the data required for each game event. 
/// </summary>
public struct GameHudEvent
{
	public string _Name;	
	public string _RecordedAt;
	public string _StackTrace;	
	public string _Level;
	public string _LogType;
	public Dictionary<string, string> _EventProperties;
}
