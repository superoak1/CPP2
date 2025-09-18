using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Current Weapon")]
    public GameObject currentWeapon;
    public Transform weaponHolder; // Empty object near the hand

    private void OnTriggerEnter(Collider other)
    {
        Weapon weapon = other.GetComponent<Weapon>();
        if (weapon != null)
        {
            PickUpWeapon(weapon);
        }
    }

    void PickUpWeapon(Weapon weapon)
    {
        // Remove old weapon from hand if exists
        if (currentWeapon != null)
        {
            currentWeapon.SetActive(true); // Optionally drop it
            currentWeapon.transform.parent = null; // Detach
        }

        // Attach new weapon
        currentWeapon = weapon.gameObject;

        // Make it a child of the weaponHolder
        currentWeapon.transform.SetParent(weaponHolder);

        // Reset position/rotation relative to hand
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;

        // Disable weapon's trigger so it won't be picked up again
        Collider col = currentWeapon.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        Debug.Log("Picked up: " + weapon.weaponName);
    }
}