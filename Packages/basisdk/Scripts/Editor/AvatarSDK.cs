using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Helpers.Editor;
using Basis.Scripts.Editor;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomEditor(typeof(BasisAvatar))]
public partial class BasisAvatarSDKInspector : Editor
{
    public VisualTreeAsset visualTree;
    public BasisAvatar Avatar;
    public VisualElement uiElementsRoot;
    public bool AvatarEyePositionState = false;
    public bool AvatarMouthPositionState = false;
    public VisualElement rootElement;
    public AvatarSDKJiggleBonesView AvatarSDKJiggleBonesView = new AvatarSDKJiggleBonesView();
    public AvatarSDKVisemes AvatarSDKVisemes = new AvatarSDKVisemes();
    public Button EventCallbackAvatarBundleButton { get; private set; }

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
            AvatarSDKVisemes.Initialize(this);
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
        AvatarSDKVisemes.Initialize(this);
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
        AvatarSDKVisemes.Initialize(this);
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

    public void EventCallbackAnimator(ChangeEvent<UnityEngine.Object> evt, ref Animator Renderer)
    {
        Debug.Log(nameof(EventCallbackAnimator));
        Undo.RecordObject(Avatar, "Change Animator");
        Renderer = (Animator)evt.newValue;
        // Check if the Avatar is part of a prefab
        if (PrefabUtility.IsPartOfPrefabInstance(Avatar))
        {
            // Record the prefab modification
            PrefabUtility.RecordPrefabInstancePropertyModifications(Avatar);
        }
        EditorUtility.SetDirty(Avatar);
    }

