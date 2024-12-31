using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.Avatar;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Unity.Mathematics;

namespace Basis.Scripts.Drivers
{
    public abstract class BaseBoneDriver : MonoBehaviour
    {
        //figures out how to get the mouth bone and eye position
        public int ControlsLength;
        [SerializeField]
        public BasisBoneControl[] Controls;
        [SerializeField]
        public BasisBoneTrackedRole[] trackedRoles;
        public bool HasControls = false;
        public double ProvidedTime;
        public float DeltaTime;
        public delegate void SimulationHandler();
        public event SimulationHandler OnSimulate;
        public event SimulationHandler OnPostSimulate;
        public OrderedDelegate ReadyToRead = new OrderedDelegate();

        public Quaternion QatCalibrationHeading;
        public Vector3 CalibrationHeading;
        public float CalibrationHeadingY;
        /// <summary>
        /// call this after updating the bone data
        /// </summary>
        public void Simulate(double timeAsDouble,float deltaTime)
        {
            // sequence all other devices to run at the same time
            ProvidedTime = timeAsDouble;
            DeltaTime = deltaTime;
            if (float.IsNaN(DeltaTime))
            {
                return;
            }

            OnSimulate?.Invoke();
            for (int Index = 0; Index < ControlsLength; Index++)
            {
                Controls[Index].ComputeMovement(DeltaTime);
            }
            OnPostSimulate?.Invoke();
        }
        public void SimulateWithoutLerp()
        {
            // sequence all other devices to run at the same time
            ProvidedTime = Time.timeAsDouble;
            DeltaTime = Time.deltaTime;
            OnSimulate?.Invoke();
            for (int Index = 0; Index < ControlsLength; Index++)
            {
                Controls[Index].LastRunData.position = Controls[Index].OutGoingData.position;
                Controls[Index].LastRunData.rotation = Controls[Index].OutGoingData.rotation;
                Controls[Index].ComputeMovement(DeltaTime);
            }
            OnPostSimulate?.Invoke();
        }
        public void ApplyMovement()
        {
            for (int Index = 0; Index < ControlsLength; Index++)
            {
                Controls[Index].ApplyMovement();
            }
            ReadyToRead?.Invoke();
            if (BasisGizmoManager.UseGizmos)
            {
                for (int Index = 0; Index < ControlsLength; Index++)
                {
                    DrawGizmos(Controls[Index]);
                }
            }
        }
        public void SimulateAndApply(double timeAsDouble, float deltaTime)
        {
            Simulate( timeAsDouble, deltaTime);
            ApplyMovement();
        }
        public void SimulateAndApplyWithoutLerp()
        {
            SimulateWithoutLerp();
            ApplyMovement();
        }
        public void CalculateHeading()
        {
            if (FindBone(out BasisBoneControl Head, BasisBoneTrackedRole.Head))
            {
                CalibrationHeadingY = Head.BoneTransform.localRotation.eulerAngles.y;
                CalibrationHeading = new Vector3(0, CalibrationHeadingY, 0);
                QatCalibrationHeading = Quaternion.Euler(CalibrationHeading);
                Debug.DrawLine(Head.BoneTransform.position, Head.BoneTransform.position + (QatCalibrationHeading * new Vector3(0, 0, 1)), Color.black, 5f);
                //  Head.BoneModelTransform.position = Head.BoneTransform.position;
                //   Head.BoneModelTransform.rotation = Head.BoneTransform.rotation;
            }
        }
        public void RemoveAllListeners()
        {
            for (int Index = 0; Index < ControlsLength; Index++)
            {
                Controls[Index].OnHasRigChanged.RemoveAllListeners();
                Controls[Index].WeightsChanged.RemoveAllListeners();
            }
        }
        public void AddRange(BasisBoneControl[] newControls, BasisBoneTrackedRole[] newRoles)
        {
            Controls = Controls.Concat(newControls).ToArray();
            trackedRoles = trackedRoles.Concat(newRoles).ToArray();
            ControlsLength = Controls.Length;
        }
        public bool FindBone(out BasisBoneControl control, BasisBoneTrackedRole Role)
        {
            int Index = Array.IndexOf(trackedRoles, Role);

            if (Index >= 0 && Index < ControlsLength)
            {
                control = Controls[Index];
                return true;
            }

            control = new BasisBoneControl();
            return false;
        }
        public bool FindTrackedRole(BasisBoneControl control, out BasisBoneTrackedRole Role)
        {
            int Index = Array.IndexOf(Controls, control);

            if (Index >= 0 && Index < ControlsLength)
            {
                Role = trackedRoles[Index];
                return true;
            }

            Role = BasisBoneTrackedRole.CenterEye;
            return false;
        }
        public void CreateInitialArrays(Transform Parent, bool IsLocal)
        {
            trackedRoles = new BasisBoneTrackedRole[] { };
            Controls = new BasisBoneControl[] { };
            int Length;
            if (IsLocal)
            {
                 Length = Enum.GetValues(typeof(BasisBoneTrackedRole)).Length;
            }
            else
            {
                 Length = 6;
            }
            Color[] Colors = GenerateRainbowColors(Length);
            List<BasisBoneControl> newControls = new List<BasisBoneControl>();
            List<BasisBoneTrackedRole> Roles = new List<BasisBoneTrackedRole>();
            for (int Index = 0; Index < Length; Index++)
            {
                SetupRole(Index, Parent, Colors[Index],out BasisBoneControl Control,out BasisBoneTrackedRole Role);
                newControls.Add(Control);
                Roles.Add(Role);
            }
            if(IsLocal == false)
            {
                SetupRole(22, Parent,Color.blue, out BasisBoneControl Control, out BasisBoneTrackedRole Role);
                newControls.Add(Control);
                Roles.Add(Role);
            }
            AddRange(newControls.ToArray(), Roles.ToArray());
            HasControls = true;
            InitalzeGizmos();
        }
        public void SetupRole(int Index,Transform Parent, Color Color,out BasisBoneControl BasisBoneControl, out BasisBoneTrackedRole role)
        {
            role = (BasisBoneTrackedRole)Index;
            BasisBoneControl = new BasisBoneControl();
            GameObject TrackedBone = new GameObject(role.ToString());
            TrackedBone.transform.parent = Parent;
            BasisBoneControl.BoneTransform = TrackedBone.transform;
            BasisBoneControl.HasBone = true;
            BasisBoneControl.GeneralLocation = BasisAvatarIKStageCalibration.FindGeneralLocation(role);
            BasisBoneControl.Initialize();
            FillOutBasicInformation(BasisBoneControl, role.ToString(), Color);
        }
        public void InitalzeGizmos()
        {
           BasisGizmoManager.OnUseGizmosChanged += UpdatGizmoUsage;
        }
        public void DeInitalzeGizmos()
        {
           BasisGizmoManager.OnUseGizmosChanged -= UpdatGizmoUsage;
        }
        public void UpdatGizmoUsage(bool State)
        {
            BasisDebug.Log("Running Bone Driver Gizmos", BasisDebug.LogTag.Gizmo);
            // BasisDebug.Log("updating State!");
            for (int Index = 0; Index < ControlsLength; Index++)
            {
                BasisBoneControl Control = Controls[Index];
                BasisBoneTrackedRole Role = trackedRoles[Index];
                if (State)
                {
                    if(Role == BasisBoneTrackedRole.CenterEye)
                    {
                        continue;
                    }
                    Vector3 BonePosition = Control.OutgoingWorldData.position;
                    if (BasisBoneTrackedRoleCommonCheck.CheckIfRightHand(Role) || BasisBoneTrackedRoleCommonCheck.CheckIfLeftHand(Role))
                    {
                        if (BasisGizmoManager.CreateSphereGizmo(out Control.GizmoReference, BonePosition, HandGizmoSize * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale, Control.Color))
                        {
                            Control.HasGizmo = true;
                        }
                    }
                    else
                    {
                        if (Control.TargetControl.HasTarget)
                        {
                            if (BasisGizmoManager.CreateLineGizmo(out Control.TargetControl.LineDrawIndex, BonePosition, Control.TargetControl.Target.OutgoingWorldData.position, 0.03f, Control.Color))
                            {
                                Control.TargetControl.HasLineDraw = true;
                            }
                        }
                        if (BasisGizmoManager.CreateSphereGizmo(out Control.GizmoReference, BonePosition, DefaultGizmoSize * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale, Control.Color))
                        {
                            Control.HasGizmo = true;
                        }
                    }
                }
                else
                {
                    Control.HasGizmo = false;
                    Control.TposeHasGizmo = false;
                }
            }
        }
        public void FillOutBasicInformation(BasisBoneControl Control, string Name, Color Color)
        {
            Control.Name = Name;
            Control.Color = Color;
        }
        public Color[] GenerateRainbowColors(int RequestColorCount)
        {
            Color[] rainbowColors = new Color[RequestColorCount];

            for (int Index = 0; Index < RequestColorCount; Index++)
            {
                float hue = Mathf.Repeat(Index / (float)RequestColorCount, 1f);
                rainbowColors[Index] = Color.HSVToRGB(hue, 1f, 1f);
            }

            return rainbowColors;
        }
        public void CreateRotationalLock(BasisBoneControl addToBone, BasisBoneControl target, float lerpAmount, float positional = 40)
        {
            addToBone.TargetControl.Target = target;
            addToBone.TargetControl.LerpAmountNormal = lerpAmount;
            addToBone.TargetControl.LerpAmountFastMovement = lerpAmount * 4;
            addToBone.TargetControl.AngleBeforeSpeedup = 25f;
            addToBone.TargetControl.HasRotationalTarget = target != null;
            addToBone.TargetControl.Offset = addToBone.TposeLocal.position - target.TposeLocal.position;
            addToBone.TargetControl.Target = target;
            addToBone.TargetControl.LerpAmount = positional;
            addToBone.TargetControl.HasTarget = target != null;
        }
        public static Vector3 ConvertToAvatarSpaceInital(Animator animator, Vector3 WorldSpace, float AvatarHeightOffset)// out Vector3 FloorPosition
        {
            if (BasisHelpers.TryGetFloor(animator, out float3 Bottom))
            {
                //FloorPosition = Bottom;
                return BasisHelpers.ConvertToLocalSpace(WorldSpace + new Vector3(0f, AvatarHeightOffset, 0f), Bottom);
            }
            else
            {
                //FloorPosition = Vector3.zero;
                BasisDebug.LogError("Missing Avatar");
                return Vector3.zero;
            }
        }
        public static Vector3 ConvertToWorldSpace(Vector3 WorldSpace, Vector3 LocalSpace)
        {
            return BasisHelpers.ConvertFromLocalSpace(LocalSpace, WorldSpace);
        }
        public static float DefaultGizmoSize = 0.05f;
        public static float HandGizmoSize = 0.015f;
        public void DrawGizmos(BasisBoneControl Control)
        {
            if (Control.Cullable)
            {
                return;
            }
            if (Control.HasBone)
            {
                Vector3 BonePosition = Control.OutgoingWorldData.position;
                if (Control.TargetControl.HasTarget)
                {
                    if (Control.TargetControl.HasLineDraw)
                    {
                        BasisGizmoManager.UpdateLineGizmo(Control.TargetControl.LineDrawIndex, BonePosition, Control.TargetControl.Target.OutgoingWorldData.position);
                    }
                }
                if (BasisLocalPlayer.Instance.LocalBoneDriver.FindTrackedRole(Control, out BasisBoneTrackedRole Role))
                {
                    if(Role == BasisBoneTrackedRole.CenterEye)
                    {
                        //ignoring center eye to stop you having issues in vr
                        return;
                    }
                    if (Control.HasGizmo)
                    {
                        if (BasisGizmoManager.UpdateSphereGizmo(Control.GizmoReference, BonePosition) == false)
                        {
                            Control.HasGizmo = false;
                        }
                    }
                }
                if (BasisLocalPlayer.Instance.AvatarDriver.CurrentlyTposing)
                {
                    if (BasisLocalPlayer.Instance.LocalBoneDriver.FindTrackedRole(Control, out BasisBoneTrackedRole role))
                    {
                        if (Role == BasisBoneTrackedRole.CenterEye)
                        {
                            //ignoring center eye to stop you having issues in vr
                            return;
                        }
                        if (BasisBoneTrackedRoleCommonCheck.CheckItsFBTracker(role))
                        {
                            if (Control.TposeHasGizmo)
                            {
                                if (BasisGizmoManager.UpdateSphereGizmo(Control.TposeGizmoReference, BonePosition) == false)
                                {
                                    Control.TposeHasGizmo = false;
                                }
                            }
                            else
                            {
                                if (BasisGizmoManager.CreateSphereGizmo(out Control.TposeGizmoReference, BonePosition, BasisAvatarIKStageCalibration.MaxDistanceBeforeMax(role) * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale, Control.Color))
                                {
                                    Control.TposeHasGizmo = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        public class OrderedDelegate
        {
            private List<KeyValuePair<int, Action>> actions = new List<KeyValuePair<int, Action>>();
            private bool isSorted = true;

            // Add an action with a priority level.
            public void AddAction(int priority, Action action)
            {
                actions.Add(new KeyValuePair<int, Action>(priority, action));
                isSorted = false;  // Mark as unsorted to avoid sorting on every add.
            }

            // Remove a specific action from a given priority.
            public void RemoveAction(int priority, Action action)
            {
                for (int i = actions.Count - 1; i >= 0; i--)
                {
                    if (actions[i].Key == priority && actions[i].Value == action)
                    {
                        actions.RemoveAt(i);
                        break;
                    }
                }
            }

            // Invoke all actions in order of priority.
            public void Invoke()
            {
                // Sort only if we have added new items since the last invoke.
                if (!isSorted)
                {
                    actions.Sort((a, b) => a.Key.CompareTo(b.Key));
                    isSorted = true;
                }

                // Use a for loop instead of foreach to avoid allocations.
                for (int i = 0; i < actions.Count; i++)
                {
                    actions[i].Value?.Invoke();
                }
            }

        }
    }
}
