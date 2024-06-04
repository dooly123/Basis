using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(BasisAvatar))]
public class BasisAvatarSDKInspector : Editor
{
    public VisualTreeAsset visualTree;
    public BasisAvatar Avatar;
    public VisualElement uiElementsRoot;
    public bool AvatarEyePositionState = false;
    public bool AvatarMouthPositionState = false;
    public VisualElement rootElement;
    public AvatarSDKJiggleBonesView AvatarSDKJiggleBonesView = new AvatarSDKJiggleBonesView();
    private void OnEnable()
    {
        visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AvatarPathConstants.uxmlPath);
        Avatar = (BasisAvatar)target;
    }
    public override VisualElement CreateInspectorGUI()
    {
        Avatar = (BasisAvatar)target;
        rootElement = new VisualElement();
        if (visualTree != null)
        {
            uiElementsRoot = visualTree.CloneTree();
            rootElement.Add(uiElementsRoot);

            BasisAutomaticSetupAvatarEditor.TryToAutomatic(this);
            SetupItems();
            AvatarSDKJiggleBonesView.Initialize(this);
        }
        else
        {
            Debug.LogError("VisualTree is null. Make sure the UXML file is assigned correctly.");
        }
        return rootElement;
    }
    public void AutomaticallyFindVisemes()
    {
        SkinnedMeshRenderer Renderer = Avatar.FaceVisemeMesh;
        Avatar.FaceVisemeMovement = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        List<string> Names = AvatarHelper.FindAllNames(Renderer);
        foreach (KeyValuePair<string, int> Value in AvatarHelper.SearchForVisemeIndex)
        {
            if (AvatarHelper.GetBlendShapes(Names, Value.Key, out int OnMeshIndex))
            {
                Avatar.FaceVisemeMovement[Value.Value] = OnMeshIndex;
            }
        }
    }
    public void AutomaticallyFindBlinking()
    {
        SkinnedMeshRenderer Renderer = Avatar.FaceBlinkMesh;
        Avatar.FaceVisemeMovement = new int[] { };
        List<string> Names = AvatarHelper.FindAllNames(Renderer);
        List<int> ints = new List<int>();
        foreach (string Name in AvatarHelper.SearchForBlinkIndex)
        {
            AvatarHelper.GetBlendShapes(Names, Name, out int BlendShapeIndex);
            ints.Add(BlendShapeIndex);
        }
        Avatar.BlinkViseme = ints.ToArray();
    }
    public void ClickedAvatarEyePositionButton(Button Button)
    {
        AvatarEyePositionState = !AvatarEyePositionState;
        Button.text = "Eye Position Gizmo " + AvatarHelper.BoolToText(AvatarEyePositionState);
    }
    public void ClickedAvatarMouthPositionButton(Button Button)
    {
        AvatarMouthPositionState = !AvatarMouthPositionState;
        Button.text = "Mouth Position Gizmo " + AvatarHelper.BoolToText(AvatarMouthPositionState);
    }
    public void EventCallbackAnimator(ChangeEvent<Object> evt)
    {
        Avatar.Animator = (Animator)evt.newValue;
    }
    public void EventCallbackFaceBlinkMesh(ChangeEvent<Object> evt)
    {
        Avatar.FaceBlinkMesh = (SkinnedMeshRenderer)evt.newValue;
    }
    public void EventCallbackFaceVisemeMesh(ChangeEvent<Object> evt)
    {
        Avatar.FaceVisemeMesh = (SkinnedMeshRenderer)evt.newValue;
    }
    private void OnMouthHeightValueChanged(ChangeEvent<Vector2> evt)
    {
        Avatar.AvatarMouthPosition = new Vector3(0, evt.newValue.x, evt.newValue.y);
    }
    private void OnEyeHeightValueChanged(ChangeEvent<Vector2> evt)
    {
        Avatar.AvatarEyePosition = new Vector3(0, evt.newValue.x, evt.newValue.y);
    }
    private void OnSceneGUI()
    {
        BasisAvatarGizmoEditor.UpdateGizmos(this);
    }
    public void SetupItems()
    {
        Button avatarEyePositionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.avatarEyePositionButton);
        Button avatarMouthPositionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.avatarMouthPositionButton);
        Button AvatarAutomaticVisemeDetectionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.AvatarAutomaticVisemeDetection);
        Button AvatarAutomaticBlinkDetectionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.AvatarAutomaticBlinkDetection);

        EventCallback<ChangeEvent<Vector2>> eventCallbackAvatarEyePosition = BasisHelpersGizmo.CallBackVector2Field(uiElementsRoot, AvatarPathConstants.avatarEyePositionField, Avatar.AvatarEyePosition);
        EventCallback<ChangeEvent<Vector2>> eventCallbackAvatarMouthPosition = BasisHelpersGizmo.CallBackVector2Field(uiElementsRoot, AvatarPathConstants.avatarMouthPositionField, Avatar.AvatarMouthPosition);

        EventCallback<ChangeEvent<Object>> eventCallbackAnimatorField = BasisHelpersGizmo.ObjectField(uiElementsRoot, AvatarPathConstants.animatorField, Avatar.Animator);
        EventCallback<ChangeEvent<Object>> eventCallbackFaceBlinkMeshField = BasisHelpersGizmo.ObjectField(uiElementsRoot, AvatarPathConstants.faceBlinkMeshField, Avatar.FaceBlinkMesh);
        EventCallback<ChangeEvent<Object>> eventCallbackFaceVisemeMeshField = BasisHelpersGizmo.ObjectField(uiElementsRoot, AvatarPathConstants.faceVisemeMeshField, Avatar.FaceVisemeMesh);

        if (avatarEyePositionClick != null)
        {
            avatarEyePositionClick.clicked += () => ClickedAvatarEyePositionButton(avatarEyePositionClick);
        }
        if (avatarMouthPositionClick != null)
        {
            avatarMouthPositionClick.clicked += () => ClickedAvatarMouthPositionButton(avatarMouthPositionClick);
        }
        if (AvatarAutomaticVisemeDetectionClick != null)
        {
            AvatarAutomaticVisemeDetectionClick.clicked += () => AutomaticallyFindVisemes();
        }
        if (AvatarAutomaticBlinkDetectionClick != null)
        {
            AvatarAutomaticBlinkDetectionClick.clicked += () => AutomaticallyFindBlinking();
        }
        if (eventCallbackAvatarEyePosition != null)
        {
            eventCallbackAvatarEyePosition += OnEyeHeightValueChanged;
        }
        if (eventCallbackAvatarMouthPosition != null)
        {
            eventCallbackAvatarMouthPosition += OnMouthHeightValueChanged;
        }
        if (eventCallbackAnimatorField != null)
        {
            eventCallbackAnimatorField += EventCallbackAnimator;
        }
        if (eventCallbackFaceBlinkMeshField != null)
        {
            eventCallbackFaceBlinkMeshField += EventCallbackFaceBlinkMesh;
        }
        if (eventCallbackFaceVisemeMeshField != null)
        {
            eventCallbackFaceVisemeMeshField += EventCallbackFaceVisemeMesh;
        }
        avatarEyePositionClick.text = "Eye Position Gizmo " + AvatarHelper.BoolToText(AvatarEyePositionState);
        avatarMouthPositionClick.text = "Mouth Position Gizmo " + AvatarHelper.BoolToText(AvatarMouthPositionState);
    }
}