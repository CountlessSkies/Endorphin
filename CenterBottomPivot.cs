using UnityEngine;
using UnityEditor;

public class MyMenu : MonoBehaviour
{
    [MenuItem("Endorphin/Center Bottom Pivot")]
    static void CenterBottomPivot()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            // Create a new empty GameObject
            GameObject emptyObject = new("EmptyObject");

            emptyObject.transform.position = GetCombinedBottomCenterPosition(selectedObject);

            // Parent the selected object to the empty GameObject
            selectedObject.transform.parent = emptyObject.transform;

            // Select the empty GameObject
            Selection.activeGameObject = emptyObject;

            emptyObject.transform.position = Vector3.zero;
            emptyObject.name = selectedObject.name;

            Vector3 GetCombinedBottomCenterPosition(GameObject parentObject)
            {
                // Initialize combined bounds with first child's bounds
                Bounds combinedBounds = GetGameObjectBounds(parentObject.transform.GetChild(0).gameObject);

                // Iterate over all child GameObjects to calculate combined bounds
                for (int i = 1; i < parentObject.transform.childCount; i++)
                {
                    GameObject childObject = parentObject.transform.GetChild(i).gameObject;
                    combinedBounds.Encapsulate(GetGameObjectBounds(childObject));
                }

                // Calculate the bottom center position using the combined bounds
                return new Vector3(combinedBounds.center.x, combinedBounds.min.y, combinedBounds.center.z);
            }

            Bounds GetGameObjectBounds(GameObject gameObject)
            {
                // Get the renderer component of the game object
                Renderer renderer = gameObject.GetComponent<Renderer>();

                if (renderer != null)
                {
                    // Return the renderer bounds
                    return renderer.bounds;
                }
                else
                {
                    // Get the collider component of the game object
                    Collider collider = gameObject.GetComponent<Collider>();

                    if (collider != null)
                    {
                        // Return the collider bounds
                        return collider.bounds;
                    }
                    else
                    {
                        // If neither renderer nor collider is found, return empty bounds
                        Debug.LogWarning("No Renderer or Collider found on the GameObject.");
                        return new Bounds(Vector3.zero, Vector3.zero);
                    }
                }
            }
        }
    }
}