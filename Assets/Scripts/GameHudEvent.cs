using System;

/// <summary>
/// Struct stores the data required for each game event. 
/// </summary>
public struct GameHudEvent
{
	public int counter;
	public string type;
	public string message;
	public string level;
	public float xPosition;
	public float yPosition;
	public float zPosition;
	public string call_stack;	
}
