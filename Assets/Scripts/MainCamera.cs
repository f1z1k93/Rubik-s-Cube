using UnityEngine;
using UnityEngine.Assertions;

public class MainCamera : MonoBehaviour
{
    [SerializeField] private Transform RubiksCube;
    [SerializeField] private float Zoom;
    private float CubeRadius = 3.34f; // TODO: remove me

    private void LateUpdate()
    {
        // TODO: Not need focus on the cube each frame
        UpdateFieldOfView();
        UpdateRotation();
    }

    private void UpdateFieldOfView()
    {
        float currentFov = Camera.main.fieldOfView;

        float currentDistance = GetDistanceBetweenCubeAndScreenBorder();
        float expectedDistance = CubeRadius * Zoom;

        float expectedTan = (expectedDistance / currentDistance) * Mathf.Tan(currentFov * Mathf.Deg2Rad);
        float expectedFov = Mathf.Atan(expectedTan) * Mathf.Rad2Deg;

        Camera.main.fieldOfView = expectedFov;
    }

    private void UpdateRotation()
    {
        var pixelHeight = Camera.main.pixelHeight;
        var pixelWidth = Camera.main.pixelWidth;
        var rayToScreenCenter = Camera.main.ScreenPointToRay(new Vector2(0.5f * pixelWidth, 0.5f * pixelHeight));
        var directionToCube = RubiksCube.transform.position - transform.position;
        var directionToScreenCenter = rayToScreenCenter.direction;

        transform.rotation *= Quaternion.FromToRotation(directionToScreenCenter, directionToCube);
    }

    private float GetDistanceBetweenCubeAndScreenBorder()
    {
        var pixelHeight = Camera.main.pixelHeight;
        var pixelWidth = Camera.main.pixelWidth;
        var cubePlaneForCamera = new Plane(transform.position - RubiksCube.position, RubiksCube.position);

        var cameraRayOfBoundScreenPoint =
            pixelHeight > pixelWidth ? Camera.main.ScreenPointToRay(new Vector2(pixelWidth, 0.5f * pixelHeight)) :
                                       Camera.main.ScreenPointToRay(new Vector2(0.5f * pixelWidth, pixelHeight));

        float enter = 0;
        bool isRaycasted = cubePlaneForCamera.Raycast(cameraRayOfBoundScreenPoint, out enter);

        Assert.IsTrue(isRaycasted);

        return Vector3.Distance(RubiksCube.position, cameraRayOfBoundScreenPoint.GetPoint(enter));
    }
}
