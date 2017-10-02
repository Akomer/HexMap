using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour {

    private HexCell[] cells;

    private HexMesh hexMesh;
    private Canvas gridCanvas;

    private bool refreshCalled;

    private void Awake()
    {
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];

    }

    private void Start()
    {
        RefreshMesh();
        refreshCalled = false;
    }

    private void LateUpdate()
    {
        if(refreshCalled)
        {
            RefreshMesh();
            refreshCalled = false;
        }
    }

    public void ToggleCoordinateLabels(bool toggle)
    {
        gridCanvas.gameObject.SetActive(toggle);
    }

    public void AddCell(int index, HexCell cell)
    {
        cells[index] = cell;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(gridCanvas.transform, false);
        cell.chunk = this;
    }

    public void Refresh()
    {
        refreshCalled = true;
    }

    private void RefreshMesh()
    {
        hexMesh.Triangulate(cells);
    }

}
