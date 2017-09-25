using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField]
    private HexCell[] neighbors;

    private int elevation;

    public HexCoordinates coordinates;

    public Color color;
    public RectTransform uiRect;

    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            elevation = value;
            var position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            transform.localPosition = position;

            var uiPosition = uiRect.localPosition;
            uiPosition.z = value * -HexMetrics.elevationStep;
            uiRect.localPosition = uiPosition;
        }
    }

    private void Awake()
    {
        neighbors = new HexCell[6];
    }

    public HexCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }
}
