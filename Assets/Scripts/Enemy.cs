using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] int hitpoint = 100;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision other)
    {
        hitpoint -= 10;

        if (hitpoint <= 0)
        {
            Destroy(gameObject);
        }

    }
}
