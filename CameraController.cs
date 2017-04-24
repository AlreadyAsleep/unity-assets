using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	[System.Serializable]
    public class ViewSettings
    {
        public float camHeight = 20;// y distance from player
        public float camTilt = 10;// x tilt from player

    }

    public ViewSettings view = new ViewSettings();
    CharacterController charController;
    Vector3 camPos = Vector3.zero;
    public Transform target;

    void Start()
    {
        SetCameraTarget(target);
        Follow();
        transform.position = camPos;
        transform.Rotate(view.camTilt, 0, 0);
    }

    void FixedUpdate()
    {
        Follow();
        
    }

    void Follow()
    {
        camPos = new Vector3(target.position.x, view.camHeight, target.position.z );
        Vector3 destination = Vector3.Slerp(transform.position, camPos, .025f);
        transform.position = destination;
        
    }

    void SetCameraTarget(Transform t)
    {
        target = t;

        if (target != null)
        {
            if (target.GetComponent<CharacterController>())
            {
                charController = target.GetComponent<CharacterController>();
            }
            else
                Debug.LogError("Need Character Contoller");
        }
        else
            Debug.LogError("no target");
    }
}
