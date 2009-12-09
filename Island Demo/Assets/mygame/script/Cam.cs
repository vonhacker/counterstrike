using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class Cam : MonoBehaviour
{
    public LayerMask lm;
    public PL pl {get{ return PL._LocalPlayer;}}
    public float maxdistance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    float x = 0.0f;
    float y = 0.0f;
    public float yoffset;
    public static Cam _This;
    // Use this for initialization 
    void Start()
    {
        _This = this;
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

    }

    void LateUpdate()
    {


        if (pl == null || pl.isdead) return;

        Vector3 pos = pl.transform.position;
        pos.y -= yoffset;
        float distance = maxdistance;

        if (Screen.lockCursor)
        {
            x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
        }
        y = ClampAngle(y, yMinLimit, yMaxLimit);

        Quaternion rotation = Quaternion.Euler(y, x, 0);

        RaycastHit hit;
        if (Physics.Raycast(pos, transform.position - pos, out hit, maxdistance, lm))
        {
            distance = hit.distance;
        }
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + pos;

        transform.rotation = rotation;
        transform.position = position;

    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }


}