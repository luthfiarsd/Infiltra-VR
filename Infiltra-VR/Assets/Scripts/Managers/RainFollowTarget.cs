using UnityEngine;

public class RainFollowTarget : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float heightOffset = 10f;
    [SerializeField] bool followY;

    void LateUpdate()
    {
        if (target == null && Camera.main != null)
            target = Camera.main.transform;

        if (target == null)
            return;

        var position = transform.position;
        position.x = target.position.x;
        position.z = target.position.z;

        if (followY)
            position.y = target.position.y + heightOffset;
        else
            position.y = heightOffset;

        transform.position = position;
    }
}
