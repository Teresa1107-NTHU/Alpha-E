/*
 * Alpha-E 3D 模型鏡頭控制：
 *
 * 1. 按住滑鼠左鍵拖曳：繞模型旋轉視角
 * 2. 按住滑鼠右鍵拖曳：平移鏡頭
 * 3. 滑鼠滾輪：放大 / 縮小
 * 4. R 鍵：回到初始視角
 *
 * 適用於 Unity WebGL 的 Alpha-E 3D Viewer。
 */

using UnityEngine;

public class AlphaECameraController : MonoBehaviour
{
    [Header("旋轉中心")]
    [SerializeField]
    private Transform target;


    [Header("旋轉設定")]
    [SerializeField]
    private float rotationSpeed = 4f;


    [Header("平移設定")]
    [SerializeField]
    private float panSpeed = 0.0025f;


    [Header("縮放設定")]
    [SerializeField]
    private float zoomSpeed = 2.5f;

    [SerializeField]
    private float minZoomRatio = 0.35f;

    [SerializeField]
    private float maxZoomRatio = 2.5f;


    private Vector3 initialPosition;
    private Quaternion initialRotation;

    private float distance;
    private float initialDistance;


    private void Start()
    {
        initialPosition =
            transform.position;

        initialRotation =
            transform.rotation;


        if (target != null)
        {
            distance =
                Vector3.Distance(
                    transform.position,
                    target.position
                );

            initialDistance =
                distance;
        }
    }


    private void Update()
    {
        if (target == null)
        {
            return;
        }


        HandleRotation();

        HandlePan();

        HandleZoom();


        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetView();
        }
    }


    /*
     * 左鍵拖曳：
     * 繞著 target 旋轉。
     */
    private void HandleRotation()
    {
        if (!Input.GetMouseButton(0))
        {
            return;
        }


        float mouseX =
            Input.GetAxis("Mouse X");

        float mouseY =
            Input.GetAxis("Mouse Y");


        transform.RotateAround(
            target.position,
            Vector3.up,
            mouseX * rotationSpeed
        );


        transform.RotateAround(
            target.position,
            transform.right,
            -mouseY * rotationSpeed
        );


        transform.LookAt(
            target.position
        );
    }


    /*
     * 右鍵拖曳：
     * 同時移動 Camera 與旋轉中心。
     */
    private void HandlePan()
    {
        if (!Input.GetMouseButton(1))
        {
            return;
        }


        float mouseX =
            Input.GetAxis("Mouse X");

        float mouseY =
            Input.GetAxis("Mouse Y");


        Vector3 move =
            (
                -transform.right * mouseX
                -
                transform.up * mouseY
            )
            *
            panSpeed
            *
            distance;


        transform.position += move;

        target.position += move;
    }


    /*
 * 滑鼠滾輪：
 * 沿模型中心進行縮放，
 * 並避免 Camera 穿過 Target。
 */
    private void HandleZoom()
    {
        float scroll =
            Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) < 0.01f)
        {
            return;
        }


        float currentDistance =
            Vector3.Distance(
                transform.position,
                target.position
            );


        float minDistance =
            initialDistance *
            minZoomRatio;

        float maxDistance =
            initialDistance *
            maxZoomRatio;


        /*
         * 根據滾輪計算新距離。
         *
         * 往上滾：
         * distance 變小
         *
         * 往下滾：
         * distance 變大
         */
        float newDistance =
            currentDistance
            -
            scroll *
            zoomSpeed;


        newDistance =
            Mathf.Clamp(
                newDistance,
                minDistance,
                maxDistance
            );


        /*
         * 保留 Camera 目前相對 Target 的方向，
         * 只改距離。
         *
         * 這樣就不會穿過 Target。
         */
        Vector3 direction =
            (
                transform.position
                -
                target.position
            ).normalized;


        transform.position =
            target.position
            +
            direction *
            newDistance;


        transform.LookAt(
            target.position
        );


        distance =
            newDistance;
    }


    /*
     * 回到初始視角。
     */
    public void ResetView()
    {
        transform.position =
            initialPosition;

        transform.rotation =
            initialRotation;


        distance =
            Vector3.Distance(
                transform.position,
                target.position
            );
    }
}