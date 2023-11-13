using System;
using System.Collections.Generic;
using System.Linq;
using Items;
using Pools;
using ServiceLocator;
using UnityEngine;

public class BoardManager : IService
{
    [SerializeField] private Transform boardParent;
    [SerializeField] private SpriteRenderer boardBackgroundSpriteRenderer;
    [SerializeField] private Camera boardCamera;

    private int _chainedIndex;
    private Cell[,] _cells;
    public event Action OnTargetUpdated;
    public event Action OnMove;
    
    private int _movesLeft;
    public int MovesLeft => _movesLeft;
    
    private Dictionary<ObstacleType, int> _currentLevelTarget;
    public Dictionary<ObstacleType, int> CurrentLevelTarget => _currentLevelTarget;
    
    private LevelPoolManager _levelPoolManager;
    private LevelManager _levelManager;

    private void Awake()
    {
        _levelPoolManager = ServiceLocator.ServiceLocator.Instance.GetService<LevelPoolManager>();
        _levelManager = ServiceLocator.ServiceLocator.Instance.GetService<LevelManager>();
    }

    public void CreateBoard(LevelInfo info)
    {
        _chainedIndex = 0;
        
        _cells = new Cell[info.grid_width, info.grid_height];
        _currentLevelTarget = new Dictionary<ObstacleType, int>();
        
        boardBackgroundSpriteRenderer.transform.localPosition = new Vector3(info.grid_width / 2f - 0.5f, info.grid_height / 2f - 0.5f, 0);
        boardBackgroundSpriteRenderer.size = new Vector2(info.grid_width + 0.5f, info.grid_height + 0.5f);
        
        for (var y = 0; y < info.grid_height; y++)
        {
            for (var x = 0; x < info.grid_width; x++)
            {
                _cells[x, y] = _levelPoolManager.GetCell();
                _cells[x, y].transform.SetParent(boardParent);
                _cells[x, y].transform.localPosition = new Vector3(x, y, 0);
                _cells[x, y].name = $"Cell ({x}, {y})";

                var gameItemString = info.grid[y * info.grid_width + x];
                var gameItem = BoardUtils.GetGameItem(_levelPoolManager, gameItemString);
                
                if (gameItem == null) continue;
                
                _cells[x, y].InitializeCell(new Vector2Int(x, y));
                _cells[x, y].InitializeGameItem(gameItem);
                _cells[x, y].OnCellClicked += DamageByPlayerInput;
                _cells[x, y].OnCubeTookDamage += OnCubeTookDamage;
                _cells[x, y].OnObstacleTookDamage += OnObstacleTookDamage;
                _cells[x, y].OnBlasterTookDamage += OnBlasterTookDamage;
                _cells[x, y].OnTntTookDamage += OnTntTookDamage;

                if (gameItem is Obstacle obstacle)
                {
                    if (_currentLevelTarget.ContainsKey(obstacle.ObstacleType))
                    {
                        _currentLevelTarget[obstacle.ObstacleType]++;
                    }
                    else
                    {
                        _currentLevelTarget.Add(obstacle.ObstacleType, 1);
                    }
                }
            }
        }
        
        DecideCubeSprites();
        
        _movesLeft = info.move_count;
        
        OnTargetUpdated?.Invoke();
        
        var max = Mathf.Max(info.grid_width, info.grid_height);
        boardCamera.orthographicSize = (max / 2f + 1f) / (9f / 16);
        boardCamera.transform.position = transform.position + new Vector3(info.grid_width / 2f - 0.5f, info.grid_height / 2f + 2.5f, -10);
    }

