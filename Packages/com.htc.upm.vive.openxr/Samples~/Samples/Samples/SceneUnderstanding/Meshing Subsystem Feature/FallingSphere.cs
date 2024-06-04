using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.OpenXR.Samples.MeshingFeature
{
    public class FallingSphere : MonoBehaviour
    {
        private Vector3 starting;
        private Rigidbody rb;

        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody>();
            starting = transform.position;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (transform.position.y < -10)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.MovePosition(starting);
            }
        }
    }
}