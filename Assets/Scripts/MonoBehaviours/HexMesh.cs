using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{

    private Mesh hexMesh;
    private static List<Vector3> vertices = new List<Vector3>();
    private static List<int> triangles = new List<int>();
    private static List<Color> colors = new List<Color>();
    private static List<Vector2> uv = new List<Vector2>();
    private MeshCollider meshCollider;

    void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
    }

    public void Triangulate(HexCell[] cells)
    {
        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        for (var i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }
        hexMesh.vertices = vertices.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.uv = uv.ToArray();
        hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;
    }

    private void Triangulate(HexCell cell)
    {
        for (var d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    private void Triangulate(HexDirection direction, HexCell cell)
    {
        //var startVertexCounter = vertices.Count;

        var center = cell.Position;
        var edge = new EdgeVertices(
            center + HexMetrics.GetFirstSolidCorner(direction),
            center + HexMetrics.GetSecondSolidCorner(direction)
        );

        TriangulateEdgeFan(center, edge, cell.Color);

        //var haxVertexCounter = vertices.Count;
        //AddCellUVs(startVertexCounter, haxVertexCounter, center);

        if (direction <= HexDirection.SE)
        {
            CreateBridge(direction, cell, edge);
        }

        //var endVertexCounter = vertices.Count;
        //for(var i = haxVertexCounter; i < endVertexCounter; i++)
        //{
        //    uv.Add(Vector2.zero);
        //}

    }

    private void AddCellUVs(int i, int iEnd, Vector3 center)
    {
        var tile = UnityEngine.Random.insideUnitCircle.normalized;
        for(var j = i; j < iEnd; j++)
        {
            var nextUvCoord = new Vector2();
            var zeroBasedV3 = vertices[j] - center;
            nextUvCoord.x = (zeroBasedV3.x + HexMetrics.maxRadious) / HexMetrics.maxRadious;
            nextUvCoord.y = (zeroBasedV3.z + HexMetrics.maxRadious) / HexMetrics.maxRadious;
            nextUvCoord += tile;
            uv.Add(nextUvCoord);
        }
    }


    private void TriangulateEdgeFan(Vector3 center, EdgeVertices edge, Color color)
    {
        AddTriangle(center, edge.v1, edge.v2);
        AddTriangleColor(color);
        AddTriangle(center, edge.v2, edge.v3);
        AddTriangleColor(color);
        AddTriangle(center, edge.v3, edge.v4);
        AddTriangleColor(color);
    }

    private void TriangulateEdgeStrip(
        EdgeVertices e1, Color c1,
        EdgeVertices e2, Color c2)
    {
        AddQuad(e1.v1, e1.v2, e2.v1, e2.v2);
        AddQuadColor(c1, c2);
        AddQuad(e1.v2, e1.v3, e2.v2, e2.v3);
        AddQuadColor(c1, c2);
        AddQuad(e1.v3, e1.v4, e2.v3, e2.v4);
        AddQuadColor(c1, c2);
    }

    private void CreateBridge(
        HexDirection direction, HexCell cell, EdgeVertices edge)
    {
        var neighbor = cell.GetNeighbor(direction);
        if (neighbor == null)
        {
            return;
        }

        var bridge = HexMetrics.GetBridge(direction);
        bridge.y = neighbor.Position.y - cell.Position.y;
        var e2 = new EdgeVertices(
            edge.v1 + bridge,
            edge.v4 + bridge
        );

        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(edge, cell, e2, neighbor);
        }
        else
        {
            TriangulateEdgeStrip(edge, cell.Color, e2, neighbor.Color);
        }

        CreateTriangleCorners(direction, cell, edge.v4, neighbor, e2.v4);

    }

    private void CreateTriangleCorners(HexDirection direction, HexCell cell, Vector3 v2, HexCell neighbor, Vector3 v4)
    {
        var nextNeighbor = cell.GetNeighbor(direction.Next());
        if (nextNeighbor != null && direction <= HexDirection.E)
        {
            var v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Position.y;
            if (cell.Elevation <= neighbor.Elevation)
            {
                if (cell.Elevation <= nextNeighbor.Elevation)
                {
                    TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
            {
                TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
            }
        }
    }

    private void TriangulateEdgeTerraces(
        EdgeVertices begin, HexCell beginCell,
        EdgeVertices end, HexCell endCell)
    {
        var e2 = EdgeVertices.TerraceLerp(begin, end, 1);
        var c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

        TriangulateEdgeStrip(begin, beginCell.Color, e2, c2);

        for (var i = 2; i < HexMetrics.terraceSteps; i++)
        {
            var e1 = e2;
            var c1 = c2;
            e2 = EdgeVertices.TerraceLerp(begin, end, i);
            c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
            TriangulateEdgeStrip(e1, c1, e2, c2);
        }

        TriangulateEdgeStrip(e2, c2, end, endCell.Color);
    }

    private void TriangulateCorner(
        Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        var leftEdge = bottomCell.GetEdgeType(leftCell);
        var rightEdge = bottomCell.GetEdgeType(rightCell);

        if (leftEdge == HexEdgeType.Slope)
        {
            if (rightEdge == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
                return;
            }
            if (rightEdge == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(left, leftCell, right, rightCell, bottom, bottomCell);
                return;
            }
            TriangulateCornerTerracesCliff(bottom, bottomCell, left, leftCell, right, rightCell);
            return;
        }
        if (rightEdge == HexEdgeType.Slope)
        {
            if (leftEdge == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                return;
            }
            TriangulateCornerCliffTerraces(bottom, bottomCell, left, leftCell, right, rightCell);
            return;
        }
        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Elevation < rightCell.Elevation)
            {
                TriangulateCornerCliffTerraces(right, rightCell, bottom, bottomCell, left, leftCell);
                return;
            }
            TriangulateCornerTerracesCliff(left, leftCell, right, rightCell, bottom, bottomCell);
            return;
        }

        AddTriangle(bottom, left, right);
        AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);

    }

    private void TriangulateCornerTerraces(
        Vector3 center, HexCell centerCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        var v3 = HexMetrics.TerraceLerp(center, left, 1);
        var v4 = HexMetrics.TerraceLerp(center, right, 1);
        var c3 = HexMetrics.TerraceLerp(centerCell.Color, leftCell.Color, 1);
        var c4 = HexMetrics.TerraceLerp(centerCell.Color, rightCell.Color, 1);

        AddTriangle(center, v3, v4);
        AddTriangleColor(centerCell.Color, c3, c4);

        for (var i = 2; i < HexMetrics.terraceSteps; i++)
        {
            var v1 = v3;
            var v2 = v4;
            var c1 = c3;
            var c2 = c4;
            v3 = HexMetrics.TerraceLerp(center, left, i);
            v4 = HexMetrics.TerraceLerp(center, right, i);
            c3 = HexMetrics.TerraceLerp(centerCell.Color, leftCell.Color, i);
            c4 = HexMetrics.TerraceLerp(centerCell.Color, rightCell.Color, i);

            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
        }

        AddQuad(v3, v4, left, right);
        AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
    }

    private void TriangulateCornerTerracesCliff(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        var t = (float)(leftCell.Elevation - beginCell.Elevation) / (rightCell.Elevation - beginCell.Elevation);
        var boundary = Vector3.Lerp(Perturb(begin), Perturb(right), t);
        var boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, t);

        TriangulateBoundaryTriangle(begin, beginCell, left, leftCell, boundary, boundaryColor);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
        }
    }

    private void TriangulateCornerCliffTerraces(
    Vector3 begin, HexCell beginCell,
    Vector3 left, HexCell leftCell,
    Vector3 right, HexCell rightCell)
    {
        var t = (float)(rightCell.Elevation - beginCell.Elevation) / (leftCell.Elevation - beginCell.Elevation);
        var boundary = Vector3.Lerp(Perturb(begin), Perturb(left), t);
        var boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, t);

        TriangulateBoundaryTriangle(right, rightCell, begin, beginCell, boundary, boundaryColor);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            AddTriangleUnperturbed(Perturb(left), Perturb(right), boundary);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
        }
    }

    private void TriangulateBoundaryTriangle(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor)
    {
        var v2 = Perturb(HexMetrics.TerraceLerp(begin, left, 1));
        var c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

        AddTriangleUnperturbed(Perturb(begin), v2, boundary);
        AddTriangleColor(beginCell.Color, c2, boundaryColor);

        for (var i = 2; i < HexMetrics.terraceSteps; i++)
        {
            var v1 = v2;
            var c1 = c2;
            v2 = Perturb(HexMetrics.TerraceLerp(begin, left, i));
            c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);

            AddTriangleUnperturbed(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }

        AddTriangleUnperturbed(v2, Perturb(left), boundary);
        AddTriangleColor(c2, leftCell.Color, boundaryColor);
    }

    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var vertexIndex = vertices.Count;
        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    private void AddTriangleColor(Color color)
    {
        AddTriangleColor(color, color, color);
    }

    private void AddTriangleColor(Color color1, Color color2, Color color3)
    {
        colors.Add(color1);
        colors.Add(color2);
        colors.Add(color3);
    }

    private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        var vertexIndex = vertices.Count;
        vertices.Add(Perturb(v1));
        vertices.Add(Perturb(v2));
        vertices.Add(Perturb(v3));
        vertices.Add(Perturb(v4));
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    private void AddQuadColor(Color color1, Color color2)
    {
        AddQuadColor(color1, color1, color2, color2);
    }

    private void AddQuadColor(Color color1, Color color2, Color color3, Color color4)
    {
        colors.Add(color1);
        colors.Add(color2);
        colors.Add(color3);
        colors.Add(color4);
    }

    private Vector3 Perturb(Vector3 position)
    {
        var sample = NoiseProvider.SampleNoise(position);
        position.x += (sample.x * 2f - 1f) * HexMetrics.cellPerturbStrength;
        position.z += (sample.z * 2f - 1f) * HexMetrics.cellPerturbStrength;
        return position;
    }

    private void AddTriangleUnperturbed(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

}
