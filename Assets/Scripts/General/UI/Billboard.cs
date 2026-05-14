using UnityEngine;
public class Billboard : MonoBehaviour
{
    // code credits: https://www.youtube.com/watch?v=eiGvVgwtJ8k
  [SerializeField] public BillboardType type;
  [SerializeField] public bool YEnabled = false;
    public enum BillboardType { LookAtCamera, CameraForward }

    void LateUpdate()
    {
        switch (type)
        {
            case BillboardType.LookAtCamera:
                Vector3 directionToCamera = Camera.main.transform.position - transform.position;
                if(!YEnabled){
                    directionToCamera.y = 0f;
                }
       
                transform.rotation = Quaternion.LookRotation(-directionToCamera, Vector3.up);
                break;

            case BillboardType.CameraForward:

                Vector3 flatForward = Camera.main.transform.forward;
                if (!YEnabled)
                {
                    flatForward.y = 0f;   
                }
                transform.forward = flatForward.normalized;
                break;
        }
    }
}
