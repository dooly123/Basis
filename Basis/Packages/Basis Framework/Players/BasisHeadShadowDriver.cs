using System;
using System.Collections.Generic;
using System.Linq;
using Basis.Scripts.Avatar;
using UnityEngine;
using UnityEngine.Rendering;

namespace Basis.Scripts.BasisSdk.Players
{
    public class BasisHeadShadowDriver : MonoBehaviour
    {
        private Transform _headNullable;

        private MeshRenderer[] _meshRenderersUnderHeadBoneOrZero;
        private SkinnedMeshRenderer[] _skinnedMeshesOnAvatarThatDependOnHead;
        private List<Transform> _transformsUnderHeadBoneOrZero;

        private int[] _skinnedMeshBlendShapeCount;

        private Transform _nonHeadAndNonNeckDisappearer;
        private SkinnedMeshRenderer[] _copiesOfSkinnedMeshes;
        private Transform[] _copiesOfTransformsUnderHeadBone;

        private bool _isShadowNecessary;

        private static readonly Bounds DoNotRenderBounds = new(Vector3.one * 999_999_999, Vector3.zero);

        public void Initialize(BasisAvatar avatar)
        {
            _headNullable = avatar.Animator is { } animator && animator
                            && animator.GetBoneTransform(HumanBodyBones.Head) is { } head && head
                ? head : null;
            if (_headNullable == null)
            {
                Debug.Log("There was Head bone to generate the shadow clone (This is not a problem)");
            }

            _meshRenderersUnderHeadBoneOrZero = _headNullable ? _headNullable.GetComponentsInChildren<MeshRenderer>(true) : Array.Empty<MeshRenderer>();
            _transformsUnderHeadBoneOrZero = _headNullable ? _headNullable.GetComponentsInChildren<Transform>(true).ToList() : new List<Transform>();

            // TODO: Need to handle special case when there is a SMR under the Head hierarchy, which may have no bones in it.
            _skinnedMeshesOnAvatarThatDependOnHead = avatar.GetComponentsInChildren<SkinnedMeshRenderer>(true)
                // Ignore SMRs that don't have a mesh.
                .Where(meshRenderer => meshRenderer.sharedMesh)
                // Intersect is lazily evaluated, so Any will stop when the first element in common is found.
                .Where(HasAnyBoneThatRequiresBonesUnderHeadHierarchy)
                .ToArray();

            _isShadowNecessary = _headNullable && (_skinnedMeshesOnAvatarThatDependOnHead.Length > 0 || _meshRenderersUnderHeadBoneOrZero.Length > 0);
            if (_isShadowNecessary)
            {
                // Head can't be null past this point.
                var neck = _headNullable.parent;

                _nonHeadAndNonNeckDisappearer = new GameObject("NonHeadAndNonNeckDisappearer")
                {
                    transform = { position = neck.position, rotation = neck.rotation, localScale = Vector3.zero }
                }.transform;
                _nonHeadAndNonNeckDisappearer.SetParent(neck, true);

                // Create copies
                CloneHead();

                _copiesOfSkinnedMeshes = new SkinnedMeshRenderer[_skinnedMeshesOnAvatarThatDependOnHead.Length];
                _skinnedMeshBlendShapeCount = new int[_skinnedMeshesOnAvatarThatDependOnHead.Length];
                for (var index = 0; index < _skinnedMeshesOnAvatarThatDependOnHead.Length; index++)
                {
                    var originalSmr = _skinnedMeshesOnAvatarThatDependOnHead[index];

                    var copy = new GameObject(NameOfShadowCopy(originalSmr.name))
                    {
                        transform = { parent = originalSmr.transform, localPosition = Vector3.zero, localRotation = Quaternion.identity, localScale = Vector3.one }
                    };
                    copy.SetActive(false);
                    copy.layer = BasisLayer.LocalPlayerAvatar;

                    var smrCopy = copy.AddComponent<SkinnedMeshRenderer>();
                    _copiesOfSkinnedMeshes[index] = smrCopy;

                    smrCopy.sharedMesh = originalSmr.sharedMesh;
                    smrCopy.bones = ProduceNewBoneArrayReferencingShadowCopyHeadBones(originalSmr.bones);

                    smrCopy.localBounds = originalSmr.localBounds;
                    smrCopy.quality = originalSmr.quality;
                    smrCopy.updateWhenOffscreen = originalSmr.updateWhenOffscreen;
                    smrCopy.rootBone = originalSmr.rootBone;

                    smrCopy.sharedMaterials = originalSmr.sharedMaterials;

                    smrCopy.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
                    smrCopy.receiveShadows = false;

                    smrCopy.lightProbeUsage = LightProbeUsage.Off;
                    smrCopy.probeAnchor = originalSmr.probeAnchor;

                    smrCopy.skinnedMotionVectors = originalSmr.skinnedMotionVectors;
                    smrCopy.allowOcclusionWhenDynamic = originalSmr.allowOcclusionWhenDynamic;
                    smrCopy.renderingLayerMask = originalSmr.renderingLayerMask;

                    _skinnedMeshBlendShapeCount[index] = originalSmr.sharedMesh.blendShapeCount;

                    copy.SetActive(true);
                }

                // TODO: It would be smarter to let MeshRenderers always display, but:
                // - Set it to render Shadow Only when rendering in first-person.
                // - Set it to render Mesh and Shadow when rendering in third-person.
            }
            else
            {
                _copiesOfTransformsUnderHeadBone = Array.Empty<Transform>();
                _copiesOfSkinnedMeshes = Array.Empty<SkinnedMeshRenderer>();
            }
        }

