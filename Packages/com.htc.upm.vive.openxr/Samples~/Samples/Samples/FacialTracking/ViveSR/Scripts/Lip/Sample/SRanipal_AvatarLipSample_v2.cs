//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
using System.Collections.Generic;
using UnityEngine;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    public class SRanipal_AvatarLipSample_v2 : MonoBehaviour
    {
        [SerializeField] private List<LipShapeTable_v2> LipShapeTables;

        public bool NeededToGetData = true;
        private Dictionary<XrLipShapeHTC, float> LipWeightings;

        private void Start()
        {
            if (!SRanipal_Lip_Framework.Instance.EnableLip)
            {
                enabled = false;
                return;
            }
            SetLipShapeTables(LipShapeTables);
        }

        private void Update()
        {
            if (SRanipal_Lip_Framework.Status != SRanipal_Lip_Framework.FrameworkStatus.WORKING) return;
            if (NeededToGetData)
            {
                SRanipal_Lip_v2.GetLipWeightings(out LipWeightings);
                UpdateLipShapes(LipWeightings);
            }
        }

        public void SetLipShapeTables(List<LipShapeTable_v2> lipShapeTables)
        {
            bool valid = true;
            if (lipShapeTables == null)
            {
                valid = false;
            }
            else
            {
                for (int table = 0; table < lipShapeTables.Count; ++table)
                {
                    if (lipShapeTables[table].skinnedMeshRenderer == null)
                    {
                        valid = false;
                        break;
                    }
                    for (int shape = 0; shape < lipShapeTables[table].lipShapes.Length; ++shape)
                    {
                        XrLipShapeHTC lipShape = lipShapeTables[table].lipShapes[shape];
                        if (lipShape > XrLipShapeHTC.XR_LIP_SHAPE_MAX_ENUM_HTC || lipShape < 0)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
            }
            if (valid)
                LipShapeTables = lipShapeTables;
        }

        public void UpdateLipShapes(Dictionary<XrLipShapeHTC, float> lipWeightings)
        {
            foreach (var table in LipShapeTables)
                RenderModelLipShape(table, lipWeightings);
        }

        private void RenderModelLipShape(LipShapeTable_v2 lipShapeTable, Dictionary<XrLipShapeHTC, float> weighting)
        {
            for (int i = 0; i < lipShapeTable.lipShapes.Length; i++)
            {
                int targetIndex = (int)lipShapeTable.lipShapes[i];
                if (targetIndex > (int)XrLipShapeHTC.XR_LIP_SHAPE_MAX_ENUM_HTC || targetIndex < 0) continue;
                lipShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[(XrLipShapeHTC)targetIndex] * 100);
            }
        }
    }
}
