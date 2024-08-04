using Valve.VR;

namespace Basis
{
    public class BasisOpenVRInputEye : BasisInputEye
    {
        public override void Initalize()
        {

        }
        public override void Simulate()
        {
            LeftPosition = SteamVR.instance.eyes[0].pos;
            RightPosition = SteamVR.instance.eyes[1].pos;
        }
    }
}
