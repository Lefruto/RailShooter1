using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject destroyedVFX;

    GameSceneManager gameSceneManager;
    void Start()
    {
        gameSceneManager = GameObject.Find("Game Scene Manager").GetComponent<GameSceneManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Referencing the colliding trigger
    void OnTriggerEnter(Collider other)
    {
        //Reload Level when entering trigger
        gameSceneManager.ReloadLevel();
        //When our ship enters a collision event it will trigger events.
        //Spawn destroy VFX effects
        Instantiate(destroyedVFX,transform.position,Quaternion.identity);
        //Destroy game object
        Destroy(gameObject);
        //DEbug Log to test if the trigger system works or not..
        Debug.Log("Hit " + other.name);
        Debug.Log($"Hit {other.gameObject.name}");
    }
}
