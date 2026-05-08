using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Level")]
    public LevelConfig level;

    [Header("Prefab")]
    public CarView carPrefab;

    [Header("Board")]
    public float cellSize = 1f;
    public Vector2 boardOrigin = new Vector2(-3f, -3f);

    private readonly List<CarView> cars = new List<CarView>();
    private int moveCount = 0;

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
        moveCount = 0;

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
                if (step > 0)
                {
                    checkX = startX + movingCar.Length - 1 + d;
                }
                else
                {
                    checkX = startX + d;
                }

                checkY = startY;
            }
            else
            {
                if (step > 0)
                {
                    checkY = startY + movingCar.Length - 1 + d;
                }
                else
                {
                    checkY = startY + d;
                }

                checkX = startX;
            }

            if (!IsInsideBoard(checkX, checkY))
            {
                break;
            }

            if (IsOccupiedByOther(checkX, checkY, movingCar))
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

        car.SetGridPosition(newX, newY);

        if (moved)
        {
            moveCount++;
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
            }
        }
    }
}