//========= Copyright 2018, HTC Corporation. All rights reserved. ===========

using System.Collections.Generic;
using UnityEngine;
using VIVE.OpenXR.FacialTracking;

namespace VIVE.OpenXR.Samples.FacialTracking
{
    public class SRanipal_AvatarEyeSample_v2 : MonoBehaviour
    {
        [SerializeField] private Transform[] EyesModels = new Transform[0];
        [SerializeField] private List<EyeShapeTable_v2> EyeShapeTables;
        /// <summary>
        /// Customize this curve to fit the blend shapes of your avatar.
        /// </summary>
        [SerializeField] private AnimationCurve EyebrowAnimationCurveUpper;
        /// <summary>
        /// Customize this curve to fit the blend shapes of your avatar.
        /// </summary>
        [SerializeField] private AnimationCurve EyebrowAnimationCurveLower;
        /// <summary>
        /// Customize this curve to fit the blend shapes of your avatar.
        /// </summary>
        [SerializeField] private AnimationCurve EyebrowAnimationCurveHorizontal;

        public bool NeededToGetData = true;
        private Dictionary<XrEyeShapeHTC, float> EyeWeightings = new Dictionary<XrEyeShapeHTC, float>();
        //Map Openxr eye shape to Avatar eye blendshape
        private static Dictionary<EyeShape_v2, XrEyeShapeHTC> ShapeMap;
        private AnimationCurve[] EyebrowAnimationCurves = new AnimationCurve[(int)EyeShape_v2.Max];
        private GameObject[] EyeAnchors;
        private const int NUM_OF_EYES = 2;
        static SRanipal_AvatarEyeSample_v2()
        {
            ShapeMap = new Dictionary<EyeShape_v2, XrEyeShapeHTC>();
            ShapeMap.Add(EyeShape_v2.Eye_Left_Blink, XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_BLINK_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Left_Wide, XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_WIDE_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Right_Blink, XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_BLINK_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Right_Wide, XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_WIDE_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Left_Squeeze, XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_SQUEEZE_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Right_Squeeze, XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_SQUEEZE_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Left_Down, XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_DOWN_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Right_Down, XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_DOWN_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Left_Left, XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_OUT_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Right_Left, XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_IN_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Left_Right, XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_IN_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Right_Right, XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_OUT_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Left_Up, XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_UP_HTC);
            ShapeMap.Add(EyeShape_v2.Eye_Right_Up, XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_UP_HTC);
        }
        private void Start()
        {
            if (!SRanipal_Eye_Framework.Instance.EnableEye)
            {
                enabled = false;
                return;
            }

            SetEyesModels(EyesModels[0], EyesModels[1]);
            SetEyeShapeTables(EyeShapeTables);

            AnimationCurve[] curves = new AnimationCurve[(int)EyeShape_v2.Max];
            for (int i = 0; i < EyebrowAnimationCurves.Length; ++i)
            {
                if (i == (int)EyeShape_v2.Eye_Left_Up || i == (int)EyeShape_v2.Eye_Right_Up) curves[i] = EyebrowAnimationCurveUpper;
                else if (i == (int)EyeShape_v2.Eye_Left_Down || i == (int)EyeShape_v2.Eye_Right_Down) curves[i] = EyebrowAnimationCurveLower;
                else curves[i] = EyebrowAnimationCurveHorizontal;
            }
            SetEyeShapeAnimationCurves(curves);
        }

