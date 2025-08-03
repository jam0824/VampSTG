using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RagdollEmbedder : MonoBehaviour
{
    [Header("Ragdoll Settings")]
    [Tooltip("Root transform of the ragdoll hierarchy")]    
    public Transform ragdollRoot;
    [Tooltip("Name of the bone to embed (e.g., Hips)")]
    public string boneName = "Hips";

    [Header("Attachment Settings")]
    [Tooltip("Rigidbody of the object to embed into")]    
    public Rigidbody targetRigidbody;
    [Tooltip("Collider of the object to embed into")]    
    public Collider targetCollider;

    [Header("Joint Options")]
    [Tooltip("Use ConfigurableJoint instead of FixedJoint")]    
    public bool useConfigurableJoint = false;
    [Tooltip("Allow angular freedom when using ConfigurableJoint")]    
    public bool freeAngularMotion = false;

    void Start()
    {
        if (ragdollRoot == null || targetRigidbody == null || targetCollider == null)
        {
            Debug.LogError("RagdollEmbedder: Assign ragdollRoot, targetRigidbody, and targetCollider in the Inspector.");
            enabled = false;
            return;
        }

        // Find the specified bone
        Transform bone = FindBone(ragdollRoot, boneName);
        if (bone == null)
        {
            Debug.LogError($"RagdollEmbedder: Bone '{boneName}' not found under ragdollRoot.");
            enabled = false;
            return;
        }

        AttachBone(bone);
    }

    // Recursive search for the bone by name
    Transform FindBone(Transform current, string name)
    {
        if (current.name == name)
            return current;
        foreach (Transform child in current)
        {
            Transform found = FindBone(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

    void AttachBone(Transform bone)
    {
        // Disable collisions between the bone's colliders and the target collider
        Collider[] boneColliders = bone.GetComponentsInChildren<Collider>();
        foreach (Collider col in boneColliders)
        {
            Physics.IgnoreCollision(col, targetCollider, true);
        }

        // Add and configure the joint
        Joint joint;
        if (useConfigurableJoint)
        {
            var cfg = bone.gameObject.AddComponent<ConfigurableJoint>();
            cfg.connectedBody = targetRigidbody;

            // Lock linear motion
            cfg.xMotion = ConfigurableJointMotion.Locked;
            cfg.yMotion = ConfigurableJointMotion.Locked;
            cfg.zMotion = ConfigurableJointMotion.Locked;

            if (freeAngularMotion)
            {
                cfg.angularXMotion = ConfigurableJointMotion.Free;
                cfg.angularYMotion = ConfigurableJointMotion.Free;
                cfg.angularZMotion = ConfigurableJointMotion.Free;
            }
            else
            {
                cfg.angularXMotion = ConfigurableJointMotion.Locked;
                cfg.angularYMotion = ConfigurableJointMotion.Locked;
                cfg.angularZMotion = ConfigurableJointMotion.Locked;
            }

            joint = cfg;
        }
        else
        {
            var fixedJ = bone.gameObject.AddComponent<FixedJoint>();
            fixedJ.connectedBody = targetRigidbody;
            joint = fixedJ;
        }

        // Prevent the joint from breaking under load
        joint.breakForce = Mathf.Infinity;
        joint.breakTorque = Mathf.Infinity;

        // Optionally make the bone's Rigidbody kinematic to prevent unwanted movement
        Rigidbody boneRb = bone.GetComponent<Rigidbody>();
        if (boneRb != null)
        {
            boneRb.isKinematic = true;
            // Or use: boneRb.constraints = RigidbodyConstraints.FreezePosition;
        }
    }
}
