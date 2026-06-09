using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(BoxCollider))]
public class CarView : MonoBehaviour
{
    public string Id { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }
    public int Length { get; private set; }
    public VehicleOrientation Orientation { get; private set; }
    public bool IsTarget { get; private set; }

    private BoardManager board;

    private Vector3 dragStartWorld;
    private Vector3 dragStartCarWorld;
    private Vector3 customScale = Vector3.one;

    private int dragStartX;
    private int dragStartY;

    private float minDragDistance;
    private float maxDragDistance;
    private float currentDragDistance;

    [Header("Outline Settings")]
    public float outlineWidth = 0.02f;       // Outline Thickness
    public Color outlineColor = Color.black; // Outline Color

    private LineRenderer[] edgeLines = new LineRenderer[12];

  
    private readonly string[] edgeNames = new string[12] {
        "Top_Front", "Top_Back", "Top_Left", "Top_Right",
        "Bottom_Front", "Bottom_Back", "Bottom_Left", "Bottom_Right",
        "Pillar_FrontLeft", "Pillar_FrontRight", "Pillar_BackLeft", "Pillar_BackRight"
    };

    public void Init(BoardManager boardManager, CarConfig config)
    {
        board = boardManager;

        Id = config.id;
        X = config.x;
        Y = config.y;
        Length = config.length;
        Orientation = config.orientation;
        IsTarget = config.isTarget;

        customScale = config.scale;

        ApplySurface(config);

        ApplySize();
        SnapToGrid();
        CreateOutlines();
    }

    private void ApplySize()
    {
        float gap = board.CellSize * 0.08f;

        float width = Orientation == VehicleOrientation.Horizontal
            ? Length * board.CellSize - gap
            : board.CellSize - gap;

        float depth = Orientation == VehicleOrientation.Vertical
            ? Length * board.CellSize - gap
            : board.CellSize - gap;

        transform.localScale = new Vector3(
            width * customScale.x,
            board.CarHeight * customScale.y,
            depth * customScale.z
        );
    }

    private void Update()
    {
        UpdateOutlines();
    }

    private void OnMouseDown()
    {
        if (board.IsInputBlocked()) return;

        dragStartWorld = GetMouseWorldOnDragPlane();
        dragStartCarWorld = transform.position;

        dragStartX = X;
        dragStartY = Y;

        currentDragDistance = 0f;

        int minDelta = board.GetAllowedDelta(this, dragStartX, dragStartY, -20);
        int maxDelta = board.GetAllowedDelta(this, dragStartX, dragStartY, 20);

        minDragDistance = minDelta * board.CellSize;
        maxDragDistance = maxDelta * board.CellSize;

        if (IsTarget && Orientation == VehicleOrientation.Horizontal)
        {
            float maxToExit = board.GetMaxDragDistanceToTargetExit(
                this,
                dragStartCarWorld
            );

            maxDragDistance = Mathf.Min(maxDragDistance, maxToExit);
        }
    }

    private void OnMouseDrag()
    {
        if (board.IsInputBlocked()) return;

        Vector3 mouse = GetMouseWorldOnDragPlane();
        Vector3 delta = mouse - dragStartWorld;

        float rawDistance;

        if (Orientation == VehicleOrientation.Horizontal)
        {
            rawDistance = delta.x;
        }
        else
        {
            rawDistance = delta.z;
        }

        currentDragDistance = Mathf.Clamp(
            rawDistance,
            minDragDistance,
            maxDragDistance
        );

        Vector3 offset;

        if (Orientation == VehicleOrientation.Horizontal)
        {
            offset = new Vector3(currentDragDistance, 0f, 0f);
        }
        else
        {
            offset = new Vector3(0f, 0f, currentDragDistance);
        }

        transform.position = dragStartCarWorld + offset;
    }

    private void OnMouseUp()
    {

        if (board.IsInputBlocked()) return;

        int cellDelta = Mathf.RoundToInt(currentDragDistance / board.CellSize);

        int newX = dragStartX;
        int newY = dragStartY;

        if (Orientation == VehicleOrientation.Horizontal)
        {
            newX += cellDelta;
        }
        else
        {
            newY += cellDelta;
        }

        board.CommitMove(this, newX, newY);
    }

