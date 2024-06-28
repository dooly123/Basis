using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace BattlePhaze.SettingsManager.AutoPerformance
{
    [AddComponentMenu("BattlePhaze/SettingsManager/BattlePhaze Auto Performance")]
    public class SettingsManagerAutoPerformance : SettingsManagerOption
    {
        [HideInInspector]
        private float CurrentAverageFps;
        public float TimeIntervalToAdaptQualitySettings = 5f;
        private int NumberOfDataPoints;
        [HideInInspector]
        public int Stability;
        private bool LastMovementWasDown;
        [HideInInspector]
        public int Flickering;
        [HideInInspector]
        public bool IsRunning;
        private SettingsMenuInput ActiveMasterQualityInput;
        private float HighFPSThreshold = 60;
        private float LowerFPSThreshold = 25f;
        public float RefreshSystemRate = 0.5f;
        [HideInInspector]
        public float FramesPerSec;
        public ThresholdChangeUpFrameRate FrameRateChangeUpController;
        [SerializeField]
        public ThresholdChangeDownFrameRate FrameRateChangeDownController;
        [SerializeField]
        public AutoPerformanceRestingState RestingState;
        public SettingsManager Manager;
        [SerializeField]
        public enum ThresholdChangeUpFrameRate
        {
            SetTargetFrameRate, AlmostVsync, Vsync, HalfVsync, Custom
        }
        public enum ThresholdChangeDownFrameRate
        {
            HalfMax, quarterMax, Custom
        }
        public enum AutoPerformanceRestingState
        {
            Off, On, OnAndSceneLoad
        }
        public void Start()
        {
            if (RestingState == AutoPerformanceRestingState.OnAndSceneLoad)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            foreach(SettingsMenuInput Input in Manager.Options)
            {
                if (Input.MasterQualityState == SettingsManagerEnums.MasterQualityState.MasterQualityOption)
                {
                    ActiveMasterQualityInput = Input;
                }
            }
            if (ActiveMasterQualityInput == null)
            {
                this.enabled = false;
            }
        }
        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Additive) { return; }
            Flickering = 0;
            UpdateFrameRate();
            if (Application.isPlaying)
            {
                StartCoroutine(AdaptQuality());
            }
        }
        void Update()
        {
            CurrentAverageFps += (1 / Time.deltaTime - CurrentAverageFps) / ++NumberOfDataPoints;
        }
        public void StartAutoPerformance()
        {
            if (IsRunning)
            {
                UpdateFrameRate();
                if (Application.isPlaying)
                {
                    StartCoroutine(AdaptQuality());
                }
            }
        }
        public override void ReceiveOption(SettingsMenuInput Option, SettingsManager Manager)
        {
            if (NameReturn(0, Option))
            {
                bool.TryParse(Option.SelectedValue, out bool CurrentState);
                if (CurrentState == true)
                {
                    IsRunning = true;
                    StartAutoPerformance();
                }
                else
                {
                    IsRunning = false;
                }
            }
        }
        public void ApplySettingsLevel(int value)
        {
            if (ActiveMasterQualityInput.SelectableValueList.Count - 1 >= value && value >= 0)
            {
                DebugSystem.SettingsManagerDebug.Log("Applying new Level " + value);
                SettingsManagerDropDown.SetOptionsValue(Manager, ActiveMasterQualityInput.OptionIndex, value, true);
                SettingsManagerDropDown.DropDownExecution(ActiveMasterQualityInput.OptionIndex, Manager, value, false);
            }
        }
        public void DecreaseQualityLevel()
        {
            for (int SelectableValueListIndex = 0; SelectableValueListIndex < Manager.Options[SelectableValueListIndex].SelectableValueList.Count; SelectableValueListIndex++)
            {
                if (ActiveMasterQualityInput.SelectedValue == ActiveMasterQualityInput.SelectableValueList[SelectableValueListIndex].RealValue)
                {
                    ApplySettingsLevel(SelectableValueListIndex - 1);
                    return;
                }
            }
        }
        public void IncreaseQualityLevel()
        {
            for (int SelectableValueListIndex = 0; SelectableValueListIndex < Manager.Options[SelectableValueListIndex].SelectableValueList.Count; SelectableValueListIndex++)
            {
                if (ActiveMasterQualityInput.SelectedValue == ActiveMasterQualityInput.SelectableValueList[SelectableValueListIndex].RealValue)
                {
                    ApplySettingsLevel(SelectableValueListIndex + 1);
                    return;
                }
            }
        }
        IEnumerator AdaptQuality()
        {
            yield return new WaitForSeconds(TimeIntervalToAdaptQualitySettings);
            if (IsRunning)
            {
                DebugSystem.SettingsManagerDebug.Log("Current Average Framerate is : " + CurrentAverageFps);
                if (CurrentAverageFps <= LowerFPSThreshold)
                {
                    UpdateFrameRate();
                    DecreaseQualityLevel();
                    --Stability;
                    if (LastMovementWasDown == false)
                    {
                        ++Flickering;
                    }
                    LastMovementWasDown = true;
                    if (Flickering > 1)
                    {
                        DebugSystem.SettingsManagerDebug.Log("Flickering detected, Disabling");
                        IsRunning = false;
                    }
                }
                else
                {
                    if (CurrentAverageFps >= HighFPSThreshold)
                    {
                        UpdateFrameRate();
                        IncreaseQualityLevel();
                        --Stability;
                        if (LastMovementWasDown)
                        {
                            ++Flickering;
                        }
                        LastMovementWasDown = false;
                    }
                    else
                    {
                        ++Stability;
                    }
                    if (Stability > 3)
                    {
                        DebugSystem.SettingsManagerDebug.Log("Flickering detected, Disabling");
                        IsRunning = false;
                    }
                    NumberOfDataPoints = 0;
                    CurrentAverageFps = 0;
                    if (Application.isPlaying)
                    {
                        StartCoroutine(AdaptQuality());
                    }
                }
            }
        }
        public void UpdateFrameRate()
        {
            switch (FrameRateChangeUpController)
            {
                case ThresholdChangeUpFrameRate.SetTargetFrameRate:
                    HighFPSThreshold = Application.targetFrameRate;
                    break;
                case ThresholdChangeUpFrameRate.Vsync:
                    HighFPSThreshold = Screen.currentResolution.refreshRate;
                    break;
                case ThresholdChangeUpFrameRate.AlmostVsync:
                    HighFPSThreshold = Screen.currentResolution.refreshRate / 1.35f;
                    break;
                case ThresholdChangeUpFrameRate.HalfVsync:
                    HighFPSThreshold = Screen.currentResolution.refreshRate / 2;
                    break;
            }
            switch (FrameRateChangeDownController)
            {
                case ThresholdChangeDownFrameRate.HalfMax:
                    LowerFPSThreshold = HighFPSThreshold / 2f;
                    break;
                case ThresholdChangeDownFrameRate.quarterMax:
                    LowerFPSThreshold = HighFPSThreshold / 4f;
                    break;
            }
        }
    }
}