using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Basis.Scripts.TransformBinders.BoneControl;
using Basis.Scripts.Avatar;
using Basis.Scripts.BasisSdk.Helpers;
using Basis.Scripts.BasisSdk.Players;
using Gizmos = Popcron.Gizmos;
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
        public void Simulate()
        {
            // sequence all other devices to run at the same time
            ProvidedTime = Time.timeAsDouble;
            DeltaTime = Time.deltaTime;
            if (float.IsNaN(DeltaTime) || DeltaTime <= 0f)
            {
                DeltaTime = 0f;
                return; // Skip simulation if DeltaTime is invalid or zero
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
        }
        public void SimulateAndApply()
        {
            Simulate();
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
        public void CreateInitialArrays(Transform Parent)
        {
            trackedRoles = new BasisBoneTrackedRole[] { };
            Controls = new BasisBoneControl[] { };
            int Length = Enum.GetValues(typeof(BasisBoneTrackedRole)).Length;
            Color[] Colors = GenerateRainbowColors(Length);
            List<BasisBoneControl> newControls = new List<BasisBoneControl>();
            List<BasisBoneTrackedRole> Roles = new List<BasisBoneTrackedRole>();
            for (int Index = 0; Index < Length; Index++)
            {
                BasisBoneTrackedRole role = (BasisBoneTrackedRole)Index;
                BasisBoneControl Control = new BasisBoneControl();
                GameObject TrackedBone = new GameObject(role.ToString());
                TrackedBone.transform.parent = Parent;
                Control.BoneTransform = TrackedBone.transform;
                Control.HasBone = true;
                Control.GeneralLocation = BasisAvatarIKStageCalibration.FindGeneralLocation(role);
                Control.Initialize();
                FillOutBasicInformation(Control, role.ToString(), Colors[Index]);
                newControls.Add(Control);
                Roles.Add(role);
            }
            AddRange(newControls.ToArray(), Roles.ToArray());
            HasControls = true;
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
        public void CreateRotationalLock(BasisBoneControl addToBone, BasisBoneControl Target, float lerpAmount)
        {
            BasisRotationalControl rotation = new BasisRotationalControl
            {
                Target = Target,
                LerpAmountNormal = lerpAmount,
                LerpAmountFastMovement = lerpAmount * 4,
                AngleBeforeSpeedup = 25f,
                HasTarget = Target != null,
            };
            addToBone.RotationControl = rotation;
        }
        public void CreatePositionalLock(BasisBoneControl Bone, BasisBoneControl Target, float Positional = 40, bool CaresAboutX = false)
        {
            Vector3 Offset = Bone.TposeLocal.position - Target.TposeLocal.position;
            if (CaresAboutX == false)
            {
                Offset.x = 0;
            }
            BasisPositionControl Position = new BasisPositionControl
            {
                Offset = Offset,
                Target = Target,
                LerpAmount = Positional,
                HasTarget = Target != null,
            };
            Bone.PositionControl = Position;
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
                Debug.LogError("Missing Avatar");
                return Vector3.zero;
            }
        }
        public static Vector3 ConvertToWorldSpace(Vector3 WorldSpace, Vector3 LocalSpace)
        {
            return BasisHelpers.ConvertFromLocalSpace(LocalSpace, WorldSpace);
        }
        public static float DefaultGizmoSize = 0.05f;
        public static float HandGizmoSize = 0.015f;
        public void OnRenderObject()
        {
            if (HasControls && Gizmos.Enabled)
            {
                for (int Index = 0; Index < ControlsLength; Index++)
                {
                    BasisBoneControl Control = Controls[Index];
                    DrawGizmos(Control);
                }
            }
        }
        public void DrawGizmos(BasisBoneControl Control)
        {
            if (Control.Cullable)
            {
                return;
            }
            if (Control.HasBone)
            {
                Vector3 BonePosition = Control.OutgoingWorldData.position;
                if (Control.PositionControl.HasTarget)
                {
                    Gizmos.Line(BonePosition, Control.PositionControl.Target.OutgoingWorldData.position, Control.Color);
                }
                if (BasisLocalPlayer.Instance.LocalBoneDriver.FindTrackedRole(Control, out BasisBoneTrackedRole Frole))
                {
                    if (BasisBoneTrackedRoleCommonCheck.CheckIfRightHand(Frole) || BasisBoneTrackedRoleCommonCheck.CheckIfLeftHand(Frole))
                    {
                        Gizmos.Sphere(BonePosition, HandGizmoSize * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale, Control.Color);
                    }
                    else
                    {
                        Gizmos.Sphere(BonePosition, DefaultGizmoSize * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale, Control.Color);
                    }
                }
                if (BasisLocalPlayer.Instance.AvatarDriver.CurrentlyTposing)
                {
                    if (BasisLocalPlayer.Instance.LocalBoneDriver.FindTrackedRole(Control, out BasisBoneTrackedRole role))
                    {
                        Gizmos.Sphere(BonePosition, (BasisAvatarIKStageCalibration.MaxDistanceBeforeMax(role) / 2) * BasisLocalPlayer.Instance.EyeRatioAvatarToAvatarDefaultScale, Control.Color);
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