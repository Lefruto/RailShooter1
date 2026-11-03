using System.Collections;
using UnityEngine;

public enum PowerUpType
{
    Shield,
    RapidFire,
    ScoreBonus
}

/// <summary>
/// Collectible bonus item that floats in space.
/// Restores lives, accelerates fire rate, or awards bonus score.
/// </summary>
public class PowerUp : MonoBehaviour
{
    [SerializeField] private PowerUpType type = PowerUpType.RapidFire;
    [SerializeField] private float rotateSpeed = 90f;
    [SerializeField] private float bobAmplitude = 0.5f;
    [SerializeField] private float bobSpeed = 3f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;

        // Ensure trigger collider exists
        SphereCollider col = GetComponent<SphereCollider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
            col.radius = 2.5f;
        }
        col.isTrigger = true;
    }

    void Update()
    {
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public void Collect(GameObject player)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerup(0.7f);
        }

        switch (type)
        {
            case PowerUpType.Shield:
                Collision col = player.GetComponent<Collision>();
                if (col != null) col.RestoreLife();
                break;

            case PowerUpType.RapidFire:
                PlayerWeapons weapons = player.GetComponent<PlayerWeapons>();
                if (weapons != null)
                {
                    weapons.StartCoroutine(RapidFireBoostRoutine(weapons));
                }
                break;

            case PowerUpType.ScoreBonus:
                if (Scoreboard.Instance != null)
                {
                    Scoreboard.Instance.IncreaseScore(100);
                }
                break;
        }

        Destroy(gameObject);
    }

    private IEnumerator RapidFireBoostRoutine(PlayerWeapons weapons)
    {
        weapons.SetRapidFire(true);
        yield return new WaitForSeconds(8f);
        weapons.SetRapidFire(false);
    }
}
