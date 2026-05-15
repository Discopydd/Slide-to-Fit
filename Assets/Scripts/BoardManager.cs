using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BoardManager : MonoBehaviour
{
    [Header("Level")]
    public LevelConfig level;

    [Header("Prefab")]
    public CarView carPrefab;

    [Header("Board")]
    public float cellSize = 1f;
    public Vector2 boardOrigin = new Vector2(-3f, -3f);

    [Header("Exit")]
    public Vector2 targetExitWorldPosition = new Vector2(3f, -0.5f);

    [Header("UI")]
    public TMP_Text moveText;
    public GameObject winPanel;

    private readonly List<CarView> cars = new List<CarView>();
    private readonly Stack<MoveRecord> moveHistory = new Stack<MoveRecord>();

    private int moveCount = 0;

    private class MoveRecord
    {
        public CarView car;
        public int oldX;
        public int oldY;
        public int newX;
        public int newY;
    }

    public float CellSize => cellSize;
    public int Width => level.width;
    public int Height => level.height;

    private void Start()
    {
        LoadLevel(level);
    }

    public void LoadLevel(LevelConfig levelData)
    {
        level = levelData;

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        cars.Clear();
        moveHistory.Clear();
        moveCount = 0;
        UpdateMoveText();

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        foreach (CarConfig config in level.cars)
        {
            CarView car = Instantiate(carPrefab, transform);
            car.Init(this, config);
            cars.Add(car);
        }
    }

    public Vector3 GridToWorld(int x, int y, CarView car)
    {
        float widthInCells = car.Orientation == VehicleOrientation.Horizontal
            ? car.Length
            : 1;

        float heightInCells = car.Orientation == VehicleOrientation.Vertical
            ? car.Length
            : 1;

        float worldX = boardOrigin.x + (x + widthInCells * 0.5f) * cellSize;
        float worldY = boardOrigin.y + (y + heightInCells * 0.5f) * cellSize;

        return new Vector3(worldX, worldY, 0f);
    }

    public int GetAllowedDelta(
    CarView movingCar,
    int startX,
    int startY,
    int desiredDelta
)
    {
        if (desiredDelta == 0)
        {
            return 0;
        }

        int step = desiredDelta > 0 ? 1 : -1;
        int allowedDelta = 0;

        for (int d = step; Mathf.Abs(d) <= Mathf.Abs(desiredDelta); d += step)
        {
            int checkX = startX;
            int checkY = startY;

            if (movingCar.Orientation == VehicleOrientation.Horizontal)
            {
                checkY = startY;

                if (step > 0)
                {
                    checkX = startX + movingCar.Length - 1 + d;
                }
                else
                {
                    checkX = startX + d;
                }
            }
            else
            {
                checkX = startX;

                if (step > 0)
                {
                    checkY = startY + movingCar.Length - 1 + d;
                }
                else
                {
                    checkY = startY + d;
                }
            }

            if (!CanMoveIntoCell(movingCar, checkX, checkY))
            {
                break;
            }

            allowedDelta = d;
        }

        return allowedDelta;
    }

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    private bool CanMoveIntoCell(CarView movingCar, int x, int y)
    {
        if (IsInsideBoard(x, y))
        {
            return !IsOccupiedByOther(x, y, movingCar);
        }

        bool isExitCell =
            movingCar.IsTarget &&
            movingCar.Orientation == VehicleOrientation.Horizontal &&
            y == level.exitRow &&
            x == Width;

        return isExitCell;
    }

    private bool IsOccupiedByOther(int x, int y, CarView movingCar)
    {
        foreach (CarView car in cars)
        {
            if (car == movingCar)
            {
                continue;
            }

            if (car.Occupies(x, y))
            {
                return true;
            }
        }

        return false;
    }

    public void CommitMove(CarView car, int newX, int newY)
    {
        bool moved = car.X != newX || car.Y != newY;

        int oldX = car.X;
        int oldY = car.Y;

        car.SetGridPosition(newX, newY);

        bool targetReachedExit =
            car.IsTarget &&
            car.Orientation == VehicleOrientation.Horizontal &&
            car.Y == level.exitRow &&
            car.X + car.Length >= Width;

        if (targetReachedExit)
        {
            float carHalfWidth = car.Length * cellSize * 0.5f;

            car.transform.position = new Vector3(
                targetExitWorldPosition.x - carHalfWidth,
                targetExitWorldPosition.y,
                0f
            );
        }

        if (moved)
        {
            moveHistory.Push(new MoveRecord
            {
                car = car,
                oldX = oldX,
                oldY = oldY,
                newX = newX,
                newY = newY
            });

            moveCount++;
            UpdateMoveText();
            Debug.Log("Moves: " + moveCount);
        }

        CheckWin();
    }

    private void CheckWin()
    {
        foreach (CarView car in cars)
        {
            if (!car.IsTarget)
            {
                continue;
            }

            bool reachedExit =
                car.Orientation == VehicleOrientation.Horizontal &&
                car.Y == level.exitRow &&
                car.X + car.Length >= Width;

            if (reachedExit)
            {
                Debug.Log("You Win!");

                if (winPanel != null)
                {
                    winPanel.SetActive(true);
                }
            }
        }
    }
    private void UpdateMoveText()
    {
        if (moveText != null)
        {
            moveText.text = "Moves: " + moveCount;
        }
    }

    public void RestartLevel()
    {
        LoadLevel(level);
    }
    public float GetMaxDragDistanceToTargetExit(CarView car, Vector3 startWorldPosition)
    {
        if (!car.IsTarget)
        {
            return float.PositiveInfinity;
        }

        if (car.Orientation != VehicleOrientation.Horizontal)
        {
            return float.PositiveInfinity;
        }

        if (car.Y != level.exitRow)
        {
            return float.PositiveInfinity;
        }

        float carHalfWidth = car.Length * cellSize * 0.5f;
        float maxCenterX = targetExitWorldPosition.x - carHalfWidth;

        return Mathf.Max(0f, maxCenterX - startWorldPosition.x);
    }

    public void UndoLastMove()
    {
        if (moveHistory.Count == 0)
        {
            return;
        }

        MoveRecord lastMove = moveHistory.Pop();

        lastMove.car.SetGridPosition(lastMove.oldX, lastMove.oldY);

        moveCount = Mathf.Max(0, moveCount - 1);
        UpdateMoveText();

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        Debug.Log("Undo move. Moves: " + moveCount);
    }
}