    private void OnCubeTookDamage(CubeType cubeType, DamageType damageType, Vector2Int gridPosition, int turnId,
        List<Cell> affectedCells)
    {
        if (affectedCells == null) affectedCells = new List<Cell>();
        
        if (damageType == DamageType.PlayerInput)
        {
            if (affectedCells.Count + 1 >= BoardUtils.GetMinimumRequiredAmountToCreateBlaster())
            {
                var blaster = _levelPoolManager.GetBlaster(BoardUtils.GetBlasterType(cubeType));
                _cells[gridPosition.x, gridPosition.y].InitializeGameItem(blaster);
            }
            else if (affectedCells.Count + 1 >= BoardUtils.GetMinimumRequiredAmountToCreateTnt())
            {
                var tnt = _levelPoolManager.GetTnt();
                _cells[gridPosition.x, gridPosition.y].InitializeGameItem(tnt);
            }
            
            var matchedCubeType = affectedCells[0].CubeType;
            
            var nearbyDamageTakenCells = new List<Cell>();

            var matchedCubes = new List<Vector2Int>();
            matchedCubes.AddRange(affectedCells.Select(cell => cell.GridPosition));
            matchedCubes.Add(gridPosition);
            
            nearbyDamageTakenCells.AddRange(GetNeighboursForNearbyDamage(matchedCubes, matchedCubeType));

            foreach (var neighbour in affectedCells)
            {
                neighbour.PrepareTakeDamage(DamageType.Match, turnId);
            }

            foreach (var cell in nearbyDamageTakenCells)
            {
                cell.PrepareTakeDamage(DamageType.Nearby, turnId);
            }
                
            foreach (var neighbour in affectedCells)
            {
                neighbour.TakeDamage(DamageType.Match, turnId);
            }
            
            foreach (var cell in nearbyDamageTakenCells)
            {
                cell.TakeDamage(DamageType.Nearby, turnId);
            }
                
            Shift(gridPosition.x);
        }

        else if (damageType == DamageType.Match)
        {
            Shift(gridPosition.x);
        }

        else if (damageType == DamageType.Tnt)
        {
            Shift(gridPosition.x);
        }
        
        else if (damageType == DamageType.Blaster)
        {
            Shift(gridPosition.x);
        }
    }

    private void OnObstacleTookDamage(ObstacleType type, Vector2Int gridPosition, int turnId, bool destroyed)
    {
        Shift(gridPosition.x);
        
        if (!CurrentLevelTarget.ContainsKey(type)) return;

        if (!destroyed) return;
        
        CurrentLevelTarget[type]--;
        
        OnTargetUpdated?.Invoke();
        
        if (_currentLevelTarget.All(targetKvp => targetKvp.Value <= 0))
        {
            _levelManager.EndGame(true);
        }
    }

    private void OnBlasterTookDamage(BlasterType blasterType, DamageType damageType, Vector2Int gridPosition,
        int turnId,
        List<Cell> affectedCells)
    {
        if (affectedCells == null) affectedCells = new List<Cell>();

        if (damageType == DamageType.PlayerInput)
        {
            foreach (var explosionCell in affectedCells)
            {
                explosionCell.PrepareTakeDamage(DamageType.Blaster, turnId);
            }

            foreach (var explosionCell in affectedCells)
            {
                explosionCell.TakeDamage(DamageType.Blaster, turnId);
            }
            
            Shift(gridPosition.x);
        }
        else if (damageType == DamageType.Tnt)
        {
            var explosionCells = GetCellsExplodedByBlaster(blasterType);
            
            foreach (var explosionCell in explosionCells)
            {
                explosionCell.PrepareTakeDamage(DamageType.Blaster, turnId);
            }

            foreach (var explosionCell in explosionCells)
            {
                explosionCell.TakeDamage(DamageType.Blaster, turnId);
            }
            
            Shift(gridPosition.x);
        }
    }

