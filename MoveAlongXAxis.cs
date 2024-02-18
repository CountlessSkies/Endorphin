using UnityEngine;

public class MoveAlongXAxis : MonoBehaviour
{
    public float speed = 5f; // Exposed variable for controlling speed

    // Update is called once per frame
    void Update()
    {
        // Calculate the movement amount based on speed and time
        float movement = speed * Time.deltaTime;

        // Move the object along the X-axis
        transform.Translate(Vector3.right * movement);
    }
}