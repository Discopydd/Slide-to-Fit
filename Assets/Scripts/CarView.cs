using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
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

        GetComponent<SpriteRenderer>().color = config.color;

        ApplySize();
        SnapToGrid();
    }

    private void ApplySize()
    {
        float gap = board.CellSize * 0.08f;

        float width = Orientation == VehicleOrientation.Horizontal
            ? Length * board.CellSize - gap
            : board.CellSize - gap;

        float height = Orientation == VehicleOrientation.Vertical
            ? Length * board.CellSize - gap
            : board.CellSize - gap;

        transform.localScale = new Vector3(width, height, 1f);
    }

    private void OnMouseDown()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0f;

        dragStartWorld = mouse;
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
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0f;

        Vector3 delta = mouse - dragStartWorld;

        float rawDistance;

        if (Orientation == VehicleOrientation.Horizontal)
        {
            rawDistance = delta.x;
        }
        else
        {
            rawDistance = delta.y;
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
            offset = new Vector3(0f, currentDragDistance, 0f);
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
}