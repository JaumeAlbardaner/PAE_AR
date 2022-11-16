    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using UnityEngine.XR.ARFoundation;
    using UnityEngine.Experimental.XR;
    
    public class mapupdater : MonoBehaviour
    {
        public GameObject Indicator;
        public GameObject Reference;
        private Vector3 Initialposition;    
        private Quaternion Initialrotation;
        private ARSessionOrigin arOrigin;
        public void Start()
        {
            //set initial position
            Initialposition = Indicator.transform.position;
            Initialrotation = Indicator.transform.rotation;

            arOrigin = FindObjectOfType<ARSessionOrigin>();
        }

        public void Update()
        {
            Quaternion newRot;
            Vector3 newPos;
            if(arOrigin != null){
                newRot = arOrigin.camera.transform.rotation;
                newPos = arOrigin.camera.transform.position;
            }
            else{
                newRot = Reference.transform.rotation;
                newPos = Reference.transform.position;
            }
            
            //Remember the previous position so we can apply deltas
            Vector3 deltaPosition = newPos + Initialposition;
            Quaternion deltaRotation = newRot * Initialrotation;
            
            if (Indicator != null)
            {
            // The initial forward vector of the sphere must be aligned with the initial camera direction in the XZ plane.
            // We apply translation only in the XZ plane.
            Indicator.transform.position =  new Vector3(deltaPosition.x,0.0f,deltaPosition.z);
            Vector3 rotation = deltaRotation.eulerAngles;
            Indicator.transform.rotation = Quaternion.Euler(0,rotation.y,0);
            // Set the pose rotation to be used in the CameraFollow script
//            FirstPersonCamera.GetComponent<FollowTarget>().targetRot = Frame.Pose.rotation;
            }
        }

    }