    private Vector3 GetMouseWorldOnDragPlane()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Plane dragPlane = new Plane(
            Vector3.up,
            new Vector3(0f, board.CarHeight * 0.5f + 0.08f, 0f)
        );

        if (dragPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }

        return transform.position;
    }

    public void SetGridPosition(int x, int y)
    {
        X = x;
        Y = y;
        SnapToGrid();
    }

    public void SnapToGrid()
    {
        transform.position = board.GridToWorld(X, Y, this);
    }

    public bool Occupies(int gridX, int gridY)
    {
        for (int i = 0; i < Length; i++)
        {
            int cellX = X + (Orientation == VehicleOrientation.Horizontal ? i : 0);
            int cellY = Y + (Orientation == VehicleOrientation.Vertical ? i : 0);

            if (cellX == gridX && cellY == gridY)
            {
                return true;
            }
        }

        return false;
    }
    private void ApplyColor(Color color)
    {
        color.a = 1f;

        Renderer renderer = GetComponent<Renderer>();

        Material material = renderer.material;

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
        else
        {
            Debug.LogWarning("Material has no color property: " + material.name);
        }
    }

    private void CreateOutlines()
    {
        Material lineMat = new Material(Shader.Find("Sprites/Default"));

        for (int i = 0; i < 12; i++)
        {
            GameObject lineObj = new GameObject(edgeNames[i]);
            lineObj.transform.SetParent(transform);

            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.positionCount = 2;
            lr.startWidth = outlineWidth;
            lr.endWidth = outlineWidth;
            lr.material = lineMat;
            lr.startColor = outlineColor;
            lr.endColor = outlineColor;

            lr.numCapVertices = 5;

            lr.useWorldSpace = true;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;

            edgeLines[i] = lr;
        }
    }

    private void UpdateOutlines()
    {
        if (edgeLines == null || edgeLines[0] == null) return;

        Vector3 center = transform.position;

        Vector3 padding = new Vector3(0.01f, 0.01f, 0.01f);
        Vector3 extents = (transform.localScale * 0.5f) + padding;

        Vector3 tFL = center + new Vector3(-extents.x, extents.y, extents.z);
        Vector3 tFR = center + new Vector3(extents.x, extents.y, extents.z);
        Vector3 tBL = center + new Vector3(-extents.x, extents.y, -extents.z);
        Vector3 tBR = center + new Vector3(extents.x, extents.y, -extents.z);

        Vector3 bFL = center + new Vector3(-extents.x, -extents.y, extents.z);
        Vector3 bFR = center + new Vector3(extents.x, -extents.y, extents.z);
        Vector3 bBL = center + new Vector3(-extents.x, -extents.y, -extents.z);
        Vector3 bBR = center + new Vector3(extents.x, -extents.y, -extents.z);

        SetLinePosition(0, tFL, tFR);  // Top_Front
        SetLinePosition(1, tBL, tBR);  // Top_Back
        SetLinePosition(2, tFL, tBL);  // Top_Left
        SetLinePosition(3, tFR, tBR);  // Top_Right

        SetLinePosition(4, bFL, bFR);  // Bottom_Front
        SetLinePosition(5, bBL, bBR);  // Bottom_Back
        SetLinePosition(6, bFL, bBL);  // Bottom_Left
        SetLinePosition(7, bFR, bBR);  // Bottom_Right

        SetLinePosition(8, tFL, bFL);  // Pillar_FrontLeft
        SetLinePosition(9, tFR, bFR);  // Pillar_FrontRight
        SetLinePosition(10, tBL, bBL); // Pillar_BackLeft
        SetLinePosition(11, tBR, bBR); // Pillar_BackRight
    }


    private void SetLinePosition(int index, Vector3 start, Vector3 end)
    {
        if (edgeLines[index] != null && edgeLines[index].gameObject.activeSelf)
        {
            edgeLines[index].startWidth = outlineWidth;
            edgeLines[index].endWidth = outlineWidth;

            edgeLines[index].SetPosition(0, start);
            edgeLines[index].SetPosition(1, end);
        }
    }

    
    public void ToggleEdge(string edgeName, bool isVisible)
    {
        for (int i = 0; i < 12; i++)
        {
            if (edgeNames[i] == edgeName && edgeLines[i] != null)
            {
                edgeLines[i].gameObject.SetActive(isVisible);
                break;
            }
        }
    }
}

