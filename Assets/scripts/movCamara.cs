using UnityEngine;

public class movCamara : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smoothTime = 0.15f;
    [SerializeField] Vector3 offset = new Vector3(0f, 1f, -10f);

    Vector3 velocity;

    void LateUpdate()
    {
        Vector3 targetPos = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
    }
}