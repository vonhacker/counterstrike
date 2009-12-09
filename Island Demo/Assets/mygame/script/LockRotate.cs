using UnityEngine;

using System.Collections;

public class LockRotate : MonoBehaviour
{
    Vector3 v;
    //Quaternion q;

    void Start()
    {
        v = transform.localPosition;
        //q = transform.localRotation;
    }
    void LateUpdate()
    {
        transform.position = transform.parent.position + v;
        
    }
}