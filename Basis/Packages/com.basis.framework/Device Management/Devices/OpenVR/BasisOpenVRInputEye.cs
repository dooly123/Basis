using Basis.Scripts.Drivers;
using Valve.VR;

namespace Basis.Scripts.Device_Management.Devices.OpenVR
{
    public class BasisOpenVRInputEye : BasisInputEye
    {
        public override void Initalize()
        {

        }
        public override void Simulate()
        {
            LeftPosition = SteamVR.instance.eyes[0].pos;//0 = left
            RightPosition = SteamVR.instance.eyes[1].pos; //1 = right
            if (BasisLocalCameraDriver.HasInstance)
            {
                BasisLocalCameraDriver.LeftEye = LeftPosition;
                BasisLocalCameraDriver.RightEye = RightPosition;
            }
        }
    }
}