        public void PrepareThisFrame()
        {
            if (!_isShadowNecessary) return;

            for (var index = 0; index < _skinnedMeshesOnAvatarThatDependOnHead.Length; index++)
            {
                var smr = _skinnedMeshesOnAvatarThatDependOnHead[index];
                if (smr) // Handle the remote possibility that the original SkinnedMeshRenderer may have been deleted by an outside system.
                {
                    var copy = _copiesOfSkinnedMeshes[index];
                    if (copy) // Handle the possibility that our copy of the SkinnedMeshRenderer may have been deleted by an outside system.
                    {
                        if (smr.enabled != copy.enabled) copy.enabled = smr.enabled;

                        // We want the copy to be Inactive when the original or any of its parents is Inactive,
                        // hence the discrepancy between activeInHierarchy and self.
                        var isActiveInHierarchy = smr.gameObject.activeInHierarchy;
                        if (isActiveInHierarchy != copy.gameObject.activeSelf)
                        {
                            copy.gameObject.SetActive(isActiveInHierarchy);
                        }

                        var blendShapeCount = _skinnedMeshBlendShapeCount[index];
                        for (var blendShapeIndex = 0; blendShapeIndex < blendShapeCount; blendShapeIndex++)
                        {
                            copy.SetBlendShapeWeight(blendShapeIndex, smr.GetBlendShapeWeight(blendShapeIndex));
                        }
                    }
                }
                else
                {
                    if (_copiesOfSkinnedMeshes[index])
                    {
                        Destroy(_copiesOfSkinnedMeshes[index].gameObject);
                    }
                    _copiesOfSkinnedMeshes[index] = null;
                }
            }

            for (var index = 0; index < _transformsUnderHeadBoneOrZero.Count; index++)
            {
                var from = _transformsUnderHeadBoneOrZero[index];
                var to = _copiesOfTransformsUnderHeadBone[index];

                if (from && to)
                {
                    to.localPosition = from.localPosition;
                    to.localRotation = from.localRotation;
                    to.localScale = from.localScale;
                }
            }
        }

        public void BeforeRenderFirstPerson()
        {
            if (!_isShadowNecessary) return;

            MakeShadowVisible();
        }

        public void AfterRenderFirstPerson()
        {
            if (!_isShadowNecessary) return;

            // Make shadow invisible to avoid making the end state of the avatar
            // dependent on the last rendered camera.
            MakeShadowInvisible();
        }

