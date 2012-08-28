using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class handles the behavior of the player object.  It's also responsible for painting the screen GUI.
/// </summary>
public class Player : MonoBehaviour
{

    /// <summary>
    /// How fast the player moves.
    /// </summary>
    public float PlayerSpeed = 5;
    /// <summary>
    /// The prefab for the projectiles.  This is the prefab that is instantiated when the player shoots (spacebar).  PewPew!
    /// </summary>
    public GameObject ProjectilePrefab;
    /// <summary>
    /// The prefab of the particle system for the explosion that happens when the player dies.
    /// </summary>
    public GameObject ExplosionPrefab;

    /// <summary>
    /// The current score for the player
    /// </summary>
    public static int Score = 0;
    /// <summary>
    /// How many lives the player has.
    /// </summary>
    public static int Lives = 3;

    public static string CurrentRank = "01 - Ensign";

    enum State
    {
        Playing,
        Exploding,
        Invincible
    }
    private Transform myTransform;
    private State state = State.Playing;
    private float shipInvisibleTime = 0.75f;
    private float shipMoveOntoScreenSpeed = 5f;
    private float blinkSpeed = 0.1f;
    private float blinkTimes = 10;
    private float blinkCount;
    private float shipStartingYPos = -3.15f;
    private float screenLeftLimit = -7.0f;
    private float screenRightLimit = 7.0f;

    void Awake()
    {
        GameHud.Version = DemoGame.VERSION;
    }

    void Start()
    {
        myTransform = transform;
        myTransform.position = new Vector3(0, shipStartingYPos, myTransform.position.z);
    }

    void Update()
    {
        if (state != State.Exploding)
        {
            float amountToMove = Input.GetAxisRaw("Horizontal") * PlayerSpeed * Time.deltaTime;
            myTransform.Translate(Vector3.right * amountToMove);

            //Screenwrap 
            if (myTransform.position.x <= screenLeftLimit)
                myTransform.position = new Vector3(screenRightLimit - 0.1f, myTransform.position.y, myTransform.position.z);
            else if (myTransform.position.x >= screenRightLimit)
                myTransform.position = new Vector3(screenLeftLimit + 0.1f, myTransform.position.y, myTransform.position.z);

            //Fire projectile
            if (Input.GetKeyDown("space"))
            {
                Vector3 position = new Vector3(myTransform.position.x, myTransform.position.y * (myTransform.localScale.y / 2));
                Instantiate(ProjectilePrefab, position, Quaternion.identity);
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 20), "Score: " + Player.Score.ToString());
        GUI.Label(new Rect(10, 40, 100, 20), "Lives: " + Player.Lives.ToString());
        GUI.Label(new Rect(10, 70, 200, 20), "Rank: " + Player.CurrentRank);
    }

    void OnTriggerEnter(Collider otherObject)
    {
        if (otherObject.tag == "Enemy" && state == State.Playing)
        {
            //"Destroy" (actually just move) the enemy
            Enemy enemy = (Enemy)otherObject.GetComponent("Enemy");
            enemy.SetStartingPositionAndSpeed();

            StartCoroutine(DestroyShip());
        }
    }

    IEnumerator DestroyShip()
    {		
		Dictionary<string, string> propertyList = new Dictionary<string, string>();
		propertyList.Add("Rank", Player.CurrentRank);
		propertyList.Add("Score", Player.Score.ToString());
		GameHudEventQueue.Log("Player died", propertyList);
		
        Lives--;
        state = State.Exploding;
        renderer.enabled = false;
        Vector3 position = new Vector3(myTransform.position.x, myTransform.position.y, myTransform.position.z);
        Instantiate(ExplosionPrefab, position, Quaternion.identity);
        if (Lives < 0)
        {
			GameHudEventQueue.Log("Game Over", propertyList);
            yield return new WaitForSeconds(1.0f);
            Application.LoadLevel("GameOver");
        }
        myTransform.position = new Vector3(0f, -4.7f, myTransform.position.z);
        yield return new WaitForSeconds(shipInvisibleTime);
        renderer.enabled = true;
        while (myTransform.position.y <= shipStartingYPos)
        {
            float amountToMove = shipMoveOntoScreenSpeed * Time.deltaTime;
            myTransform.position = new Vector3(0, myTransform.position.y + amountToMove, myTransform.position.z);
            yield return 0;
        }
        state = State.Invincible;
        while (blinkCount < blinkTimes)
        {
            renderer.enabled = !renderer.enabled;
            if (renderer.enabled)
                blinkCount++;
            yield return new WaitForSeconds(blinkSpeed);
        }
        blinkCount = 0;
        state = State.Playing;
        Debug.LogError("Pretend Error: View the details in GameHud.");
    }
}
