using System;
using System.Collections.Generic;
using DG.Tweening;
using Pools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Items
{
    public enum CellState {
        Stationary,
        Exploding,
    }
    
    public class Cell : MonoBehaviour, IPointerClickHandler
    {
        private Vector2Int _gridPosition;
    
        private GameItem _gameItem;
        private ItemType _itemType = ItemType.Empty;
        private Cube _cube;
        private Obstacle _obstacle;
        private Blaster _blaster;
        private Tnt _tnt;
        private int _chainedIndex;
    
        private static float DurationForShiftingEachCell = 0.35f;
    
        public GameItem GameItem => _gameItem;
        public CellState CellState { get; set; }
        public Vector2Int GridPosition => _gridPosition;
        public bool IsCube => _itemType == ItemType.Cube;
        public bool IsObstacle => _itemType == ItemType.Obstacle;
        public bool IsBlaster => _itemType == ItemType.Blaster;
        public bool IsTnt => _itemType == ItemType.Tnt;
        public bool IsOccupied => _itemType != ItemType.Empty;
        public Cube Cube => _cube;
        public Obstacle Obstacle => _obstacle;
        public Blaster Blaster => _blaster;
        public Tnt Tnt => _tnt;

        public CubeType CubeType => _cube.CubeType;
        public ObstacleType ObstacleType => _obstacle.ObstacleType;
        public BlasterType BlasterType => _blaster.BlasterType;
    
        public event Action<Cell> OnCellClicked;
        public event Action<CubeType, DamageType, Vector2Int, int, List<Cell>> OnCubeTookDamage;
        public event Action<ObstacleType, Vector2Int, int, bool> OnObstacleTookDamage;
        public event Action<DamageType, Vector2Int, int, List<Cell>> OnTntTookDamage;
        public event Action<BlasterType, DamageType, Vector2Int, int, List<Cell>> OnBlasterTookDamage;

        protected LevelPoolManager levelPoolManager;

        private void Awake()
        {
            levelPoolManager = ServiceLocator.ServiceLocator.Instance.GetService<LevelPoolManager>();
        }

        public void InitializeCell(Vector2Int gridPosition)
        {
            _gridPosition = gridPosition;
        }

        public void InitializeGameItem(GameItem gameItem)
        {
            _chainedIndex = 0;
            gameItem.transform.position = transform.position;
            SetGameItem(gameItem);
        }

        public void SetGameItem(GameItem gameItem)
        {
            gameItem.transform.SetParent(transform);
            switch (gameItem)
            {
                case Cube cube:
                    _itemType = ItemType.Cube;
                    _cube = cube;
                    break;
                case Obstacle obstacle:
                    _itemType = ItemType.Obstacle;
                    _obstacle = obstacle;
                    break;
                case Tnt tnt:
                    _itemType = ItemType.Tnt;
                    _tnt = tnt;
                    break;
                case Blaster blaster:
                    _itemType = ItemType.Blaster;
                    _blaster = blaster;
                    break;
            }
        
            _gameItem = gameItem;
            _gameItem.SpriteRenderer.sortingOrder = _gridPosition.y;
        }
    
        public void EmptyCell()
        {
            _itemType = ItemType.Empty;
        
            _cube = null;
            _obstacle = null;
            _blaster = null;
            _tnt = null;
            _gameItem = null;
        }

        public void DestroyCell()
        {
            if (IsOccupied)
            {
                _gameItem.Destroy();
            }
        
            levelPoolManager.ReleaseCell(this);
        }

        public void PrepareTakeDamage(DamageType damageType, int turnId)
        {
            if (CellState != CellState.Stationary)
            {
                return;
            }

            if (!IsOccupied)
            {
                return;
            }
        
            if (!_gameItem.CanTakeDamage(damageType))
            {
                return;
            }
        
            CellState = CellState.Exploding;
            _chainedIndex = turnId;
        }
    
        public void TakeDamage(DamageType damageType, int turnId, List<Cell> affectedCells = null)
        {
            if (CellState != CellState.Exploding)
            {
                return;
            }
        
            if (!IsOccupied)
            {
                CellState = CellState.Stationary;
                return;
            }
            else if (IsCube)
            {
                var cubeType = _cube.CubeType;
                _cube.TakeDamage(damageType);
            
                if (_cube.Health <= 0) EmptyCell();
            
                CellState = CellState.Stationary;
                OnCubeTookDamage?.Invoke(cubeType, damageType, GridPosition, turnId, affectedCells);
            }
            else if (IsObstacle)
            {
                var obstacleType = ObstacleType;
                _obstacle.TakeDamage(damageType);
                var obstacleHealth = _obstacle.Health;
            
                if (obstacleHealth <= 0) EmptyCell();
            
                CellState = CellState.Stationary;
                OnObstacleTookDamage?.Invoke(obstacleType, GridPosition, turnId, obstacleHealth <= 0);
            }
            else if (IsTnt)
            {
                if (damageType == DamageType.PlayerInput)
                {
                    TntTakeDamage();
                }
                else if (damageType == DamageType.Tnt)
                {
                    DOVirtual.DelayedCall(Tnt.NestedDelay * _chainedIndex, TntTakeDamage);
                }
                else if (damageType == DamageType.Match)
                {
                    _tnt.TakeDamage(damageType);
                
                    if (_tnt.Health <= 0) EmptyCell();
                
                    CellState = CellState.Stationary;
                    OnTntTookDamage?.Invoke(damageType, GridPosition, turnId, affectedCells);
                }

                void TntTakeDamage()
                {
                    _tnt.TakeDamage(damageType);
                
                    if (_tnt.Health <= 0) EmptyCell();
                
                    CellState = CellState.Stationary;
                    OnTntTookDamage?.Invoke(damageType, GridPosition, turnId, affectedCells);
                }
            }
            else if (IsBlaster)
            {
                var blasterType = _blaster.BlasterType;
                _blaster.TakeDamage(damageType);
                    
                if (_blaster.Health <= 0) EmptyCell();

                CellState = CellState.Stationary;
                OnBlasterTookDamage?.Invoke(blasterType, damageType, GridPosition, turnId, affectedCells);
            }
        }

        public void Shift(Cell targetCell)
        {
            targetCell.SetGameItem(_gameItem);
            EmptyCell();
        }

        public bool CanShift()
        {
            if (!IsOccupied) return false;
            if (CellState != CellState.Stationary) return false;
            return _gameItem.CanShift;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnCellClicked?.Invoke(this);
        }

        public bool ContainsSameType(Cell otherCell)
        {
            if (!IsOccupied || !otherCell.IsOccupied) return false;
        
            if (IsCube && otherCell.IsCube)
            {
                return CubeType == otherCell.CubeType;
            }
            if (IsObstacle && otherCell.IsObstacle)
            {
                return ObstacleType == otherCell.ObstacleType;
            }
            if (IsTnt && otherCell.IsTnt)
            {
                return true;
            }
        
            return false;
        }

        public void Shake()
        {
            if (!IsOccupied) return;
        
            _gameItem.Shake();
        }
    }
}