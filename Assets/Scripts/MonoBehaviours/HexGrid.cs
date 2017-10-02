using System;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    private HexCell[] cells;
    private HexGridChunk[] chunks;

    int cellCountX;
    int cellCountZ;
    public int chunkCountX = 4;
    public int chunkCountZ = 3;
    public Color defaultColor = Color.white;
    public Color touchedColor = Color.magenta;

    public HexCell cellPrefab;
    public Text cellLabelPrefab;
    public HexGridChunk chunkPrefab;

    public Texture2D mapNoise;

    private void Awake()
    {
        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
        cells = new HexCell[cellCountZ * cellCountX];

        NoiseProvider.Init(mapNoise);

        CreateChunks();
        CreateCells();
    }

    private void OnEnable()
    {
        NoiseProvider.Init(mapNoise);
    }

    private void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];
        for (int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for (var x = 0; x < chunkCountX; x++, i++)
            {
                var chunk = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
                chunks[i] = chunk;
            }
        }
    }

    private void CreateCells()
    {
        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (var x = 0; x < cellCountX; x++, i++)
            {
                CreateCell(x, z, i);
            }
        }
    }

    public void ColorCell(Vector3 position, Color color)
    {
        var cell = GetCell(position);
        cell.Color = color;
    }

    public HexCell GetCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        var coordinates = HexCoordinates.FromPosition(position);
        var index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        index = Mathf.Clamp(index, 0, cells.Length-1);
        return cells[index];
    }

    public HexCell GetCell(HexCoordinates coordinates)
    { 
        var z = coordinates.Z;
        if (z < 0 || z >= cellCountZ)
        {
            return HexCell.nullCell;
        }

        var x = coordinates.X + z / 2;
        if (x < 0 || x >= cellCountX)
        {
            return HexCell.nullCell;
        }

        return cells[x + z * cellCountX];
    }
     
    public void ToggleCoordinateLabels(bool toggle)
    {
        for(var i = 0; i < chunks.Length; i++)
        {
            chunks[i].ToggleCoordinateLabels(toggle);
        }
    }

    private void TouchCell(Vector3 position)
    {
        ColorCell(position, touchedColor);
    }

    private void CreateCell(int x, int z, int i)
    {
        var pos = new Vector3(
            (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f),
            0f,
            z * (HexMetrics.outerRadius * 1.5f));

        var cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.localPosition = pos;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.Color = defaultColor;

        SetNeighbors(x, z, i, cell);

        var label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.anchoredPosition = new Vector2(pos.x, pos.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();
        cell.uiRect = label.rectTransform;
        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);
    }

    private void AddCellToChunk(int x, int z, HexCell cell)
    {
        var chunkX = x / HexMetrics.chunkSizeX;
        var chunkZ = z / HexMetrics.chunkSizeZ;
        var chunk = chunks[chunkX + chunkZ * chunkCountX];

        var localX = x - chunkX * HexMetrics.chunkSizeX;
        var localZ = z - chunkZ * HexMetrics.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    private void SetNeighbors(int x, int z, int i, HexCell cell)
    {
        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if (x < cellCountX - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }
    }
}
