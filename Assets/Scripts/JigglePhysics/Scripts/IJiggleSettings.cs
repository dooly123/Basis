using UnityEngine;

namespace JigglePhysics
{
    public interface IJiggleSettings
    {
        JiggleSettingsData GetData();
        float GetRadius(float normalizedIndex);
        void SetData(JiggleSettingsData data);
        void SetRadiusCurve(AnimationCurve curve);
    }
}