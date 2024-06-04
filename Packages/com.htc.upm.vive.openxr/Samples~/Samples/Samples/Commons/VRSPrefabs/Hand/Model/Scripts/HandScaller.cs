// Copyright HTC Corporation All Rights Reserved.

using System.Collections;
using UnityEngine;

namespace VIVE.OpenXR.Samples
{
    public class HandScaller : MonoBehaviour
    {
        [SerializeField] HandFlag Hand;
        void Start()
        {
            StartCoroutine(Updating_HandScale());
        }

        //1 0 12 13 14 15
        IEnumerator Updating_HandScale()
        {
            float _StandardHandLength = 0.17f;//0.1752f;
            float _UserHandLength;
            float _Scaller = 1;
            while (true)
            {
                _UserHandLength = 0;
                _UserHandLength += Vector3.Distance(HandTracking.GetHandJointLocations(Hand)[1].position, HandTracking.GetHandJointLocations(Hand)[0].position);
                _UserHandLength += Vector3.Distance(HandTracking.GetHandJointLocations(Hand)[0].position, HandTracking.GetHandJointLocations(Hand)[12].position);
                _UserHandLength += Vector3.Distance(HandTracking.GetHandJointLocations(Hand)[12].position, HandTracking.GetHandJointLocations(Hand)[13].position);
                _UserHandLength += Vector3.Distance(HandTracking.GetHandJointLocations(Hand)[13].position, HandTracking.GetHandJointLocations(Hand)[14].position);
                _UserHandLength += Vector3.Distance(HandTracking.GetHandJointLocations(Hand)[14].position, HandTracking.GetHandJointLocations(Hand)[15].position);

                _Scaller = _UserHandLength / _StandardHandLength;
                transform.localScale = new Vector3(_Scaller, _Scaller, _Scaller);
                yield return new WaitForFixedUpdate();
            }
        }
    }
}