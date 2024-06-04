using JigglePhysics;
using System.Collections.Generic;
using UnityEngine;
public class BasisAvatarStrainJiggleDriver : MonoBehaviour
{
    public BasisPlayer player;
    public JiggleRigBuilder Jiggler;
    public JiggleRigSimpleLOD SimpleJiggleLOD = new JiggleRigSimpleLOD();
    public void OnCalibration()
    {
        if (player.Avatar != null)
        {
            if (player.Avatar.JiggleStrains != null && player.Avatar.JiggleStrains.Count != 0)
            {
                if (Jiggler == null)
                {
                    CreateJiggler();
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
    }
    public void SetWind(Vector3 Wind)
    {
        Jiggler.wind = Wind;
    }
    public void CreateJiggler()
    {
        Transform Transform = player.Avatar.Animator.GetBoneTransform(HumanBodyBones.Hips);
        if (Transform == null)
        {
            Transform = player.Avatar.transform;
        }
        Jiggler = Transform.gameObject.AddComponent<JiggleRigBuilder>();
        //  SimpleJiggleLOD = Hips.gameObject.AddComponent<JiggleRigSimpleLOD>();
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