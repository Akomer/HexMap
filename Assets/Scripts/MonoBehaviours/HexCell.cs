using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    [SerializeField]
    private HexCell[] neighbors;

    private const int NOT_REAL_ELEVATION_FOR_START = int.MinValue;

    private int elevation = NOT_REAL_ELEVATION_FOR_START;
    private Color color;

    public HexCoordinates coordinates;

    public RectTransform uiRect;
    public HexGridChunk chunk;

    public static HexCell nullCell = new HexCell();

    #region Properties

    public int Elevation
    {
        get
        {
            return elevation;
        }
        set
        {
            if (elevation == value)
            {
                return;
            }

            elevation = value;
            var position = transform.localPosition;
            position.y = value * HexMetrics.elevationStep;
            position.y += (NoiseProvider.SampleNoise(position).y * 2f - 1f) *
                HexMetrics.elevationPerturbStrength;
            transform.localPosition = position;

            var uiPosition = uiRect.localPosition;
            uiPosition.z = -position.y;
            uiRect.localPosition = uiPosition;

            Refresh();
        }
    }

    public Color Color
    {
        get { return color; }
        set
        {
            if (color == value)
            {
                return;
            }
            color = value;
            Refresh();
        }
    }

    #endregion

    public Vector3 Position
    {
        get { return transform.localPosition; }
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

    public HexEdgeType GetEdgeType(HexDirection direction)
    {
        return HexMetrics.GetEdgeType(elevation, neighbors[(int)direction].elevation);
    }

    public HexEdgeType GetEdgeType(HexCell other)
    {
        return HexMetrics.GetEdgeType(elevation, other.elevation);
    }

    private void Refresh()
    {
        if (chunk)
        {
            chunk.Refresh();
            for(var i = 0; i < neighbors.Length; i++)
            {
                var neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }
}
