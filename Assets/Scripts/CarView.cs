using UnityEngine;

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

        ApplyColor(config.color);

        ApplySize();
        SnapToGrid();
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

    private void OnMouseDown()
    {
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
            new Vector3(0f, board.CarHeight * 0.5f, 0f)
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
}