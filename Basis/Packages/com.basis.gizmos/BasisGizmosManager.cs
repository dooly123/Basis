using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class BasisGizmoManager
{
    public static Dictionary<int, BasisGizmos> Gizmos = new Dictionary<int, BasisGizmos>();
    public static Dictionary<int, BasisLineGizmos> GizmosLine = new Dictionary<int, BasisLineGizmos>();
    private static int _nextID = 0; // Counter for unique IDs.

    public static string MaterialGizmo = "GizmoMaterial";
    public static string GameobjectGizmo = "Packages/com.basis.gizmos/Gizmo.prefab";
    public static string GameobjectGizmoLine = "Packages/com.basis.gizmos/GizmoLine.prefab";
    public static Action<bool> OnUseGizmosChanged; // Callback delegate.

    public static bool UseGizmos = false;
    public static GameObject LoadedLineGizmo;
    public static GameObject LoadedGizmo;
    public static GameObject Parent;
    public static void TryCreateParent()
    {
        if (Parent == null)
        {
            Parent = new GameObject("Parent Of Debug Data");
        }
    }
    public static void DestroyParent()
    {
        if(Parent != null)
        {
            GameObject.Destroy(Parent);
        }
    }
    /// <summary>
    /// Creates a new sphere gizmo.
    /// </summary>
    public static bool CreateSphereGizmo(out int linkedID, Vector3 position, float size, Color color)
    {
        TryCreateParent();
        linkedID = CreateNewID();

        if (Gizmos.ContainsKey(linkedID))
        {
            Debug.LogError($"[BasisGizmoManager] Gizmo with ID {linkedID} already exists. Use UpdateSphereGizmo to modify it.");
            return false;
        }

        if (LoadedGizmo == null)
        {
            AsyncOperationHandle<GameObject> Loadable = Addressables.LoadAssetAsync<GameObject>(GameobjectGizmo);
            LoadedGizmo = Loadable.WaitForCompletion();
        }
        if (LoadedGizmo == null)
        {
            Debug.LogError($"[BasisGizmoManager] Failed to load Gizmo prefab from {GameobjectGizmo}");
            return false;
        }

        GameObject tempSphere = UnityEngine.Object.Instantiate(LoadedGizmo, Parent.transform);
        tempSphere.name = $"SphereGizmo_{linkedID}";

        AsyncOperationHandle<Material> materialLoad = Addressables.LoadAssetAsync<Material>(MaterialGizmo);
        Material material = materialLoad.WaitForCompletion();
        if (material == null)
        {
            Debug.LogError($"[BasisGizmoManager] Failed to load Gizmo material from {MaterialGizmo}");
            UnityEngine.Object.Destroy(tempSphere);
            return false;
        }

        if (tempSphere.TryGetComponent(out BasisGizmos basisGizmos))
        {
            basisGizmos.ConfigureMeshGizmo(material, Resources.GetBuiltinResource<Mesh>("New-Sphere.fbx"), color);
            Gizmos[linkedID] = basisGizmos;

            tempSphere.transform.position = position;
            tempSphere.transform.localScale = Vector3.one * size;

            Debug.Log($"[BasisGizmoManager] Created SphereGizmo with ID {linkedID}");
            return true;
        }
        else
        {
            Debug.LogError($"[BasisGizmoManager] Prefab missing BasisGizmos component.");
            UnityEngine.Object.Destroy(tempSphere);
            return false;
        }
    }

    /// <summary>
    /// Updates an existing sphere gizmo.
    /// </summary>
    public static bool UpdateSphereGizmo(int linkedID, Vector3 position)
    {
        if (!Gizmos.TryGetValue(linkedID, out BasisGizmos gizmo))
        {
            Debug.LogError($"[BasisGizmoManager] No SphereGizmo found with ID {linkedID}. Use CreateSphereGizmo first.");
            return false;
        }

        gizmo.UpdatePosition(position);
        return true;
    }
    public static bool CreateLineGizmo(int linkedID, Vector3 start, Vector3 end, float width, Color color,GameObject Reference)
    {
        TryCreateParent();
        GameObject gizmoObject = UnityEngine.Object.Instantiate(Reference,Parent.transform);
        if (gizmoObject.TryGetComponent(out BasisLineGizmos basisGizmos))
        {
            LineRenderer lineRenderer = basisGizmos.LineRenderer;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPositions(new[] { start, end });
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            GizmosLine[linkedID] = basisGizmos;

            Debug.Log($"[BasisGizmoManager] Created LineGizmo with ID {linkedID}");
            return true;
        }
        else
        {
            Debug.LogError($"[BasisGizmoManager] Prefab missing BasisLineGizmos component.");
            UnityEngine.Object.Destroy(gizmoObject);
            return false;
        }
    }
    public static bool CreateLineGizmo(out int linkedID, Vector3 start, Vector3 end, float width, Color color)
    {
        linkedID = CreateNewID();
        if (GizmosLine.ContainsKey(linkedID))
        {
            Debug.LogError($"[BasisGizmoManager] LineGizmo with ID {linkedID} already exists. Use UpdateLineGizmo to modify it.");
            return false;
        }
        if (LoadedLineGizmo == null)
        {
            AsyncOperationHandle<GameObject> LoadAble = Addressables.LoadAssetAsync<GameObject>(GameobjectGizmoLine);
            LoadedLineGizmo = LoadAble.WaitForCompletion();
        }
        if (LoadedLineGizmo == null)
        {
            Debug.LogError($"[BasisGizmoManager] Failed to load LineGizmo prefab from {GameobjectGizmoLine}");
            return false;
        }
        return CreateLineGizmo(linkedID, start, end, width, color, LoadedLineGizmo);
    }

    /// <summary>
    /// Updates an existing line gizmo.
    /// </summary>
    public static bool UpdateLineGizmo(int linkedID, Vector3 start, Vector3 end)
    {
        if (!GizmosLine.TryGetValue(linkedID, out BasisLineGizmos gizmo))
        {
            Debug.LogError($"[BasisGizmoManager] No LineGizmo found with ID {linkedID}. Use CreateLineGizmo first.");
            return false;
        }

        gizmo.LineRenderer.SetPosition(0, start);
        gizmo.LineRenderer.SetPosition(1, end);
        return true;
    }

    /// <summary>
    /// Destroys a gizmo with the specified ID.
    /// </summary>
    public static void DestroyGizmo(int linkedID)
    {
        if (Gizmos.Remove(linkedID, out BasisGizmos gizmo))
        {
            UnityEngine.Object.Destroy(gizmo.gameObject);
            Debug.Log($"[BasisGizmoManager] Destroyed SphereGizmo with ID {linkedID}");
        }
        else if (GizmosLine.Remove(linkedID, out BasisLineGizmos lineGizmo))
        {
            UnityEngine.Object.Destroy(lineGizmo.gameObject);
            Debug.Log($"[BasisGizmoManager] Destroyed LineGizmo with ID {linkedID}");
        }
        else
        {
            Debug.LogWarning($"[BasisGizmoManager] No Gizmo found with ID {linkedID} to destroy.");
        }
    }

    /// <summary>
    /// Creates a new unique ID for a gizmo.
    /// </summary>
    private static int CreateNewID()
    {
        return ++_nextID;
    }
}