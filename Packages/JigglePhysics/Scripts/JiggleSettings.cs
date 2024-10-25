using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JigglePhysics
{
public class JiggleSettings : JiggleSettingsBase {
    [SerializeField] [Range(0f,2f)] [Tooltip("How much gravity to apply to the simulation, it is a multiplier of the Physics.gravity setting.")]
    private float gravityMultiplier = 1f;
    [SerializeField] [Range(0f,1f)] [Tooltip("How much mechanical friction to apply, this is specifically how quickly oscillations come to rest.")]
    private float friction = 0.4f;
    [SerializeField] [Range(0f,1f)] [Tooltip("How much angular force is applied to bring it to the target shape.")]
    private float angleElasticity = 0.4f;
    [SerializeField] [Range(0f,1f)] [Tooltip("How much of the simulation should be expressed. A value of 0 would make the jiggle have zero effect. A value of 1 gives the full movement as intended. 0.5 would ")]
    private float blend = 1f;
    [FormerlySerializedAs("airFriction")] [HideInInspector] [SerializeField] [Range(0f, 1f)] [Tooltip( "How much jiggled objects should get dragged behind by moving through the air. Or how \"thick\" the air is.")]
    private float airDrag = 0.1f;
    [HideInInspector] [SerializeField] [Range(0f, 1f)] [Tooltip( "How rigidly the rig holds its length. Low values cause lots of squash and stretch!")]
    private float lengthElasticity = 0.8f;
    [HideInInspector] [SerializeField] [Range(0f, 1f)] [Tooltip("How much to allow free bone motion before engaging elasticity.")]
    private float elasticitySoften = 0f;
    [HideInInspector] [SerializeField] [Tooltip("How much radius points have, only used for collisions. Set to 0 to disable collisions")]
    private float radiusMultiplier = 0f;
    [HideInInspector] [SerializeField] [Tooltip("How the radius is expressed as a curve along the bone chain from root to child.")]
    private AnimationCurve radiusCurve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));

    public override JiggleSettingsData GetData() {
        return new JiggleSettingsData {
            gravityMultiplier = gravityMultiplier,
            friction = friction,
            airDrag = airDrag,
            blend = blend,
            angleElasticity = angleElasticity,
            elasticitySoften = elasticitySoften,
            lengthElasticity = lengthElasticity,
            radiusMultiplier = radiusMultiplier,
        };
    }
    public void SetData(JiggleSettingsData data) {
        gravityMultiplier = data.gravityMultiplier;
        friction = data.friction;
        angleElasticity = data.angleElasticity;
        blend = data.blend;
        airDrag = data.airDrag;
        lengthElasticity = data.lengthElasticity;
        elasticitySoften = data.elasticitySoften;
        radiusMultiplier = data.radiusMultiplier;
    }
    public override float GetRadius(float normalizedIndex) {
        return radiusMultiplier * radiusCurve.Evaluate(normalizedIndex);
    }
    public void SetRadiusCurve(AnimationCurve curve) {
        radiusCurve = curve;
    }

    [Serializable]
    private struct KeyframeData {
        public KeyframeData(Keyframe frame) {
            time = frame.time;
            value = frame.value;
            inTangent = frame.inTangent;
            outTangent = frame.outTangent;
            weightedMode = (int)frame.weightedMode;
            inWeight = frame.inWeight;
            outWeight = frame.outWeight;
        }
        [SerializeField] private float time;
        [SerializeField] private float value;
        [SerializeField] private float inTangent;
        [SerializeField] private float outTangent;
        [SerializeField] private int weightedMode;
        [SerializeField] private float inWeight;
        [SerializeField] private float outWeight;
        public Keyframe ToKeyframe() {
            return new Keyframe {
                time = time,
                value = value,
                inTangent = inTangent,
                outTangent = outTangent,
                weightedMode = (WeightedMode)weightedMode,
                inWeight = inWeight,
                outWeight = outWeight
            };
        }
    }

    [Serializable]
    private struct AnimationCurveData {
        public AnimationCurveData(AnimationCurve target) {
            List<KeyframeData> keyframeDatas = new List<KeyframeData>();
            foreach (var key in target.keys) {
                keyframeDatas.Add(new KeyframeData(key));
            }
            this.keyframeDatas = keyframeDatas.ToArray();
            preWrapMode = target.preWrapMode;
            postWrapMode = target.postWrapMode;
        }
        [SerializeField] private KeyframeData[] keyframeDatas;
        [SerializeField] private WrapMode preWrapMode;
        [SerializeField] private WrapMode postWrapMode;
        public AnimationCurve ToCurve() {
            List<Keyframe> keyframes = new List<Keyframe>();
            foreach (var keyData in keyframeDatas) {
                keyframes.Add(keyData.ToKeyframe());
            }
            return new AnimationCurve() {
                keys = keyframes.ToArray(),
                preWrapMode = preWrapMode,
                postWrapMode = postWrapMode,
            };
        }
    }

    [Serializable]
    private struct SettingsData {
        public SettingsData(JiggleSettings settings) {
            gravityMultiplier = settings.gravityMultiplier;
            friction = settings.friction;
            angleElasticity = settings.angleElasticity;
            blend = settings.blend;
            airDrag = settings.airDrag;
            lengthElasticity = settings.lengthElasticity;
            elasticitySoften = settings.elasticitySoften;
            radiusMultiplier = settings.radiusMultiplier;
            radiusCurve = new AnimationCurveData(settings.radiusCurve);
        }

        [SerializeField] private float gravityMultiplier;
        [SerializeField] private float friction;
        [SerializeField] private float angleElasticity;
        [SerializeField] private float blend;
        [SerializeField] private float airDrag;
        [SerializeField] private float lengthElasticity;
        [SerializeField] private float elasticitySoften;
        [SerializeField] private float radiusMultiplier;
        [SerializeField] private AnimationCurveData radiusCurve;

        public void ApplyTo(JiggleSettings target) {
            target.gravityMultiplier = gravityMultiplier;
            target.friction = friction;
            target.angleElasticity = angleElasticity;
            target.blend = blend;
            target.airDrag = airDrag;
            target.lengthElasticity = lengthElasticity;
            target.elasticitySoften = elasticitySoften;
            target.radiusMultiplier = radiusMultiplier;
            target.radiusCurve = radiusCurve.ToCurve();
        }
    }
}

}