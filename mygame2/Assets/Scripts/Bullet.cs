using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Bullet : Base
{
    
    float skinWidth = 0.1f; 
    float minimumExtent;
    float partialExtent;
    float sqrMinimumExtent;
    Vector3 previousPosition;
    Transform myRigidbody;
    protected override void Awake()
    {
        myRigidbody = transform;
        minimumExtent = Mathf.Min(Mathf.Min(collider.bounds.extents.x, collider.bounds.extents.y), collider.bounds.extents.z);
        partialExtent = minimumExtent * (1.0f - skinWidth);
        sqrMinimumExtent = minimumExtent * minimumExtent;
    }

    public Transform exp;
    public float velocity;
    
    float tm;
    protected override void FixedUpdate()
    {
        this.transform.position += transform.TransformDirection(Vector3.forward) * velocity * Time.deltaTime;
        
        tm += Time.deltaTime;
        if (tm > 5) Destroy(gameObject);

        if (previousPosition != default(Vector3))
        {
            Vector3 movementThisStep = myRigidbody.position - previousPosition;
            float movementMagnitude = movementThisStep.magnitude;
            RaycastHit hitInfo;

            if (Physics.Raycast(previousPosition, movementThisStep, out hitInfo, movementThisStep.magnitude, 1))
                Hit(hitInfo.point);


        }

        previousPosition = myRigidbody.position;

    }
    protected NetworkPlayer killby;
    

    public float maxdistance = 30;
    public float maxdamage = 60;



    Player LocalPlayer { get { return Find<Player>("LocalPlayer"); } }

    private void Hit(Vector3 transform)
    {
        Object dt = (Object)Instantiate(exp, transform, Quaternion.identity);
        Destroy(dt, 10);
        Destroy(gameObject);        
        float dist = Vector3.Distance(LocalPlayer.transform.position, transform);
        int damage = (int)(Mathf.Max(maxdistance - dist, 0) * maxdamage);        
        if (!LocalPlayer.isdead && damage != 0)
        {            
            LocalPlayer.killedyby = OwnerID.Value;           
            LocalPlayer.RPCSetLife(LocalPlayer.Life - damage);
        }
    }

}
