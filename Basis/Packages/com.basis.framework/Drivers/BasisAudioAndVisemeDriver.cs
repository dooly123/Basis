using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using UnityEngine;
using uLipSync;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Basis.Scripts.BasisSdk.Helpers;
namespace Basis.Scripts.Drivers
{
    public class BasisAudioAndVisemeDriver : MonoBehaviour
    {
        public int smoothAmount = 70;
        public bool[] HasViseme;
        public int BlendShapeCount;
        public BasisPlayer Player;
        public BasisAvatar Avatar;
        public uLipSync.uLipSync uLipSync;
        public uLipSyncBlendShape uLipSyncBlendShape;
        public List<PhonemeBlendShapeInfo> phonemeBlendShapeTable = new List<PhonemeBlendShapeInfo>();
        private uLipSync.Profile profile;
        public bool FirstTime = false;
        [System.Serializable]
        public class PhonemeBlendShapeInfo
        {
            public string phoneme;
            public int blendShape;
        }
        public bool WasSuccessful;
        public int HashInstanceID = -1;
        public bool TryInitialize(BasisPlayer BasisPlayer)
        {
            WasSuccessful = false;
            Avatar = BasisPlayer.BasisAvatar;
            Player = BasisPlayer;
            if (Avatar == null)
            {
                BasisDebug.Log("not setting up BasisVisemeDriver Avatar was null");
                return false;
            }
            if (Avatar.FaceVisemeMesh == null)
            {
                BasisDebug.Log("not setting up BasisVisemeDriver FaceVisemeMesh was null");
                return false;
            }
            if (Avatar.FaceVisemeMesh.sharedMesh.blendShapeCount == 0)
            {
                BasisDebug.Log("not setting up BasisVisemeDriver blendShapeCount was empty");
                return false;
            }
            if (uLipSync == null)
            {
                FirstTime = true;
            }
            uLipSync = BasisHelpers.GetOrAddComponent<uLipSync.uLipSync>(this.gameObject);
            phonemeBlendShapeTable.Clear();
            if (uLipSync.profile == null)
            {
                if (Profile == null)
                {
                    // Start loading the ScriptableObject from Addressables using the addressable key
                    AsyncOperationHandle<uLipSync.Profile> handle = Addressables.LoadAssetAsync<uLipSync.Profile>("Packages/com.hecomi.ulipsync/Assets/Profiles/uLipSync-Profile-Sample.asset");

                    // Wait for the operation to complete
                    handle.WaitForCompletion();
                    Profile = handle.Result;
                }
                uLipSync.profile = Profile;
            }

            uLipSyncBlendShape = BasisHelpers.GetOrAddComponent<uLipSyncBlendShape>(this.gameObject);
            uLipSyncBlendShape.usePhonemeBlend = true;
            uLipSyncBlendShape.skinnedMeshRenderer = Avatar.FaceVisemeMesh;
            BlendShapeCount = Avatar.FaceVisemeMovement.Length;
            HasViseme = new bool[BlendShapeCount];
            for (int Index = 0; Index < BlendShapeCount; Index++)
            {
                if (Avatar.FaceVisemeMovement[Index] != -1)
                {
                    int FaceVisemeIndex = Avatar.FaceVisemeMovement[Index];
                    HasViseme[Index] = true;
                    switch (Index)
                    {
                        case 10:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "A",
                                    blendShape = FaceVisemeIndex
                                };
                                phonemeBlendShapeTable.Add(PhonemeBlendShapeInfo);
                                break;
                            }

                        case 12:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "I",
                                    blendShape = FaceVisemeIndex
                                };
                                phonemeBlendShapeTable.Add(PhonemeBlendShapeInfo);
                                break;
                            }

                        case 14:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "U",
                                    blendShape = FaceVisemeIndex
                                };
                                phonemeBlendShapeTable.Add(PhonemeBlendShapeInfo);
                                break;
                            }

                        case 11:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "E",
                                    blendShape = FaceVisemeIndex
                                };
                                phonemeBlendShapeTable.Add(PhonemeBlendShapeInfo);
                                break;
                            }
                        case 13:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "O",
                                    blendShape = FaceVisemeIndex
                                };
                                phonemeBlendShapeTable.Add(PhonemeBlendShapeInfo);
                                break;
                            }
                        case 7:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "S",
                                    blendShape = FaceVisemeIndex
                                };
                                phonemeBlendShapeTable.Add(PhonemeBlendShapeInfo);
                                break;
                            }

                    }
                }
                else
                {
                    HasViseme[Index] = false;
                }
            }
            uLipSyncBlendShape.blendShapes.Clear();
            for (int Index = 0; Index < phonemeBlendShapeTable.Count; Index++)
            {
                PhonemeBlendShapeInfo info = phonemeBlendShapeTable[Index];
                uLipSyncBlendShape.AddBlendShape(info.phoneme, info.blendShape);
            }
            if (FirstTime)
            {
                uLipSync.uLipSyncBlendShape = uLipSyncBlendShape;
            }
            uLipSync.Initalize();
            if (Player != null && Player.FaceRenderer != null && HashInstanceID != Player.FaceRenderer.GetInstanceID())
            {
                BasisDebug.Log("Wired up Renderer Check For Blinking");
                Player.FaceRenderer.Check += UpdateFaceVisibility;
                Player.FaceRenderer.DestroyCalled += TryDeinitalize;
            }
            UpdateFaceVisibility(Player.FaceisVisible);
            WasSuccessful = true;
            return true;
        }
        public void TryDeinitalize()
        {
            WasSuccessful = false;
            OnDeInitalize();
        }
        public bool uLipSyncEnabledState = true;
        public Profile Profile { get => profile; set => profile = value; }
        private void UpdateFaceVisibility(bool State)
        {
            uLipSyncEnabledState = State;
        }
        public void OnDeInitalize()
        {
            if (Player != null)
            {
                if (Player.FaceRenderer != null && HashInstanceID == Player.FaceRenderer.GetInstanceID())
                {
                    Player.FaceRenderer.Check -= UpdateFaceVisibility;
                    Player.FaceRenderer.DestroyCalled -= TryDeinitalize;
                }
            }
        }
        public void ProcessAudioSamples(float[] data,int channels,int Length)
        {
            if (uLipSyncEnabledState == false)
            {
                return;
            }
            if (WasSuccessful == false)
            {
                return;
            }
            uLipSync.OnDataReceived(data, channels, Length);
        }
    }
}