        public void BeforeRenderThirdPerson()
        {
            if (!_isShadowNecessary) return;

            MakeShadowInvisible();
        }

        public void AfterRenderThirdPerson()
        {
            if (!_isShadowNecessary) return;

        }

        private void MakeShadowVisible()
        {
            // There may be better ways to do this.
            for (var index = 0; index < _copiesOfSkinnedMeshes.Length; index++)
            {
                var original = _skinnedMeshesOnAvatarThatDependOnHead[index];
                var copy = _copiesOfSkinnedMeshes[index];

                // Handle the possibility of runtime removal by an external system.
                if (original && copy)
                {
                    copy.updateWhenOffscreen = original.updateWhenOffscreen;
                    copy.bounds = original.bounds;
                }
            }
        }

        private void MakeShadowInvisible()
        {
            // There may be better ways to do this.
            foreach (var copy in _copiesOfSkinnedMeshes)
            {
                // Handle the possibility of runtime removal by an external system.
                if (copy)
                {
                    copy.updateWhenOffscreen = false;
                    copy.bounds = DoNotRenderBounds;
                }
            }
        }

        private Transform[] ProduceNewBoneArrayReferencingShadowCopyHeadBones(Transform[] originalSmrBones)
        {
            var newBoneArray = new Transform[originalSmrBones.Length];
            for (var index = 0; index < originalSmrBones.Length; index++)
            {
                var originalBone = originalSmrBones[index];
                var indexOfBoneOrMinus = _transformsUnderHeadBoneOrZero.IndexOf(originalBone);
                if (indexOfBoneOrMinus != -1)
                {
                    newBoneArray[index] = _copiesOfTransformsUnderHeadBone[indexOfBoneOrMinus];
                }
                else
                {
                    newBoneArray[index] = _nonHeadAndNonNeckDisappearer;
                }
            }

            return newBoneArray;
        }

        private void CloneHead()
        {
            if (_headNullable == null)
            {
                _copiesOfTransformsUnderHeadBone = Array.Empty<Transform>();
                return;
            }

            var copies = new Transform[_transformsUnderHeadBoneOrZero.Count];
            for (var index = 0; index < _transformsUnderHeadBoneOrZero.Count; index++)
            {
                var current = _transformsUnderHeadBoneOrZero[index];
                var copy = new GameObject(NameOfShadowCopy(current.name))
                {
                    transform = { position = current.position, rotation = current.rotation, localScale = current.localScale }
                };
                copies[index] = copy.transform;
            }

            for (var index = 0; index < _transformsUnderHeadBoneOrZero.Count; index++)
            {
                var current = _transformsUnderHeadBoneOrZero[index];
                var copy = copies[index];

                var indexOfParentOrMinus = _transformsUnderHeadBoneOrZero.IndexOf(current.parent);
                if (indexOfParentOrMinus < 0)
                {
                    // The parent of the Head bone can't be found, so it must be the neck bone.
                    copy.SetParent(_headNullable.parent, true);
                }
                else
                {
                    copy.SetParent(copies[indexOfParentOrMinus], true);
                }
            }

            _copiesOfTransformsUnderHeadBone = copies;
        }

        private static string NameOfShadowCopy(string name)
        {
            return $"_{name}_ShadowCopy";
        }

        private bool HasAnyBoneThatRequiresBonesUnderHeadHierarchy(SkinnedMeshRenderer meshRenderer)
        {
            // This does not exclude skinned mesh that have something under the Head bone without actually requiring it
            // (it would be better if it did, but that's probably a task better done at avatar build time).
            return AsSafeSet(meshRenderer.bones).Intersect(_transformsUnderHeadBoneOrZero).Any();
        }

        private static HashSet<Transform> AsSafeSet(Transform[] transformsWithNullsAndDuplicates)
        {
            return new HashSet<Transform>(transformsWithNullsAndDuplicates.Where(t => t != null));
        }
    }
}
