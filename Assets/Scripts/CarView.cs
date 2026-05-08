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
    private int dragStartX;
    private int dragStartY;

    private int previewX;
    private int previewY;

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

        previewX = X;
        previewY = Y;
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
        dragStartX = X;
        dragStartY = Y;

        previewX = X;
        previewY = Y;
    }

    private void OnMouseDrag()
    {
        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = 0f;

        Vector3 delta = mouse - dragStartWorld;

        int desiredDelta;

        if (Orientation == VehicleOrientation.Horizontal)
        {
            desiredDelta = Mathf.RoundToInt(delta.x / board.CellSize);
        }
        else
        {
            desiredDelta = Mathf.RoundToInt(delta.y / board.CellSize);
        }

        int allowedDelta = board.GetAllowedDelta(
            this,
            dragStartX,
            dragStartY,
            desiredDelta
        );

        previewX = dragStartX;
        previewY = dragStartY;

        if (Orientation == VehicleOrientation.Horizontal)
        {
            previewX += allowedDelta;
        }
        else
        {
            previewY += allowedDelta;
        }

        transform.position = board.GridToWorld(previewX, previewY, this);
    }

    private void OnMouseUp()
    {
        board.CommitMove(this, previewX, previewY);
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