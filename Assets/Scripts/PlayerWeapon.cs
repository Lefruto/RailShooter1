using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeapon : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    bool isFiring = false; //2

    //[SerializeField] GameObject laser;
    [SerializeField] GameObject[] lasers; //8
    [SerializeField] RectTransform crosshair; //11

    [SerializeField] Transform targetPoint; //14
    [SerializeField] float targetDistance = 100f; //15
    void Start()
    {
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        ProcessFire();
        MoveCrosshair();
        MoveTargetPoint();
        AimFire();
    }

    void OnFire(InputValue value) //1
    {
        isFiring = value.isPressed; //4
        //Debug.Log("Fire");//3
    }

    void ProcessFire()
    {
        /* if(isFiring) //5
        {
            //Debug.Log("Fire");
            
        } */
        // ParticleSystem.EmissionModule emmision = laser.GetComponent<ParticleSystem>().emission; //6
        // emmision.enabled = isFiring; //7
        
        foreach(GameObject laser in lasers) //8
        {
            ParticleSystem.EmissionModule emmision = laser.GetComponent<ParticleSystem>().emission; //9
            emmision.enabled = isFiring; //10
        }

    }

    void MoveCrosshair() //12
    {
        crosshair.position = Input.mousePosition; //13
    }

    void MoveTargetPoint() //16
    {
        Vector3 targetPointPosition = new Vector3(Input.mousePosition.x,Input.mousePosition.y,targetDistance); //17
        targetPoint.position = Camera.main.ScreenToWorldPoint(targetPointPosition); //18
    }

    //To aim to the target point
    void AimFire()
    {
        //We are pointing lasers to the target point..So we need to access each laser
        foreach(GameObject laser in lasers)
        {
            Vector3 fireDirection = targetPoint.position - this.transform.position; //Creating a vector for fire direction
            //if we dont want to use laser transformation we can use ship transform position. It is logical to use ship's transformation.
            //Vector3 fireDirection = targetPoint.position - this.transform.position; //this keyword means the gameobject the script attached..
            Quaternion rotationToTarget = Quaternion.LookRotation(fireDirection); //Creating a rotation for fire direction vector
            laser.transform.rotation = rotationToTarget; //Assigning each laser the rotation factor.
        }
    }
}
