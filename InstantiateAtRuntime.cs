using UnityEngine;

public class InstantiateAtRuntime : MonoBehaviour
{
    // Reference to the prefab to be instantiated
    public GameObject myPrefab;

    // This script will simply instantiate the Prefab when the game starts.
    void Start()
    {
        // Disable the GameObject this script is attached to
        gameObject.SetActive(false);

        // Instantiate the specified prefab at the position and rotation of this GameObject
        Instantiate(myPrefab, transform.position, transform.rotation);
    }
}
