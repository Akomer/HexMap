using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapCamera : MonoBehaviour
{

    private Transform swivel, stick;
    private float zoom = 1f;
    private float rotationAngle;

    public float stickMinZoom, stickMaxZoom;
    public float swivelMinZoom, swivelMaxZoom;
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float rotationSpeed;

    public HexGrid grid;

    private void Awake()
    {
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
    }

    private void Update()
    {
        var zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        var rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0)
        {
            AdjustRotation(rotationDelta);
        }

        var xDelta = Input.GetAxis("Horizontal");
        var zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0 || zDelta != 0)
        {
            AdjustPosition(xDelta, zDelta);
        }
    }

    private void AdjustPosition(float xDelta, float zDelta)
    {
        var distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * Time.deltaTime;
        var direction = transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
        var damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));

        var position = transform.localPosition;
        position += direction * distance * damping;
        transform.localPosition = ClampPositon(position);
    }

    private Vector3 ClampPositon(Vector3 position)
    {
        var xMax =
            (grid.chunkCountX * HexMetrics.chunkSizeX - 0.5f) *
            (2f * HexMetrics.innerRadius);
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        var zMax =
            (grid.chunkCountZ * HexMetrics.chunkSizeZ - 1f) *
            (1.5f * HexMetrics.outerRadius);
        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
    }

    private void AdjustZoom(float zoomDelta)
    {
        zoom = Mathf.Clamp01(zoom + zoomDelta);

        var distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);

        var angel = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angel, 0f, 0f);
    }

    public void AdjustRotation(float rotationDelta)
    {
        rotationAngle += rotationDelta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }
        else if (rotationAngle > 360f)
        {
            rotationAngle -= 360f;
        }

        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }
}
