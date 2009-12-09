using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Patron : Base
{

    public LayerMask layerMask; //make sure we aren't in this layer 
    float skinWidth = 0.1f; //probably doesn't need to be changed 
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
    protected override void Start()
    {
        this.rigidbody.velocity = transform.TransformDirection(Vector3.forward) * velocity;
        //Physics.IgnoreCollision
    }
    float tm;
    protected override void FixedUpdate()
    {
        

        tm += Time.deltaTime;
        if (tm > 5) Destroy(gameObject);
        
        if (previousPosition != default(Vector3))//((previousPosition - myRigidbody.position).sqrMagnitude > sqrMinimumExtent) 
        {                
            Vector3 movementThisStep = myRigidbody.position - previousPosition;
            float movementMagnitude = movementThisStep.magnitude;
            RaycastHit hitInfo;

            if (Physics.Raycast(previousPosition, movementThisStep, out hitInfo, movementThisStep.magnitude, layerMask.value))
            {
                if (hitInfo.collider.gameObject.tag == "Player")//(!(hitInfo.collider.gameObject.tag == "Player" && !hitInfo.collider.gameObject.networkView.isMine))
                //Trace.Log(hitInfo.collider.gameObject.name);
                {
                    Trace.Log("hit:" + OwnerID + ":" + hitInfo.collider.gameObject.GetComponent<PL>().OwnerID);
                    if (OwnerID != hitInfo.collider.gameObject.GetComponent<PL>().OwnerID) Hit(hitInfo.point); //myRigidbody.position = hitInfo.point - (movementThisStep / movementMagnitude) * partialExtent;
                }
                else 
                    Hit(hitInfo.point);
            }
                    
            
        }
        
        previousPosition = myRigidbody.position;

    }
    protected NetworkPlayer killby;
    PL pl { get { return PL._LocalPlayer; } }
    
    public float _md=3;
    public float _p=60;
    
    

    List<Vector3> trs = new List<Vector3>();
    private void Hit(Vector3 transform)
    {  
        
        trs.Add(transform);

        //Network.RemoveRPCs(networkView.owner, (int)Group.Patron);
        //Detonator.size = 3;
        Object dt = (Object)Instantiate(exp, transform, Quaternion.identity);
        //Detonator a = (Detonator)dt.GetComponent(typeof(Detonator));                
        Destroy(dt, 10);
        Destroy(gameObject);
        if (!pl.isdead)
        {
            float dist = Vector3.Distance(pl.transform.position, transform);
            pl.killedyby = OwnerID.Value;
            pl.Life -= (int)(Mathf.Max(_md - dist, 0) * _p);
        }
    }
}