        private void Update()
        {
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

            if (NeededToGetData)
            {
                SRanipal_Eye_v2.GetEyeWeightings(out EyeWeightings);
                UpdateEyeShapes(EyeWeightings);


                Vector3 GazeDirectionCombinedLocal = Vector3.zero;
                if (EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_IN_HTC] > EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_OUT_HTC])
                {
                    GazeDirectionCombinedLocal.x = EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_IN_HTC];
                }
                else
                {
                    GazeDirectionCombinedLocal.x = -EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_OUT_HTC];
                }
                if (EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_UP_HTC] > EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_DOWN_HTC])
                {
                    GazeDirectionCombinedLocal.y = EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_UP_HTC];
                }
                else
                {
                    GazeDirectionCombinedLocal.y = -EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_LEFT_DOWN_HTC];
                }
                GazeDirectionCombinedLocal.z = (float)1.0;

                Vector3 target = EyeAnchors[0].transform.TransformPoint(GazeDirectionCombinedLocal);
                EyesModels[0].LookAt(target);

                if (EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_IN_HTC] > EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_OUT_HTC])
                {
                    GazeDirectionCombinedLocal.x = -EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_IN_HTC];
                }
                else
                {
                    GazeDirectionCombinedLocal.x = EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_OUT_HTC];
                }
                if (EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_UP_HTC] > EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_DOWN_HTC])
                {
                    GazeDirectionCombinedLocal.y = EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_UP_HTC];
                }
                else
                {
                    GazeDirectionCombinedLocal.y = -EyeWeightings[XrEyeShapeHTC.XR_EYE_EXPRESSION_RIGHT_DOWN_HTC];
                }
                GazeDirectionCombinedLocal.z = (float)1.0;

                target = EyeAnchors[1].transform.TransformPoint(GazeDirectionCombinedLocal);
                EyesModels[1].LookAt(target);
            }
        }
        private void Release()
        {
        }
        private void OnDestroy()
        {
            DestroyEyeAnchors();
        }

        public void SetEyesModels(Transform leftEye, Transform rightEye)
        {
            if (leftEye != null && rightEye != null)
            {
                EyesModels = new Transform[NUM_OF_EYES] { leftEye, rightEye };
                DestroyEyeAnchors();
                CreateEyeAnchors();
            }
        }

        public void SetEyeShapeTables(List<EyeShapeTable_v2> eyeShapeTables)
        {
            bool valid = true;
            if (eyeShapeTables == null)
            {
                valid = false;
            }
            else
            {
                for (int table = 0; table < eyeShapeTables.Count; ++table)
                {
                    if (eyeShapeTables[table].skinnedMeshRenderer == null)
                    {
                        valid = false;
                        break;
                    }
                    for (int shape = 0; shape < eyeShapeTables[table].eyeShapes.Length; ++shape)
                    {
                        EyeShape_v2 eyeShape = eyeShapeTables[table].eyeShapes[shape];
                        if (eyeShape > EyeShape_v2.Max || eyeShape < 0)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
            }
            if (valid)
                EyeShapeTables = eyeShapeTables;
        }

        public void SetEyeShapeAnimationCurves(AnimationCurve[] eyebrowAnimationCurves)
        {
            if (eyebrowAnimationCurves.Length == (int)EyeShape_v2.Max)
                EyebrowAnimationCurves = eyebrowAnimationCurves;
        }

        public void UpdateEyeShapes(Dictionary<XrEyeShapeHTC, float> eyeWeightings)
        {
            foreach (var table in EyeShapeTables)
                RenderModelEyeShape(table, eyeWeightings);
        }

        private void RenderModelEyeShape(EyeShapeTable_v2 eyeShapeTable, Dictionary<XrEyeShapeHTC, float> weighting)
        {
            for (int i = 0; i < eyeShapeTable.eyeShapes.Length; ++i)
            {
                EyeShape_v2 eyeShape = eyeShapeTable.eyeShapes[i];
                if (eyeShape > EyeShape_v2.Max || eyeShape < 0 || !ShapeMap.ContainsKey(eyeShape)) continue;
                XrEyeShapeHTC xreyeshape = ShapeMap[eyeShape];
                if (eyeShape == EyeShape_v2.Eye_Left_Blink || eyeShape == EyeShape_v2.Eye_Right_Blink)
                {

                    eyeShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, weighting[xreyeshape] * 100f);
                }
                else
                {
                    AnimationCurve curve = EyebrowAnimationCurves[(int)eyeShape];
                    eyeShapeTable.skinnedMeshRenderer.SetBlendShapeWeight(i, curve.Evaluate(weighting[xreyeshape]) * 100f);
                }


            }
        }

        private void CreateEyeAnchors()
        {
            EyeAnchors = new GameObject[NUM_OF_EYES];
            for (int i = 0; i < NUM_OF_EYES; ++i)
            {
                EyeAnchors[i] = new GameObject();
                EyeAnchors[i].name = "EyeAnchor_" + i;
                EyeAnchors[i].transform.SetParent(gameObject.transform);
                EyeAnchors[i].transform.localPosition = EyesModels[i].localPosition;
                EyeAnchors[i].transform.localRotation = EyesModels[i].localRotation;
                EyeAnchors[i].transform.localScale = EyesModels[i].localScale;
            }
        }

        private void DestroyEyeAnchors()
        {
            if (EyeAnchors != null)
            {
                foreach (var obj in EyeAnchors)
                    if (obj != null) Destroy(obj);
            }
        }
    }
}
