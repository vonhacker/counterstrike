using UnityEngine;

using System.Collections;

public class CameraFacingBillboard : MonoBehaviour
{
    Cam m_Camera { get { return Cam._This; } }

    void LateUpdate()
    {
        transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.back,
            m_Camera.transform.rotation * Vector3.up);
    }
}