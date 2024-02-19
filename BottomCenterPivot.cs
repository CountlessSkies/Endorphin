using UnityEngine;
using UnityEditor;
using System.Linq;

public class MyMenu : MonoBehaviour
{
    [MenuItem("Endorphin/Bottom Center Pivot")]
    static void BottomCenterPivot()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects != null)
        {
            foreach (GameObject selectedObject in selectedObjects)
            {
                if (selectedObject != null)
                {
                    PrefabUtility.UnpackPrefabInstance(selectedObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

                    int childCount = selectedObject.transform.childCount;
                    Transform[] children = new Transform[childCount];
                    int index = 0;
                    foreach (Transform child in selectedObject.transform)
                    {
                        children[index++] = child;
                    }

                    Vector3 bottomCenterPivot = Vector3.zero;
                    float lowestY = float.MaxValue;

                    // Unparent the children objects
                    foreach (Transform child in children)
                    {
                        child.parent = null;
                        Vector3 childBottomCenterPivot = GetBottomCenterPosition(child.gameObject);
                        bottomCenterPivot += childBottomCenterPivot;
                        if (childBottomCenterPivot.y < lowestY)
                            lowestY = childBottomCenterPivot.y;
                    }

                    bottomCenterPivot /= childCount;
                    bottomCenterPivot.y = lowestY;
                    selectedObject.transform.position = bottomCenterPivot;

                    foreach (Transform child in children)
                    {
                        child.parent = selectedObject.transform;
                    }

                    Vector3 newPosition = selectedObject.transform.position;
                    newPosition.y = 0;
                    selectedObject.transform.position = newPosition;

                    // PrefabUtility.SaveAsPrefabAssetAndConnect(selectedObject, "Assets/Models/" + selectedObject.name + ".prefab", InteractionMode.UserAction);

                    // // Create a new empty GameObject
                    // GameObject emptyObject = new("EmptyObject");

                    // emptyObject.transform.position = GetCombinedBottomCenterPosition(selectedObject);

                    // // Parent the selected object to the empty GameObject
                    // selectedObject.transform.parent = emptyObject.transform;

                    // // Select the empty GameObject
                    // Selection.activeGameObject = emptyObject;

                    // Vector3 newPosition = emptyObject.transform.position;
                    // newPosition.y = 0;
                    // emptyObject.transform.position = newPosition;
                    // emptyObject.name = selectedObject.name;
                }
            }
        }

        static Vector3 GetBottomCenterPosition(GameObject obj)
        {
            // Get the bounds of the GameObject
            Bounds bounds = obj.GetComponent<Renderer>().bounds;

            // Calculate the bottom center position
            Vector3 bottomCenter = new(bounds.center.x, bounds.min.y, bounds.center.z);

            return bottomCenter;
        }
    }
}