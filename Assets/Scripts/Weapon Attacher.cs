// WeaponAttacher.cs
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponAttacher : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Animator characterAnimator; // The character's Animator (Humanoid)
    [SerializeField] private bool useRightHand = true;   // Toggle left/right hand
    [Tooltip("Optional explicit socket transform. If set, this overrides hand auto-detect.")]
    [SerializeField] private Transform customSocket;

    [Header("Weapon")]
    [Tooltip("If set, this prefab will be spawned and attached. Leave null to attach an existing 'weaponInScene' object.")]
    [SerializeField] private GameObject weaponPrefab;
    [Tooltip("If you already placed a weapon in the scene (child of this object or anywhere), assign it here.")]
    [SerializeField] private Transform weaponInScene;

    [Header("Offsets (relative to socket)")]
    [SerializeField] private Vector3 localPosition;
    [SerializeField] private Vector3 localEulerAngles;
    [SerializeField] private Vector3 localScale = Vector3.one;

    private Transform _socket;
    private Transform _weapon;

    private void Reset()
    {
        characterAnimator = GetComponentInParent<Animator>();
    }

    private void Awake()
    {
        if (!characterAnimator)
            characterAnimator = GetComponentInParent<Animator>();

        // 1) Find or use the socket
        _socket = ResolveSocket();

        if (_socket == null)
        {
            Debug.LogError("[WeaponAttacher] Could not resolve a hand socket. Ensure Animator is Humanoid or assign a Custom Socket.", this);
            enabled = false;
            return;
        }

        // 2) Spawn or use existing weapon
        if (!_weapon)
        {
            if (weaponPrefab)
            {
                var go = Instantiate(weaponPrefab, _socket);
                _weapon = go.transform;
            }
            else if (weaponInScene)
            {
                _weapon = weaponInScene;
                _weapon.SetParent(_socket, worldPositionStays: false);
            }
            else
            {
                Debug.LogWarning("[WeaponAttacher] No weapon assigned. Set a prefab or a scene transform.", this);
                return;
            }
        }

        // 3) Apply offsets
        ApplyOffsets();
    }

#if UNITY_EDITOR
    // Keep alignment updated if you tweak offsets in the Inspector at edit-time.
    private void OnValidate()
    {
        if (Application.isPlaying && _weapon && _socket)
        {
            ApplyOffsets();
        }
    }
#endif

    private Transform ResolveSocket()
    {
        if (customSocket) return customSocket;

        if (characterAnimator && characterAnimator.isHuman)
        {
            var bone = useRightHand ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand;
            return characterAnimator.GetBoneTransform(bone);
        }

        // Fallback: try to find by name (non-humanoid rigs)
        string guess = useRightHand ? "RightHand" : "LeftHand";
        var t = transform.GetComponentInParent<Transform>();
        return t ? FindChildRecursive(t, guess) : null;
    }

    private static Transform FindChildRecursive(Transform root, string name)
    {
        foreach (Transform child in root)
        {
            if (child.name.Equals(name)) return child;
            var found = FindChildRecursive(child, name);
            if (found) return found;
        }
        return null;
    }

    private void ApplyOffsets()
    {
        _weapon.SetLocalPositionAndRotation(localPosition, Quaternion.Euler(localEulerAngles));
        _weapon.localScale = localScale;
    }

    /// <summary>
    /// Public helper if you want to swap weapons at runtime.
    /// </summary>
    public void Equip(Transform newWeapon, Vector3 pos, Vector3 euler, Vector3 scale)
    {
        if (!_socket) _socket = ResolveSocket();
        if (!_socket) return;

        _weapon = newWeapon;
        _weapon.SetParent(_socket, worldPositionStays: false);
        localPosition = pos;
        localEulerAngles = euler;
        localScale = scale;
        ApplyOffsets();
    }

    /// <summary>
    /// Public helper if you want to swap to a prefab at runtime.
    /// </summary>
    public void Equip(GameObject prefab, Vector3 pos, Vector3 euler, Vector3 scale)
    {
        if (_weapon) Destroy(_weapon.gameObject);
        var go = Instantiate(prefab, _socket);
        Equip(go.transform, pos, euler, scale);
    }
}