    private void OnTntTookDamage(DamageType damageType, Vector2Int gridPosition, int turnId, List<Cell> affectedCells)
    {
        if (affectedCells == null) affectedCells = new List<Cell>();
        
        if (damageType == DamageType.PlayerInput)
        {
            foreach (var explosionCell in affectedCells)
            {
                explosionCell.PrepareTakeDamage(DamageType.Tnt, turnId);
            }
                
            foreach (var explosionCell in affectedCells)
            {
                explosionCell.TakeDamage(DamageType.Tnt, turnId);
            }
                
            Shift(gridPosition.x);
        }
            
        else if (damageType == DamageType.Tnt)
        {
            var explosionCells = GetCellsExplodedByTnt(gridPosition, false);
                
            foreach (var explosionCell in explosionCells)
            {
                explosionCell.PrepareTakeDamage(DamageType.Tnt, turnId);
            }
            
            foreach (var explosionCell in explosionCells)
            {
                explosionCell.TakeDamage(DamageType.Tnt, turnId);
            }
                
            Shift(gridPosition.x);
        }
        else if (damageType == DamageType.Match)
        {
            
        }
    }

    private void OnMoveByPlayer()
    {
        _movesLeft--;
        OnMove?.Invoke();
        
        if (_movesLeft <= 0)
        {
            if (_currentLevelTarget.Any(targetKvp => targetKvp.Value > 0))
            {
                _levelManager.EndGame(false);
                return;
            }
        }
        
        if (_currentLevelTarget.All(targetKvp => targetKvp.Value <= 0))
        {
            _levelManager.EndGame(true);
            return;
        }
    }
    
    public void DamageByPlayerInput(Cell cell)
    {
        _chainedIndex = 0;
        
        if (cell.CellState != CellState.Stationary) return;
        
        if (cell.IsCube)
        {
            var neighbours = GetSameTypeAdjacent(cell);

            if (neighbours.Count + 1 < BoardUtils.GetMinimumRequiredAmountToMatch())
            {
                return;
            }
            
            cell.PrepareTakeDamage(DamageType.PlayerInput, _chainedIndex);
            cell.TakeDamage(DamageType.PlayerInput, _chainedIndex, neighbours);
            
            OnMoveByPlayer();
        }
        else if (cell.IsTnt)
        {
            var neighbours = GetSameTypeAdjacent(cell);

            List<Cell> explosionCells;
            
            if (neighbours.Count == 0)
            {
                explosionCells = GetCellsExplodedByTnt(cell.GridPosition, false);
            }
            else
            {
                foreach (var neighbour in neighbours)
                {
                    neighbour.PrepareTakeDamage(DamageType.Match, _chainedIndex);
                    neighbour.TakeDamage(DamageType.Match, _chainedIndex);
                }
                
                explosionCells = GetCellsExplodedByTnt(cell.GridPosition, true);
            }
            
            var chainedTntCells = GetChainedTntCells(cell.GridPosition);
            
            foreach (var chainedTnt in chainedTntCells)
            {
                chainedTnt.Item1.PrepareTakeDamage(DamageType.Tnt, chainedTnt.Item2);
                if (!explosionCells.Contains(chainedTnt.Item1))
                {
                    explosionCells.Add(chainedTnt.Item1);
                }
            }
            
            cell.PrepareTakeDamage(DamageType.PlayerInput, _chainedIndex);
            cell.TakeDamage(DamageType.PlayerInput, _chainedIndex, explosionCells);
            
            OnMoveByPlayer();
        }
        else if (cell.IsObstacle)
        {
            cell.Shake();
        }
        else if (cell.IsBlaster)
        {
            var explosionCells = GetCellsExplodedByBlaster(cell.BlasterType);
            
            cell.PrepareTakeDamage(DamageType.PlayerInput, _chainedIndex);
            cell.TakeDamage(DamageType.PlayerInput, _chainedIndex, explosionCells);
            
            OnMoveByPlayer();
        }
    }

    public List<Cell> GetCellsExplodedByBlaster(BlasterType blasterType)
    {
        var explosionCells = new List<Cell>();

        foreach (var cell in _cells)
        {
            if (!cell.IsCube) continue;
            
            if (BoardUtils.GetBlasterType(cell.CubeType) != blasterType) continue;
            
            explosionCells.Add(cell);
        }

        return explosionCells;
    }
    
