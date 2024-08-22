using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Helpers.Editor;
using Basis.Scripts.Editor;
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
        Undo.RecordObject(Avatar, "Automatically Find Visemes");
        Avatar.FaceVisemeMovement = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        List<string> Names = AvatarHelper.FindAllNames(Renderer);
        foreach (KeyValuePair<string, int> Value in AvatarHelper.SearchForVisemeIndex)
        {
            if (AvatarHelper.GetBlendShapes(Names, Value.Key, out int OnMeshIndex))
            {
                Avatar.FaceVisemeMovement[Value.Value] = OnMeshIndex;
            }
        }
        EditorUtility.SetDirty(Avatar);
        AssetDatabase.Refresh();
    }

    public void AutomaticallyFindBlinking()
    {
        SkinnedMeshRenderer Renderer = Avatar.FaceBlinkMesh;
        Undo.RecordObject(Avatar, "Automatically Find Blinking");
        Avatar.BlinkViseme = new int[] { };
        List<string> Names = AvatarHelper.FindAllNames(Renderer);
        List<int> Ints = new List<int>();
        foreach (string Name in AvatarHelper.SearchForBlinkIndex)
        {
            AvatarHelper.GetBlendShapes(Names, Name, out int BlendShapeIndex);
            Ints.Add(BlendShapeIndex);
        }
        Avatar.BlinkViseme = Ints.ToArray();
        EditorUtility.SetDirty(Avatar);
        AssetDatabase.Refresh();
    }

    public void ClickedAvatarEyePositionButton(Button Button)
    {
        Undo.RecordObject(Avatar, "Toggle Eye Position Gizmo");
        AvatarEyePositionState = !AvatarEyePositionState;
        Button.text = "Eye Position Gizmo " + AvatarHelper.BoolToText(AvatarEyePositionState);
        EditorUtility.SetDirty(Avatar);
    }

    public void ClickedAvatarMouthPositionButton(Button Button)
    {
        Undo.RecordObject(Avatar, "Toggle Mouth Position Gizmo");
        AvatarMouthPositionState = !AvatarMouthPositionState;
        Button.text = "Mouth Position Gizmo " + AvatarHelper.BoolToText(AvatarMouthPositionState);
        EditorUtility.SetDirty(Avatar);
    }

    public void EventCallbackAnimator(ChangeEvent<Object> evt)
    {
        Undo.RecordObject(Avatar, "Change Animator");
        Avatar.Animator = (Animator)evt.newValue;
        EditorUtility.SetDirty(Avatar);
    }

    public void EventCallbackFaceBlinkMesh(ChangeEvent<Object> evt)
    {
        Undo.RecordObject(Avatar, "Change Face Blink Mesh");
        Avatar.FaceBlinkMesh = (SkinnedMeshRenderer)evt.newValue;
        EditorUtility.SetDirty(Avatar);
    }

    public void EventCallbackFaceVisemeMesh(ChangeEvent<Object> evt)
    {
        Undo.RecordObject(Avatar, "Change Face Viseme Mesh");
        Avatar.FaceVisemeMesh = (SkinnedMeshRenderer)evt.newValue;
        EditorUtility.SetDirty(Avatar);
    }

    private void OnMouthHeightValueChanged(ChangeEvent<Vector2> evt)
    {
        Undo.RecordObject(Avatar, "Change Mouth Height");
        Avatar.AvatarMouthPosition = new Vector3(0, evt.newValue.x, evt.newValue.y);
        EditorUtility.SetDirty(Avatar);
    }

    private void OnEyeHeightValueChanged(ChangeEvent<Vector2> evt)
    {
        Undo.RecordObject(Avatar, "Change Eye Height");
        Avatar.AvatarEyePosition = new Vector3(0, evt.newValue.x, evt.newValue.y);
        EditorUtility.SetDirty(Avatar);
    }

    private void OnSceneGUI()
    {
        BasisAvatar avatar = (BasisAvatar)target;
        BasisAvatarGizmoEditor.UpdateGizmos(this, avatar);
    }

    public void SetupItems()
    {
        Button avatarEyePositionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.avatarEyePositionButton);
        Button avatarMouthPositionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.avatarMouthPositionButton);
        Button AvatarAutomaticVisemeDetectionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.AvatarAutomaticVisemeDetection);
        Button AvatarAutomaticBlinkDetectionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.AvatarAutomaticBlinkDetection);
        Button AvatarAvatarBuildBundleClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.AvatarBuildBundle);

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
        if (AvatarAvatarBuildBundleClick != null)
        {
            AvatarAvatarBuildBundleClick.clicked += () => EventCallbackAvatarAvatarBuildBundleClick(AvatarAvatarBuildBundleClick);
        }

        avatarEyePositionClick.text = "Eye Position Gizmo " + AvatarHelper.BoolToText(AvatarEyePositionState);
        avatarMouthPositionClick.text = "Mouth Position Gizmo " + AvatarHelper.BoolToText(AvatarMouthPositionState);
    }
    public void EventCallbackAvatarAvatarBuildBundleClick(Button Button)
    {
        BasisAddressableBuildPipeline.CreateAddressableForPrefab(BuildTarget.StandaloneWindows64, Avatar, Avatar.gameObject);
    }
}