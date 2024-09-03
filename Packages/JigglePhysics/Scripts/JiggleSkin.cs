using System;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
namespace JigglePhysics
{
    [DefaultExecutionOrder(15001)]
    public partial class JiggleSkin : MonoBehaviour
    {
        public JiggleZone[] jiggleZones;
        [SerializeField]
        [Tooltip("The list of skins to send the deformation data too, they should have JiggleSkin-compatible materials!")]
        public List<SkinnedMeshRenderer> targetSkins;
        [Tooltip("An air force that is applied to the entire rig, this is useful to plug in some wind volumes from external sources.")]
        public Vector3 wind;
        [Tooltip("Level of detail manager. This system will control how the jiggle skin saves performance cost.")]
        public JiggleRigLOD levelOfDetail;
        [SerializeField]
        [Tooltip("Draws some simple lines to show what the simulation is doing. Generally this should be disabled.")]
        private bool debugDraw = false;
        private bool wasLODActive = true;
        private double accumulation;
        private List<Material> targetMaterials;
        private List<Vector4> packedVectors;
        private int jiggleInfoNameID;
        private bool dirtyFromEnable;
        public int jiggleZonesCount;
        public Vector3 Gravity;
        public void PrepareTeleport()
        {
            for (int Index = 0; Index < jiggleZonesCount; Index++)
            {
                JiggleZone zone = jiggleZones[Index];
                zone.PrepareTeleport();
            }
        }
        public void FinishTeleport(double timeAsDouble,float FixedDeltaTime)
        {
            for (int Index = 0; Index < jiggleZonesCount; Index++)
            {
                jiggleZones[Index].FinishTeleport(timeAsDouble, FixedDeltaTime);
            }
        }
        private void OnEnable()
        {
            Initialize();
            dirtyFromEnable = true;
            Gravity = Physics.gravity;
            CachedSphereCollider.AddSkin(this);
        }
        private void OnDisable()
        {
            PrepareTeleport();
            CachedSphereCollider.RemoveSkin(this);
        }
        public void Initialize()
        {
            accumulation = 0f;
            jiggleZonesCount = jiggleZones.Length;
            for (int JiggleIndex = 0; JiggleIndex < jiggleZonesCount; JiggleIndex++)
            {
                JiggleZone zone = jiggleZones[JiggleIndex];
                zone.Initialize();
            }
            targetMaterials = new List<Material>();
            jiggleInfoNameID = Shader.PropertyToID("_JiggleInfos");
            packedVectors = new List<Vector4>();
        }
        public void Advance(float deltaTime,double TimeASDouble, float FixedDeltaTime)
        {
            if (levelOfDetail != null && !levelOfDetail.CheckActive(transform.position))
            {
                if (wasLODActive) PrepareTeleport();
                CachedSphereCollider.StartPass();
                CachedSphereCollider.FinishedPass();
                wasLODActive = false;
                return;
            }
            if (!wasLODActive)
            {
                FinishTeleport(TimeASDouble, FixedDeltaTime);
            }
            CachedSphereCollider.StartPass();
            for (int Index = 0; Index < jiggleZones.Length; Index++)
            {
                jiggleZones[Index].PrepareBone(transform.position, levelOfDetail, TimeASDouble);
            }

            if (dirtyFromEnable)
            {
                for (int JiggleIndex = 0; JiggleIndex < jiggleZones.Length; JiggleIndex++)
                {
                    JiggleZone rig = jiggleZones[JiggleIndex];
                    rig.FinishTeleport(TimeASDouble, FixedDeltaTime);
                }
                dirtyFromEnable = false;
            }

            accumulation = Math.Min(accumulation + deltaTime, FixedDeltaTime * 4f);
            while (accumulation > FixedDeltaTime)
            {
                accumulation -= FixedDeltaTime;
                double time = TimeASDouble - accumulation;
                for (int Index = 0; Index < jiggleZonesCount; Index++)
                {
                    JiggleZone zone = jiggleZones[Index];
                    zone.Update(wind, time, FixedDeltaTime, Gravity);
                }
            }
            for (int Index = 0; Index < jiggleZonesCount; Index++)
            {
                JiggleZone zone = jiggleZones[Index];
                zone.DeriveFinalSolve(TimeASDouble);
            }
            UpdateMesh();
            CachedSphereCollider.FinishedPass();

            if (!debugDraw) return;
            foreach (JiggleZone zone in jiggleZones)
            {
                zone.DebugDraw();
            }
        }
        private void LateUpdate()
        {
            Advance(Time.deltaTime,Time.timeAsDouble, Time.fixedDeltaTime);
        }
        private void UpdateMesh()
        {
            // Pack the data
            packedVectors.Clear();
            for (int SkinIndex = 0; SkinIndex < targetSkins.Count; SkinIndex++)
            {
                SkinnedMeshRenderer targetSkin = targetSkins[SkinIndex];
                for (int JiggleZoneIndex = 0; JiggleZoneIndex < jiggleZonesCount; JiggleZoneIndex++)
                {
                    JiggleZone zone = jiggleZones[JiggleZoneIndex];
                    Vector3 targetPointSkinSpace = targetSkin.rootBone.InverseTransformPoint(zone.GetRootTransform().position);
                    Vector3 verletPointSkinSpace = targetSkin.rootBone.InverseTransformPoint(zone.GetPointSolve());
                    packedVectors.Add(new Vector4(targetPointSkinSpace.x, targetPointSkinSpace.y, targetPointSkinSpace.z,
                        zone.radius * zone.GetRootTransform().lossyScale.x));
                    packedVectors.Add(new Vector4(verletPointSkinSpace.x, verletPointSkinSpace.y, verletPointSkinSpace.z,
                        zone.jiggleSettings.GetData().blend));
                }
            }
            for (int i = packedVectors.Count; i < 16; i++)
            {
                packedVectors.Add(Vector4.zero);
            }
            // Send the data
            foreach (SkinnedMeshRenderer targetSkin in targetSkins)
            {
                targetSkin.GetMaterials(targetMaterials);
                foreach (Material m in targetMaterials)
                {
                    m.SetVectorArray(jiggleInfoNameID, packedVectors);
                }
            }
        }
        void OnValidate()
        {
            if (jiggleZones == null)
            {
                return;
            }
            for (int i = jiggleZonesCount - 1; i > 8; i--)
            {
                Array.Resize(ref jiggleZones, 8);
            }
        }
        void OnRenderObject()
        {
            if (jiggleZones == null)
            {
                return;
            }
            for (int Index = 0; Index < jiggleZonesCount; Index++)
            {
                JiggleZone zone = jiggleZones[Index];
                zone.OnRenderObject();
            }
        }
    }
}