    public List<(Cell, int)> GetChainedTntCells(Vector2Int gridPosition)
    {
        var chainedTntCells = new List<(Cell, int)>();
        
        var queue = new List<(Cell,int)>();
        queue.Add((_cells[gridPosition.x, gridPosition.y], 0));
        
        while (queue.Count > 0)
        {
            var currentCell = queue[^1];
            queue.RemoveAt(queue.Count - 1);
            chainedTntCells.Add(currentCell);
            
            var neighbours = GetCellsExplodedByTnt(currentCell.Item1.GridPosition, false);
            foreach (var neighbour in neighbours)
            {
                if (neighbour.IsTnt && !chainedTntCells.Exists(tuple => tuple.Item1.Equals(neighbour)) && !queue.Exists(tuple => tuple.Item1.Equals(neighbour)))
                {
                    queue.Add((neighbour, currentCell.Item2 + 1));
                }
            }
        }

        return chainedTntCells;
    }
    
    public List<Cell> GetNeighboursForNearbyDamage(List<Vector2Int> gridPositions, CubeType cubeType)
    {
        var neighbours = new List<Cell>();
        
        foreach (var gridPosition in gridPositions)
        {
            var temp = GetNeighbours(gridPosition);
            foreach (var neighbour in temp)
            {
                if (!(neighbour.IsCube && neighbour.CubeType == cubeType) && !neighbours.Contains(neighbour))
                {
                    neighbours.Add(neighbour);
                }
            }
        }

        return neighbours;
    }

    public List<Cell> GetSameTypeAdjacent(Cell cell)
    {
        var adjacentCells = new List<Cell>();
        
        var queue = new Queue<Cell>();
        queue.Enqueue(cell);
        
        while (queue.Count > 0)
        {
            var currentCell = queue.Dequeue();
            adjacentCells.Add(currentCell);
            
            var neighbours = GetNeighbours(currentCell.GridPosition);
            foreach (var neighbour in neighbours)
            {
                if (cell.ContainsSameType(neighbour) && !adjacentCells.Contains(neighbour) && !queue.Contains(neighbour))
                {
                    queue.Enqueue(neighbour);
                }
            }
        }

        adjacentCells.Remove(cell);
        return adjacentCells;
    }
    
    private List<Cell> GetNeighbours(Vector2Int gridPosition)
    {
        var neighbours = new List<Cell>();
        
        var x = gridPosition.x;
        var y = gridPosition.y;
        
        if (x > 0)
        {
            neighbours.Add(_cells[x - 1, y]);
        }
        
        if (x < _cells.GetLength(0) - 1)
        {
            neighbours.Add(_cells[x + 1, y]);
        }
        
        if (y > 0)
        {
            neighbours.Add(_cells[x, y - 1]);
        }
        
        if (y < _cells.GetLength(1) - 1)
        {
            neighbours.Add(_cells[x, y + 1]);
        }
        
        return neighbours;
    }
    
    private List<Cell> GetCellsExplodedByTnt(Vector2Int gridPosition, bool combo)
    {
        var explodingCells = new List<Cell>();
        var area = BoardUtils.GetTntDamageArea(combo);
        var damageWidthToEachSide = area / 2;
        
        var minX = Mathf.Max(0, gridPosition.x - damageWidthToEachSide);
        var maxX = Mathf.Min(_cells.GetLength(0) - 1, gridPosition.x + damageWidthToEachSide);
        var minY = Mathf.Max(0, gridPosition.y - damageWidthToEachSide);
        var maxY = Mathf.Min(_cells.GetLength(1) - 1, gridPosition.y + damageWidthToEachSide);
        
        for (var x = minX; x <= maxX; x++)
        {
            for (var y = minY; y <= maxY; y++)
            {
                var currentCell = _cells[x, y];
                
                if (currentCell.GridPosition.Equals(gridPosition)) continue;
                if (!currentCell.IsOccupied) continue;
                if (currentCell.CellState != CellState.Stationary) continue;
                explodingCells.Add(currentCell);
            }
        }
        return explodingCells;
    }

