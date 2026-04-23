using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ModularBody : MonoBehaviour
{
    
    [System.Serializable]
    public class PartConfig
    {
        public string partName;
        public string[] addressableKeys;
        public GameObject mountPoint; 
        
        public BodyConfig[] bodies;

        public HandConfig[] hands;

        public HeadConfig[] heads;
    }

    [System.Serializable]
    public class HeadConfig
    {
        public string addressableKey;
        public Vector3 offset;
        public Vector3 rotation;
    }

    [System.Serializable]
    public class BodyConfig
    {
        public string addressableKey;
        public Vector3 headOffset;
        public Vector3 bodyOffset;
        public Vector3 bodyRotation;
        public Vector3 leftHandOffset;
        public Vector3 rightHandOffset;
        public bool overrideScale;
        public Vector3 headScale;
        public Vector3 scale;
        public float handScale;
        public float colliderHeight;
        public float colliderRadius;
        public Vector3 colliderCenter;

        public string leftArmBone;
        public string rightArmBone;
        public string neckBone;
        public bool noHands = false;
        public string[] excludedHandles;
    }
    [System.Serializable]
    public class HandConfig
    {
        public string addressableKey;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
        public Vector3 offsetPosition;
        public bool overridePosition;
        public Vector3 position;
    }

    [SerializeField] PartConfig[] parts;
    private List<AsyncOperationHandle<GameObject>> loadedParts = new();

    public System.Action OnPartsLoaded;

    public async Task LoadRandomParts()
    {
        string spawnedHead = null;
        string spawnedBodyKey = null;
        GameObject spawnedObject = null;

        foreach(var config in parts)
        {
            if(config.addressableKeys == null || config.addressableKeys.Length < 1) continue;
            if(config.partName == "LeftHand" || config.partName == "RightHand") continue;

            string randomKey = config.addressableKeys[Random.Range(0, config.addressableKeys.Length)];

            if(config.partName == "Head") spawnedHead = randomKey;
            if(config.partName == "Body") spawnedBodyKey = randomKey;

            var assetHandle = Addressables.LoadAssetAsync<GameObject>(randomKey);

            await assetHandle.Task;

            loadedParts.Add(assetHandle);

            GameObject prefab = assetHandle.Result;
            GameObject part = Instantiate(prefab, config.mountPoint.transform);
            Debug.Log("loaded");

            if(config.partName == "Body") spawnedObject = part;
        }

        PartConfig bodyConfig = System.Array.Find(parts, p => p.partName == "Body");
        BodyConfig bodyOffsets = bodyConfig?.bodies != null ? System.Array.Find(bodyConfig.bodies, e => e.addressableKey == spawnedBodyKey) : null;

        string spawnedLeftHand = null;
        string spawnedRightHand = null;

        foreach(var config in parts)
        {
            if (config.addressableKeys == null || config.addressableKeys.Length < 1) continue;
            if (config.partName != "LeftHand" && config.partName != "RightHand") continue;
            string[] excluded = bodyOffsets.excludedHandles;
            string chosenKey = PickAllowedKey(config.addressableKeys, excluded);

            if(chosenKey == null) continue;

            if(config.partName == "LeftHand") spawnedLeftHand = chosenKey;
            if(config.partName == "RightHand") spawnedRightHand = chosenKey;

            var assetHandle = Addressables.LoadAssetAsync<GameObject>(chosenKey);
            await assetHandle.Task;
            loadedParts.Add(assetHandle);

            Instantiate(assetHandle.Result, config.mountPoint.transform);
        }

        ApplyOffsets(spawnedBodyKey, spawnedHead, spawnedLeftHand, spawnedRightHand, spawnedObject);
        OnPartsLoaded?.Invoke();
    }

    private string PickAllowedKey(string[] keys, string[] excluded)
    {
        if(keys == null || keys.Length == 0) return null;
        if(excluded == null || excluded.Length == 0)
        {
            return keys[Random.Range(0, keys.Length)];
        }

        List<string> allowed = new();
        foreach(var k in keys)
        {
            if(System.Array.IndexOf(excluded, k) < 0) allowed.Add(k);
        }

        if(allowed.Count == 0)
        {
            return keys[Random.Range(0, keys.Length)];
        }

        return allowed[Random.Range(0, allowed.Count)];
    }

    public GameObject GetMountPoint(string partName)
    {
        PartConfig config = System.Array.Find(parts, p => p.partName == partName);
        return config?.mountPoint;
    }

    public void ReleaseHandles()
    {
        foreach(var handle in loadedParts)
        {
            if(handle.IsValid()) Addressables.Release(handle);
        }
        loadedParts.Clear();
    }

    void OnDestroy()
    {
        ReleaseHandles();
    }

    private void ApplyOffsets(string spawnedBodyKey, string spawnedHead, string spawnedLeftHand, string spawnedRightHand, GameObject spawnedObject)
    {
 
        PartConfig bodyConfig = System.Array.Find(parts, p => p.partName == "Body");
 
        BodyConfig bodyOffsets = null;

        if(bodyConfig != null){
            bodyOffsets = System.Array.Find(bodyConfig.bodies, e => e.addressableKey == spawnedBodyKey);
        if (bodyConfig.mountPoint != null)
        {
            bodyConfig.mountPoint.transform.localPosition += bodyOffsets.bodyOffset;
            bodyConfig.mountPoint.transform.localRotation  = Quaternion.Euler(bodyOffsets.bodyRotation);
            if (bodyOffsets.overrideScale)
            {
                bodyConfig.mountPoint.transform.localScale = bodyOffsets.scale;
                CapsuleCollider col = GetComponent<CapsuleCollider>();
                if (col != null)
                {
                    col.height = bodyOffsets.colliderHeight;
                    col.radius = bodyOffsets.colliderRadius;
                    col.center = bodyOffsets.colliderCenter;
                }
            }
        }
        }
 
        PartConfig headPart   = System.Array.Find(parts, p => p.partName == "Head");
        HeadConfig headConfig = null;
        if (headPart != null)
        {
            headConfig = System.Array.Find(headPart.heads, e => e.addressableKey == spawnedHead);
            if (headConfig != null)
            {
                if(bodyOffsets != null)
                {
                    headPart.mountPoint.transform.localPosition += bodyOffsets.headOffset + headConfig.offset;

                    if (bodyOffsets.overrideScale)
                        headPart.mountPoint.transform.localScale = bodyOffsets.headScale;
                }
                headPart.mountPoint.transform.localRotation  = Quaternion.Euler(headConfig.rotation);
           
            }
        }
 
        PartConfig leftHandConfig   = System.Array.Find(parts, p => p.partName == "LeftHand");
        HandConfig leftHandRotation = null;
        if (leftHandConfig != null)
        {
            if (bodyOffsets.noHands)
            {
                leftHandConfig.mountPoint.gameObject.SetActive(false);
            }
            else{
            leftHandConfig.mountPoint.transform.localPosition += bodyOffsets.leftHandOffset;
            leftHandRotation = System.Array.Find(leftHandConfig.hands, e => e.addressableKey == spawnedLeftHand);
            if (leftHandRotation != null)
            {
                leftHandConfig.mountPoint.transform.localRotation  = Quaternion.Euler(leftHandRotation.rotation);
                leftHandConfig.mountPoint.transform.localScale     = leftHandRotation.scale;
                leftHandConfig.mountPoint.transform.localPosition += leftHandRotation.offsetPosition;
                if (leftHandRotation.overridePosition)
                    leftHandConfig.mountPoint.transform.localPosition = leftHandRotation.position;
                if (bodyOffsets.overrideScale)
                    leftHandConfig.mountPoint.transform.localScale *= bodyOffsets.handScale;
                
                MeshJitter lhj = leftHandConfig.mountPoint.GetComponent<MeshJitter>();
                if (lhj != null) { lhj.parented = true; lhj.UpdateBaseValues(); }
            }
            }
        }
 
        PartConfig rightHandConfig   = System.Array.Find(parts, p => p.partName == "RightHand");
        HandConfig rightHandRotation = null;
        if (rightHandConfig != null)
        {
            if (bodyOffsets.noHands)
            {
                rightHandConfig.mountPoint.gameObject.SetActive(false);
            }
            else{
            rightHandConfig.mountPoint.transform.localPosition += bodyOffsets.rightHandOffset;
            rightHandRotation = System.Array.Find(rightHandConfig.hands, e => e.addressableKey == spawnedRightHand);
            if (rightHandRotation != null)
            {
                rightHandConfig.mountPoint.transform.localRotation  = Quaternion.Euler(rightHandRotation.rotation);
                rightHandConfig.mountPoint.transform.localScale     = rightHandRotation.scale;
                rightHandConfig.mountPoint.transform.localPosition += rightHandRotation.offsetPosition;
                if (rightHandRotation.overridePosition)
                    rightHandConfig.mountPoint.transform.localPosition = rightHandRotation.position;
                if (bodyOffsets.overrideScale)
                    rightHandConfig.mountPoint.transform.localScale *= bodyOffsets.handScale;
                
                MeshJitter rhj = rightHandConfig.mountPoint.GetComponent<MeshJitter>();
                if (rhj != null) { rhj.parented = true; rhj.UpdateBaseValues(); }
            }
            }
        }
 
        if (spawnedObject == null) return;
 
        if (!string.IsNullOrEmpty(bodyOffsets.neckBone) && headPart != null)
        {
            Transform neck = FindBone(spawnedObject.transform, bodyOffsets.neckBone);
            if (neck != null)
            {
                headPart.mountPoint.transform.SetParent(neck, true);
                headPart.mountPoint.transform.localPosition = bodyOffsets.headOffset;
            }
        }
 
        if (!string.IsNullOrEmpty(bodyOffsets.leftArmBone) && leftHandConfig != null)
        {
            Transform leftBone = FindBone(spawnedObject.transform, bodyOffsets.leftArmBone);
            if (leftBone != null)
            {
                leftHandConfig.mountPoint.transform.SetParent(leftBone, true);
                leftHandConfig.mountPoint.transform.localPosition = bodyOffsets.leftHandOffset;
                if (leftHandRotation != null)
                    leftHandConfig.mountPoint.transform.position += leftHandRotation.offsetPosition;
            }
        }
 
        if (!string.IsNullOrEmpty(bodyOffsets.rightArmBone) && rightHandConfig != null)
        {
            Transform rightBone = FindBone(spawnedObject.transform, bodyOffsets.rightArmBone);
            if (rightBone != null)
            {
                rightHandConfig.mountPoint.transform.SetParent(rightBone, true);
                rightHandConfig.mountPoint.transform.localPosition = bodyOffsets.rightHandOffset;
                if (rightHandRotation != null)
                    rightHandConfig.mountPoint.transform.position += rightHandRotation.offsetPosition;
            }
        }

        if (rightHandConfig != null)
            Debug.Log(gameObject);
            Debug.Log($"{gameObject} :  ApplyOffsets end — RightHand localScale: {rightHandConfig.mountPoint.transform.localScale}, override scale: {bodyOffsets.handScale}");
    }

    Transform FindBone(Transform root, string boneName)
    {
        if(root.name == boneName) return root;
        foreach(Transform child in root)
        {
            Transform result = FindBone(child, boneName);
            if(result != null) return result;
        }
        return null;
    }
}