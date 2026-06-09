using UnityEngine;

public class SmartOutline : MonoBehaviour
{
    [Header("Outline settings")]
    public float outlineWidth = 0.08f;      // Line thickness
    public Color outlineColor = Color.black; // Line color

    // Variables for Controlling Individual Lines
    private LineRenderer lineTop, lineBottom, lineLeft, lineRight;

    void Start()
    {
        // 1. Calculate the Actual Size (World Coordinates) Based on the Current Block's Collider
        BoxCollider col = GetComponent<BoxCollider>();
        if (col == null) col = GetComponentInChildren<BoxCollider>();

        if (col == null)
        {
            Debug.LogWarning("Cannot Draw Outline: BoxCollider Not Found!");
            return;
        }

        Vector3 center = col.bounds.center;
        Vector3 extents = col.bounds.extents;

        // 2. Get the Coordinates of the Four Vertices on the Top Face of the Block
        Vector3 topLeft = center + new Vector3(-extents.x, extents.y, extents.z);
        Vector3 topRight = center + new Vector3(extents.x, extents.y, extents.z);
        Vector3 bottomLeft = center + new Vector3(-extents.x, extents.y, -extents.z);
        Vector3 bottomRight = center + new Vector3(extents.x, extents.y, -extents.z);

        // 3. Create Four Lines Independently
        lineTop = CreateEdge("Edge_Top", topLeft, topRight);
        lineBottom = CreateEdge("Edge_Bottom", bottomLeft, bottomRight);
        lineLeft = CreateEdge("Edge_Left", topLeft, bottomLeft);
        lineRight = CreateEdge("Edge_Right", topRight, bottomRight);
    }

    private LineRenderer CreateEdge(string edgeName, Vector3 start, Vector3 end)
    {
        // Create an Empty Object and Add It as a Child
        GameObject edgeObj = new GameObject(edgeName);
        edgeObj.transform.SetParent(transform);

        LineRenderer lr = edgeObj.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);

        // Set the Solid-Color Material and Line Thickness
        lr.startWidth = outlineWidth;
        lr.endWidth = outlineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default")); 
        lr.startColor = outlineColor;
        lr.endColor = outlineColor;
        lr.useWorldSpace = true; // Keep World Coordinates so the Lines Follow Correctly Even When the Block Moves

        return lr;
    }

    
    public void ToggleEdge(string edgeName, bool isVisible)
    {
        if (edgeName == "Top" && lineTop != null) lineTop.gameObject.SetActive(isVisible);
        else if (edgeName == "Bottom" && lineBottom != null) lineBottom.gameObject.SetActive(isVisible);
        else if (edgeName == "Left" && lineLeft != null) lineLeft.gameObject.SetActive(isVisible);
        else if (edgeName == "Right" && lineRight != null) lineRight.gameObject.SetActive(isVisible);
    }
}
