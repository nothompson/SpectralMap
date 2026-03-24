using UnityEngine;
public class Billboard : MonoBehaviour
{
    // code credits: https://www.youtube.com/watch?v=eiGvVgwtJ8k
    [SerializeField] public BillboardType type;
    public enum BillboardType {LookAtCamera, CameraForward};

    void LateUpdate()
    {
        switch (type)
        {
            case BillboardType.LookAtCamera:
                transform.LookAt(Camera.main.transform.position, Vector3.up);
                break;
            case BillboardType.CameraForward:
                transform.forward = Camera.main.transform.forward;
                break;
            default:
            break;
        }
    }
}
