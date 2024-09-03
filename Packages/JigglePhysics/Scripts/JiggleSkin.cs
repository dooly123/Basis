using System;
using System.Collections.Generic;
using UnityEngine;
namespace JigglePhysics
{
    [DefaultExecutionOrder(15001)]
    public partial class JiggleSkin : MonoBehaviour
    {
        public List<JiggleZone> jiggleZones;
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
        public void PrepareTeleport()
        {
            foreach (var zone in jiggleZones)
            {
                zone.PrepareTeleport();
            }
        }
        public void FinishTeleport()
        {
            foreach (var zone in jiggleZones)
            {
                zone.FinishTeleport();
            }
        }
        private void OnEnable()
        {
            Initialize();
            dirtyFromEnable = true;
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
            jiggleZones ??= new List<JiggleZone>();
            foreach (JiggleZone zone in jiggleZones)
            {
                zone.Initialize();
            }
            targetMaterials = new List<Material>();
            jiggleInfoNameID = Shader.PropertyToID("_JiggleInfos");
            packedVectors = new List<Vector4>();
        }
        public void Advance(float deltaTime)
        {
            if (levelOfDetail != null && !levelOfDetail.CheckActive(transform.position))
            {
                if (wasLODActive) PrepareTeleport();
                CachedSphereCollider.StartPass();
                CachedSphereCollider.FinishedPass();
                wasLODActive = false;
                return;
            }
            if (!wasLODActive) FinishTeleport();
            CachedSphereCollider.StartPass();
            foreach (JiggleZone zone in jiggleZones)
            {
                zone.PrepareBone(transform.position, levelOfDetail);
            }

            if (dirtyFromEnable)
            {
                foreach (var rig in jiggleZones)
                {
                    rig.FinishTeleport();
                }
                dirtyFromEnable = false;
            }

            accumulation = Math.Min(accumulation + deltaTime, Time.fixedDeltaTime * 4f);
            while (accumulation > Time.fixedDeltaTime)
            {
                accumulation -= Time.fixedDeltaTime;
                double time = Time.timeAsDouble - accumulation;
                foreach (JiggleZone zone in jiggleZones)
                {
                    zone.Update(wind, time);
                }
            }
            foreach (JiggleZone zone in jiggleZones)
            {
                zone.DeriveFinalSolve();
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
            Advance(Time.deltaTime);
        }
        private void UpdateMesh()
        {
            // Pack the data
            packedVectors.Clear();
            foreach (var targetSkin in targetSkins)
            {
                foreach (var zone in jiggleZones)
                {
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
            for (int i = jiggleZones.Count - 1; i > 8; i--)
            {
                jiggleZones.RemoveAt(i);
            }
        }
        void OnRenderObject()
        {
            if (jiggleZones == null)
            {
                return;
            }
            foreach (JiggleZone zone in jiggleZones)
            {
                zone.OnRenderObject();
            }
        }
    }
}