using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using JigglePhysics;
using System.Collections.Generic;
using UnityEngine;

namespace Basis.Scripts.Drivers
{
public class BasisAvatarStrainJiggleDriver : MonoBehaviour
{
    public BasisPlayer player;
    public JiggleRigBuilder Jiggler;
    public JiggleRigSimpleLOD SimpleJiggleLOD = new JiggleRigSimpleLOD();

    public void OnCalibration()
    {
        if (Jiggler != null)
        {
            PrepareTeleport();
        }
        if (player.Avatar != null)
        {
            if (player.Avatar.JiggleStrains != null && player.Avatar.JiggleStrains.Count != 0)
            {
                if (Jiggler == null)
                {
                    Jiggler = BasisHelpers.GetOrAddComponent<JiggleRigBuilder>(player.Avatar.Animator.gameObject);
                }
                SimpleJiggleLOD.currentCamera = BasisLocalCameraDriver.Instance.Camera;
                Jiggler.interpolate = true;
                Jiggler.levelOfDetail = SimpleJiggleLOD;
                Jiggler.jiggleRigs = new List<JiggleRigBuilder.JiggleRig>();
                foreach (BasisJiggleStrain Strain in player.Avatar.JiggleStrains)
                {
                    JiggleRigBuilder.JiggleRig JiggleRig = Conversion(Strain);
                    Jiggler.jiggleRigs.Add(JiggleRig);
                    Jiggler.Initialize();
                }
            }
        }
        if (Jiggler != null)
        {
            Jiggler.FinishTeleport();
        }
    }
    public void PrepareTeleport()
    {
        if (Jiggler != null)
        {
            Jiggler.PrepareTeleport();
        }
    }
    public void FinishTeleport()
    {
        if (Jiggler != null)
        {
            Jiggler.FinishTeleport();
        }
    }
    public void SetWind(Vector3 Wind)
    {
        Jiggler.wind = Wind;
    }
    public JiggleRigBuilder.JiggleRig Conversion(BasisJiggleStrain Strain)
    {
        JiggleSettings Base = new JiggleSettings();
        JiggleSettingsData Data = new JiggleSettingsData();
        Data.gravityMultiplier = Strain.GravityMultiplier;
        Data.friction = Strain.Friction;
        Data.angleElasticity = Strain.AngleElasticity;
        Data.blend = Strain.Blend;
        Data.airDrag = Strain.AirDrag;
        Data.lengthElasticity = Strain.LengthElasticity;
        Data.elasticitySoften = Strain.ElasticitySoften;
        Data.radiusMultiplier = Strain.RadiusMultiplier;
        Base.SetData(Data);
        JiggleRigBuilder.JiggleRig JiggleRig = new JiggleRigBuilder.JiggleRig(Strain.RootTransform, Base, Strain.IgnoredTransforms, Strain.Colliders);
        return JiggleRig;
    }
}
}