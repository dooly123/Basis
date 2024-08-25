using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scenes.Garden_doolys_scene_name_that_cant_be_replicated_by_u_wahhaha.Scripts.Runtime
{
[ExecuteAlways]
public class DynamicLightController : MonoBehaviour
{
    [SerializeField]
    float maxDisplayDistance = 50f;

    [SerializeField]
    float attenuationDistance = 5f;

    [SerializeField]
    AnimationCurve intensityCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField]
    AnimationCurve rangeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    private Light m_light;
    private LightFlicker m_flicker;
    private float m_origIntensity;
    private float m_origRange;

    void Awake()
    {
        m_light = GetComponent<Light>();
        m_origIntensity = m_light.intensity;
        m_origRange = m_light.range;
        m_flicker = m_light.GetComponent<LightFlicker>();

        DynamicLightsManager.AddLight(this);
    }

    private void OnDestroy()
    {
        DynamicLightsManager.RemoveLight(this);
    }

    public void UpdateForCamera(Camera _camera)
    {
        if (!enabled)
            return;


        var distance = Vector3.Distance(_camera.transform.position, transform.position);

        if (distance < maxDisplayDistance)
        {
            m_light.enabled = true;
            var fadeFactor = 1f - Mathf.InverseLerp(maxDisplayDistance - attenuationDistance, maxDisplayDistance, distance);

            // If the light has a flicked script, use the value modified by the flicker as a base
            m_light.intensity = intensityCurve.Evaluate( fadeFactor ) * ((m_flicker != null) ? m_flicker.modifiedIntensity : m_origIntensity);

            m_light.range = rangeCurve.Evaluate( fadeFactor ) * m_origRange;
        }
        else
        {
            m_light.enabled = false;
        }
    }

    public void Reset()
    {
        m_light.enabled = enabled;
        m_light.intensity = (m_flicker != null) ? m_flicker.modifiedIntensity : m_origIntensity;
        m_light.range = m_origRange;
    }
}

public class DynamicLightsManager
{
    static DynamicLightsManager m_instance;
    public static DynamicLightsManager instance
    {
        get
        { 
            if(m_instance == null)
                m_instance = new DynamicLightsManager();

            return m_instance;
        }
    }

    static bool ignoreSceneCamera = true;

    static List<DynamicLightController> m_dynamicLightsControllers = new List<DynamicLightController>();

    [RuntimeInitializeOnLoadMethod]
    static void Init()
    {
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        Application.quitting += () => RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering ;
    }

    static void OnBeginCameraRendering(ScriptableRenderContext _ctx, Camera _camera)
    {
#if UNITY_EDITOR
        if (ignoreSceneCamera && _camera.name == "SceneCamera")
            ResetAllLights();
        else
#endif
            UpdateLightsForCamera(_camera);
    }

    static void UpdateLightsForCamera(Camera _camera)
    {
        foreach (var l in m_dynamicLightsControllers)
            l.UpdateForCamera(_camera);
    }

    static void ResetAllLights()
    {
        foreach (var l in m_dynamicLightsControllers)
            l.Reset();
    }

    public static void AddLight(DynamicLightController dynamicLightController )
    {
        if (!m_dynamicLightsControllers.Contains(dynamicLightController))
            m_dynamicLightsControllers.Add(dynamicLightController);
    }

    public static void RemoveLight(DynamicLightController dynamicLightController)
    {
        if (m_dynamicLightsControllers.Contains(dynamicLightController))
            m_dynamicLightsControllers.Remove(dynamicLightController);
    }

    public static void Clear()
    {
        m_dynamicLightsControllers.Clear();
    }
}
}