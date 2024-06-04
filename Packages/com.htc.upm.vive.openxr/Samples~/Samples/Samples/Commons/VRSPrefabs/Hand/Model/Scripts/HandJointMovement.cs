// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;

namespace VIVE.OpenXR.Samples
{
    public class HandJointMovement : MonoBehaviour
    {
        public HandFlag Hand;
        [SerializeField] int Joint;
        void Start()
        {
            Joint = int.Parse(name.Split(new char[] { '_' })[1]);
        }

        void Update()
        {
            if(HandTracking.GetHandJointLocations(Hand)[Joint].isValid)
            {
                //transform.position = HandTracking.Get_HandJointLocations(Hand)[Joint].Position;
                transform.rotation = HandTracking.GetHandJointLocations(Hand)[Joint].rotation;
            }
        }
    }
}