using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestrySelf : MonoBehaviour
{
    public float LifeTime = 5f;
    // Start is called before the first frame update
    void Start()
    {   
        Destroy(this.gameObject, LifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
