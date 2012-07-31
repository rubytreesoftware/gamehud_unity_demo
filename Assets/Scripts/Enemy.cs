using UnityEngine;
using System.Collections;

/// <summary>
/// This class controls the behavior for the Enemy object.
/// </summary>
public class Enemy : MonoBehaviour {
    /// <summary>
    /// The minimum speed that an Enemy can move
    /// </summary>
    public float MinSpeed = 4;
    /// <summary>
    /// The maximum speed that an Enemy can move
    /// </summary>
    public float MaxSpeed = 6;

    private Transform myTransform;
    private float currentSpeed;
    private float x, y, z;

    void Start()
    {
        myTransform = transform;
        SetStartingPositionAndSpeed();
    }
	
	void Update () {
        float amountToMove = currentSpeed * Time.deltaTime;
        myTransform.Translate(Vector3.down * amountToMove);

        //Enemy has moved past bottom of screen.  Move him back to top at a new random position.
        if (myTransform.position.y <= -4.6)
            SetStartingPositionAndSpeed();
	}

    /// <summary>
    /// Sets the starting position and speed of the Enemy object.
    /// </summary>
    public void SetStartingPositionAndSpeed()
    {
            //Using Random.Range here (instead of Random.RandomRange) as suggested causes jerkiness when the game is running.
            currentSpeed = Random.Range(MinSpeed, MaxSpeed);
            x = Random.Range(-5.5f, 5.5f);
            y = 7.0f;
            z = 0.0f;
            myTransform.position = new Vector3(x, y, z);
    }
}
