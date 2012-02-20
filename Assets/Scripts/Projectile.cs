using UnityEngine;
using System.Collections;

/// <summary>
/// This class handles the behavior for the projectiles.
/// </summary>
public class Projectile : MonoBehaviour
{
    /// <summary>
    /// How fast the projectile moves
    /// </summary>
    public float ProjectileSpeed = 10;
    /// <summary>
    /// The prefab for the explosion particle system.  This is the explosion that happens when an enemy is shot.
    /// </summary>
    public GameObject ExplosionPrefab;

    private Transform myTransform;

    void Start()
    {
        myTransform = transform;
    }

    void Update()
    {
        float amountToMove = ProjectileSpeed * Time.deltaTime;
        myTransform.Translate(Vector3.up * amountToMove);

        if (myTransform.position.y > 6.25f)
            Destroy(this.gameObject);
    }

    void OnTriggerEnter(Collider otherObject)
    {
        if (otherObject.tag == "Enemy")
        {
            Vector3 position = new Vector3(otherObject.transform.position.x, otherObject.transform.position.y, otherObject.transform.position.z);
            Instantiate(ExplosionPrefab, position, Quaternion.identity);
            //"Destroy" (actually just move) the enemy
            Enemy enemy = (Enemy)otherObject.GetComponent("Enemy");
            enemy.SetStartingPositionAndSpeed();

            Debug.Log(Player.Score.ToString());
            Debug.Log(Player.Lives.ToString());
            Player.Score += 100;
            SetPlayerRank();

            //Destroy the projectile
            Destroy(gameObject);

            Debug.Log("Player destroyed the enemy!  Now the score is " + Player.Score.ToString());
        }
    }

    private void SetPlayerRank()
    {
        bool rankChanged = false;

        if (Player.Score == 3500)
        {
            Player.CurrentRank = "08 - Fleet Admiral";
            rankChanged = true;
        }
        else if (Player.Score == 3000)
        {
            Player.CurrentRank = "07 - Admiral";
            rankChanged = true;
        }
        else if (Player.Score == 2500)
        {
            Player.CurrentRank = "06 - Captain";
            rankChanged = true;
        }
        else if (Player.Score == 2000)
        {
            Player.CurrentRank = "05 - Commander";
            rankChanged = true;
        }
        else if (Player.Score == 1500)
        {
            Player.CurrentRank = "04 - Lt. Commander";
            rankChanged = true;
        }
        else if (Player.Score == 1000)
        {
            Player.CurrentRank = "03 - Lieutenant";
            rankChanged = true;
        }
        else if (Player.Score == 500)
        {
            Player.CurrentRank = "02 - Lt. Junior Grade";
            rankChanged = true;
        }
        else if (Player.Score == 0)
        {
            Player.CurrentRank = "01 - Ensign";
            rankChanged = true;
        }

        if (rankChanged)
            GameHudEventQueue.Log("Rank Earned", Player.CurrentRank);
    }
}
