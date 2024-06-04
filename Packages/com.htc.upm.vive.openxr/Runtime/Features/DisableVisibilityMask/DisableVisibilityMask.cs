// Copyright HTC Corporation All Rights Reserved.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VIVE.OpenXR {
    public class DisableVisibilityMask
    {
        static GameObject Provider;

        [RuntimeInitializeOnLoadMethod]
        static void Start()
        {
            Provider = new GameObject();
            Provider.AddComponent<VisibilityMaskDisabler>();
        }
    }
}