    public void Shift(int x)
    {
        for (var y = 1; y < _cells.GetLength(1); y++)
        {
            var targetY = y - 1;
            var currentCell = _cells[x, y];
            
            if (!currentCell.CanShift()) continue;

            var targetCell = GetTargetEmptyCellForShift(x, ref targetY);

            if (targetCell == null) continue;
            
            currentCell.Shift(targetCell);
        }
        
        var emptyCellCount = 0;
        
        for (var y = 0; y < _cells.GetLength(1); y++)
        {
            if (_cells[x, y].IsOccupied) continue;
            emptyCellCount++;
        }
        
        for (var i = 0; i < emptyCellCount; i++)
        {
            var targetY = _cells.GetLength(dimension: 1) - 1;
            
            var emptyCell = GetTargetEmptyCellForShift(x, ref targetY);
            
            if (emptyCell == null) continue;
        
            var cube = BoardUtils.GetRandomCube(_levelPoolManager);
            emptyCell.SetGameItem(cube);
            var position = emptyCell.transform.position;

            float y = _cells.GetLength(dimension: 1) + i;

            if (emptyCell.GridPosition.y > 0)
            {
                y = Math.Max(y, _cells[emptyCell.GridPosition.x, emptyCell.GridPosition.y - 1].GameItem.transform.position.y + 1);
            }
            
            cube.transform.position = new Vector3(position.x, y, position.z);
        }
        
        if (!IsColumnStabilized(x)) return;

        DecideCubeSprites();
    }

    private bool IsColumnStabilized(int x)
    {
        for (var y = 0; y < _cells.GetLength(1); y++)
        {
            var currentCell = _cells[x, y];
            if (currentCell.CellState != CellState.Stationary) return false;
        }
        
        return true;
    }

    private HashSet<Vector2Int> _adjacentsCountedCubePositions = new();
    private void DecideCubeSprites()
    {
        _adjacentsCountedCubePositions.Clear();
        for (var x = 0; x < _cells.GetLength(0); x++)
        {
            for (var y = 0; y < _cells.GetLength(1); y++)
            {
                if (!_cells[x, y].IsCube) continue;
                
                if (_cells[x, y].CellState != CellState.Stationary) continue;
                
                if (_adjacentsCountedCubePositions.Contains(_cells[x, y].GridPosition)) continue;
                
                var adjacentCells = GetSameTypeAdjacent(_cells[x, y]);
                
                if (adjacentCells.Exists(cell => cell.CellState != CellState.Stationary)) continue;
                
                _adjacentsCountedCubePositions.Add(_cells[x, y].GridPosition);
                _adjacentsCountedCubePositions.UnionWith(adjacentCells.Select(cell => cell.GridPosition));

                if (adjacentCells.Count + 1 < BoardUtils.GetMinimumRequiredAmountToCreateTnt())
                {
                    _cells[x, y].Cube.UpdateVisuals();

                    foreach (var cell in adjacentCells)
                    {
                        cell.Cube.UpdateVisuals();
                    }
                }
                else
                {
                    _cells[x, y].Cube.SetTntCreateSprite();
                    
                    foreach (var cell in adjacentCells)
                    {
                        cell.Cube.SetTntCreateSprite();
                    }
                }
            }
        }
    }

    private Cell GetTargetEmptyCellForShift(int x, ref int targetY)
    {
        Cell targetCell = null;
        while (targetY >= 0)
        {
            var tempCell = _cells[x, targetY];

            if (tempCell.IsOccupied || tempCell.CellState != CellState.Stationary)
            {
                break;
            }

            targetCell = tempCell;
            targetY--;
        }
        
        targetY++;

        return targetCell;
    }
}