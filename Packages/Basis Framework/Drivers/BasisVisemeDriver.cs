using Basis.Scripts.BasisSdk;
using Basis.Scripts.BasisSdk.Players;
using System;
using UnityEngine;
using uLipSync;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Basis.Scripts.BasisSdk.Helpers;
namespace Basis.Scripts.Drivers
{
    public class BasisVisemeDriver : MonoBehaviour
    {
        public int smoothAmount = 70;
        public bool[] HasViseme;
        public int BlendShapeCount;
        public BasisPlayer Player;
        public BasisAvatar Avatar;
        public uLipSync.uLipSync uLipSync;
        public uLipSyncBlendShape uLipSyncBlendShape;
        public List<PhonemeBlendShapeInfo> phonemeBlendShapeTable = new List<PhonemeBlendShapeInfo>();
        public uLipSync.Profile profile;
        [System.Serializable]
        public class PhonemeBlendShapeInfo
        {
            public string phoneme;
            public int blendShape;
        }
        public bool WasSuccessful;
        public bool TryInitialize(BasisPlayer BasisPlayer)
        {
            WasSuccessful = false;
            Avatar = BasisPlayer.Avatar;
            Player = BasisPlayer;
            if (Avatar == null)
            {
                Debug.Log("not setting up BasisVisemeDriver Avatar was null");
                return false;
            }
            if (Avatar.FaceVisemeMesh == null)
            {
                Debug.Log("not setting up BasisVisemeDriver FaceVisemeMesh was null");
                return false;
            }
            if (Avatar.FaceVisemeMesh.sharedMesh.blendShapeCount == 0)
            {
                Debug.Log("not setting up BasisVisemeDriver blendShapeCount was empty");
                return false;
            }

            GameObject.Destroy(uLipSyncBlendShape);

            uLipSync = BasisHelpers.GetOrAddComponent<uLipSync.uLipSync>(this.gameObject);

            if (uLipSync.profile == null)
            {
                if (profile == null)
                {
                    // Start loading the ScriptableObject from Addressables using the addressable key
                    AsyncOperationHandle<uLipSync.Profile> handle = Addressables.LoadAssetAsync<uLipSync.Profile>("Packages/com.hecomi.ulipsync/Assets/Profiles/uLipSync-Profile-Sample.asset");

                    // Wait for the operation to complete
                    handle.WaitForCompletion();
                    profile = handle.Result;
                }
                uLipSync.profile = profile;
            }

            uLipSyncBlendShape = BasisHelpers.GetOrAddComponent<uLipSyncBlendShape>(this.gameObject);

            uLipSyncBlendShape.skinnedMeshRenderer = Avatar.FaceVisemeMesh;
            uLipSyncBlendShape.updateMethod = UpdateMethod.External;
            BlendShapeCount = Avatar.FaceVisemeMovement.Length;
            HasViseme = new bool[BlendShapeCount];
            for (int Index = 0; Index < BlendShapeCount; Index++)
            {
                if (Avatar.FaceVisemeMovement[Index] != -1)
                {
                    HasViseme[Index] = true;
                    switch (Index)
                    {
                        case 10:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "A",
                                    blendShape = Index
                                };
                                phonemeBlendShapeTable.Add(PhonemeBlendShapeInfo);
                                break;
                            }

                        case 12:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "I",
                                    blendShape = Index
                                };
                                phonemeBlendShapeTable.Add(PhonemeBlendShapeInfo);
                                break;
                            }

                        case 14:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "U",
                                    blendShape = Index
                                };
                                phonemeBlendShapeTable.Add(PhonemeBlendShapeInfo);
                                break;
                            }

                        case 11:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "E",
                                    blendShape = Index
                                };
                                phonemeBlendShapeTable.Add(PhonemeBlendShapeInfo);
                                break;
                            }
                        case 13:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "O",
                                    blendShape = Index
                                };
                                phonemeBlendShapeTable.Add(PhonemeBlendShapeInfo);
                                break;
                            }
                        case 7:
                            {
                                PhonemeBlendShapeInfo PhonemeBlendShapeInfo = new PhonemeBlendShapeInfo
                                {
                                    phoneme = "S",
                                    blendShape = Index
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
            for (int Index = 0; Index < phonemeBlendShapeTable.Count; Index++)
            {
                PhonemeBlendShapeInfo info = phonemeBlendShapeTable[Index];
                uLipSyncBlendShape.AddBlendShape(info.phoneme, info.blendShape);
            }
            uLipSyncBlendShape.updateMethod = UpdateMethod.LipSyncUpdateEvent;
            uLipSync.onLipSyncUpdate.RemoveAllListeners();
            uLipSync.onLipSyncUpdate.AddListener(uLipSyncBlendShape.OnLipSyncUpdate);
            WasSuccessful = true;
            return true;
        }
        public void ProcessAudioSamples(float[] data)
        {
            if (WasSuccessful == false)
            {
                return;
            }
            if (Player.FaceisVisible == false)
            {
                return;
            }
            uLipSync.OnDataReceived(data, 1);
        }
    }
}