    public void EventCallbackFaceVisemeMesh(ChangeEvent<UnityEngine.Object> evt, ref SkinnedMeshRenderer Renderer)
    {
        Debug.Log(nameof(EventCallbackFaceVisemeMesh));
        Undo.RecordObject(Avatar, "Change Face Viseme Mesh");
        Renderer = (SkinnedMeshRenderer)evt.newValue;

        // Check if the Avatar is part of a prefab
        if (PrefabUtility.IsPartOfPrefabInstance(Avatar))
        {
            // Record the prefab modification
            PrefabUtility.RecordPrefabInstancePropertyModifications(Avatar);
        }
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
        // Initialize Buttons
        Button avatarEyePositionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.avatarEyePositionButton);
        Button avatarMouthPositionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.avatarMouthPositionButton);
        Button avatarBundleButton = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.AvatarBundleButton);
        Button avatarAutomaticVisemeDetectionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.AvatarAutomaticVisemeDetection);
        Button avatarAutomaticBlinkDetectionClick = BasisHelpersGizmo.Button(uiElementsRoot, AvatarPathConstants.AvatarAutomaticBlinkDetection);

        // Initialize Event Callbacks for Vector2 fields (for Avatar Eye and Mouth Position)
        EventCallback<ChangeEvent<Vector2>> eventCallbackAvatarEyePosition = BasisHelpersGizmo.CallBackVector2Field(uiElementsRoot, AvatarPathConstants.avatarEyePositionField, Avatar.AvatarEyePosition);
        EventCallback<ChangeEvent<Vector2>> eventCallbackAvatarMouthPosition = BasisHelpersGizmo.CallBackVector2Field(uiElementsRoot, AvatarPathConstants.avatarMouthPositionField, Avatar.AvatarMouthPosition);

        // Initialize ObjectFields and assign references
        ObjectField animatorField = uiElementsRoot.Q<ObjectField>(AvatarPathConstants.animatorField);
        ObjectField faceBlinkMeshField = uiElementsRoot.Q<ObjectField>(AvatarPathConstants.FaceBlinkMeshField);
        ObjectField faceVisemeMeshField = uiElementsRoot.Q<ObjectField>(AvatarPathConstants.FaceVisemeMeshField);

        TextField AvatarNameField = uiElementsRoot.Q<TextField>(AvatarPathConstants.AvatarName);
        TextField AvatarDescriptionField = uiElementsRoot.Q<TextField>(AvatarPathConstants.AvatarDescription);

        TextField AvatarpasswordField = uiElementsRoot.Q<TextField>(AvatarPathConstants.Avatarpassword);

        ObjectField AvatarIconField = uiElementsRoot.Q<ObjectField>(AvatarPathConstants.AvatarIcon);

        animatorField.allowSceneObjects = true;
        faceBlinkMeshField.allowSceneObjects = true;
        faceVisemeMeshField.allowSceneObjects = true;
        AvatarIconField.allowSceneObjects = true;

        AvatarIconField.value = null;
        animatorField.value = Avatar.Animator;
        faceBlinkMeshField.value = Avatar.FaceBlinkMesh;
        faceVisemeMeshField.value = Avatar.FaceVisemeMesh;

        AvatarNameField.value = Avatar.BasisBundleDescription.AssetBundleName;
        AvatarDescriptionField.value = Avatar.BasisBundleDescription.AssetBundleDescription;

        AvatarNameField.RegisterCallback<ChangeEvent<string>>(AvatarName);
        AvatarDescriptionField.RegisterCallback<ChangeEvent<string>>(AvatarDescription);
        AvatarpasswordField.RegisterCallback<ChangeEvent<string>>(AvatarPassword);

        AvatarIconField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(OnAssignTexture2D);

        // Button click events
        avatarEyePositionClick.clicked += () => ClickedAvatarEyePositionButton(avatarEyePositionClick);
        avatarMouthPositionClick.clicked += () => ClickedAvatarMouthPositionButton(avatarMouthPositionClick);
        avatarAutomaticVisemeDetectionClick.clicked += AutomaticallyFindVisemes;
        avatarAutomaticBlinkDetectionClick.clicked += AutomaticallyFindBlinking;
        avatarBundleButton.clicked += EventCallbackAvatarBundle;

        // Register change events
        eventCallbackAvatarEyePosition += OnEyeHeightValueChanged;
        eventCallbackAvatarMouthPosition += OnMouthHeightValueChanged;

        // Register Animator field change event
        animatorField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(evt => EventCallbackAnimator(evt, ref Avatar.Animator));

        // Register Blink and Viseme Mesh field change events
        faceBlinkMeshField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(evt => EventCallbackFaceVisemeMesh(evt, ref Avatar.FaceBlinkMesh));
        faceVisemeMeshField.RegisterCallback<ChangeEvent<UnityEngine.Object>>(evt => EventCallbackFaceVisemeMesh(evt, ref Avatar.FaceVisemeMesh));

        // Update Button Text
        avatarEyePositionClick.text = "Eye Position Gizmo " + AvatarHelper.BoolToText(AvatarEyePositionState);
        avatarMouthPositionClick.text = "Mouth Position Gizmo " + AvatarHelper.BoolToText(AvatarMouthPositionState);
    }
    public Texture2D Texture;
    public void OnAssignTexture2D(ChangeEvent<UnityEngine.Object> Texture2D)
    {
        Texture = (Texture2D)Texture2D.newValue;
    }
    public string Password;
    public void AvatarPassword(ChangeEvent<string> evt)
    {
        Password = evt.newValue;
    }
    public void AvatarDescription(ChangeEvent<string> evt)
    {
        Avatar.BasisBundleDescription.AssetBundleDescription = evt.newValue;
        EditorUtility.SetDirty(Avatar);
        AssetDatabase.Refresh();
    }
    public void AvatarName(ChangeEvent<string> evt)
    {
        Avatar.BasisBundleDescription.AssetBundleName = evt.newValue;
        EditorUtility.SetDirty(Avatar);
        AssetDatabase.Refresh();
    }
    private void EventCallbackAvatarBundle()
    {
        if (string.IsNullOrEmpty(Password))
        {
            Debug.LogError("IsNullOrEmpty " + Password);

        }
        else
        {
            if (Password.Length < 5)
            {
                Debug.LogError("Password needs to be longer then 5 " + Password);
            }
            else
            {
                BasisAssetBundleObject BasisAssetBundleObject = AssetDatabase.LoadAssetAtPath<BasisAssetBundleObject>(BasisAssetBundleObject.AssetBundleObject);

                if (Avatar.gameObject.TryGetComponent(out BasisContentBase basisContentBase))
                {
                    BasisBundleInformation basisBundleInformation = new BasisBundleInformation
                    {
                        BasisBundleDescription = basisContentBase.BasisBundleDescription
                    };
                    BasisAssetBundlePipeline.BuildAssetBundle(Avatar.gameObject, BasisAssetBundleObject, basisBundleInformation, Password);
                }
                else
                {
                    Debug.LogError("Missing the Avatar");
                }
            }
        }
    }
}