using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{

    public Color[] colors;
    public HexGrid hexGrid;

    private Color activeColor;
    private int activeElevation;
    private bool applyColor;
    private bool applyElevation;
    private int brushSize;

    private void Awake()
    {
        SelectColor(-1);
    }

    private void Update()
    {
        if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            HandleInput();
        }
    }

    public void SelectColor(int index)
    {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }
        
    }

    public void SetApplyElevation(bool toggle)
    {
        applyElevation = toggle;
    }

    public void SetElevation(float elevation)
    {
        activeElevation = (int)elevation;
    }

    public void SetBrushSize(float size)
    {
        brushSize = (int)size;
    }

    private void HandleInput()
    {
        var inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            var cell = hexGrid.GetCell(hit.point);
            EditCells(cell);
        }
    }

    private void EditCells(HexCell center)
    {
        var centerX = center.coordinates.X;
        var centerZ = center.coordinates.Z;
        
        for (int r = 0, z = centerZ - brushSize; z<= centerZ; z++, r++)
        {
            for (var x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }

        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (var x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
    }

    private void EditCell(HexCell cell)
    {
        if (cell == HexCell.nullCell)
        {
            return;
        }

        if (applyColor)
        {
            cell.Color = activeColor;
        }
        if (applyElevation)
        {
            cell.Elevation = activeElevation;
        }
    }
}
