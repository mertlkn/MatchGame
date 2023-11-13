using System.Collections.Generic;
using Area;
using Items;
using ScriptableObjects;
using ServiceLocator;
using UnityEngine;
using UnityEngine.Pool;

namespace Pools
{
    public class LevelPoolManager : IService
    {
        [SerializeField] private Cube cubePrefab;
        [SerializeField] private Obstacle obstaclePrefab;
        [SerializeField] private Tnt tntPrefab;
        [SerializeField] private Blaster blasterPrefab;
        [SerializeField] private Cell cellPrefab;
    
        [SerializeField] private Transform poolParent;

        private IObjectPool<Cube> _cubePool;
        private IObjectPool<Obstacle> _obstaclePool;
        private IObjectPool<Tnt> _tntPool;
        private IObjectPool<Blaster> _blasterPool;
        private IObjectPool<Cell> _cellPool;

        protected void Awake()
        {
            _cubePool = new ObjectPool<Cube>(CreateCube, OnGetCube, OnReleaseCube);
            _obstaclePool =
                new ObjectPool<Obstacle>(CreateObstacle, OnGetObstacle, OnReleaseObstacle);
            _tntPool = new ObjectPool<Tnt>(CreateTnt, OnGetTnt, OnReleaseTnt);
            _blasterPool = new ObjectPool<Blaster>(CreateBlaster, OnGetBlaster, OnReleaseBlaster);
            _cellPool = new ObjectPool<Cell>(CreateCell, OnGetCell, OnReleaseCell);
        }

        private void OnDestroy()
        {
            _cubePool.Clear();
            _obstaclePool.Clear();
            _tntPool.Clear();
            _blasterPool.Clear();
            _cellPool.Clear();
        }

        public Cube GetCube(CubeType cubeType)
        {
            var cube = _cubePool.Get();
            
            var cubeScriptableObject = BoardUtils.GetCubeScriptableObject(cubeType);
            cube.Initialize(cubeType, cubeScriptableObject.cubeSprites,
                cubeScriptableObject.acceptableDamageTypes, cubeScriptableObject.tntSprite);
            return cube;
        }

        public void ReleaseCube(Cube cube)
        {
            cube.transform.SetParent(poolParent);
            _cubePool.Release(cube);
        }

        public Obstacle GetObstacle(ObstacleType obstacleType)
        {
            var obstacle = _obstaclePool.Get();
            
            var obstacleScriptableObject = BoardUtils.GetObstacleScriptableObject(obstacleType);
            obstacle.Initialize(obstacleType, obstacleScriptableObject.obstacleSprites,
                obstacleScriptableObject.acceptableDamageTypes,
                obstacleScriptableObject.health, obstacleScriptableObject.canShift);
            return obstacle;
        }

        public void ReleaseObstacle(Obstacle obstacle)
        {
            obstacle.transform.SetParent(poolParent);
            _obstaclePool.Release(obstacle);
        }

        public Tnt GetTnt()
        {
            var tnt = _tntPool.Get();
            
            var tntScriptableObject = BoardUtils.GetTntScriptableObject();
            tnt.Initialize(tntScriptableObject.tntSprites, tntScriptableObject.acceptableDamageTypes,
                tntScriptableObject.health, tntScriptableObject.nestedDelay);
            return tnt;
        }

        public void ReleaseTnt(Tnt tnt)
        {
            tnt.transform.SetParent(poolParent);
            _tntPool.Release(tnt);
        }

        public Blaster GetBlaster(BlasterType blasterType)
        {
            var blaster = _blasterPool.Get();

            var blasterScriptableObject = BoardUtils.GetBlasterScriptableObject();
            blaster.Initialize(blasterScriptableObject.blasterSprites, blasterScriptableObject.acceptableDamageTypes,
                blasterScriptableObject.health, blasterType);
            return blaster;
        }

        public void ReleaseBlaster(Blaster blaster)
        {
            blaster.transform.SetParent(poolParent);
            _blasterPool.Release(blaster);
        }

        public Cell GetCell()
        {
            return _cellPool.Get();
        }

        public void ReleaseCell(Cell cell)
        {
            _cellPool.Release(cell);
        }
        
        private void OnReleaseCube(Cube obj)
        {
            obj.KillTweens();
            obj.gameObject.SetActive(false);
        }

        private void OnGetCube(Cube obj)
        {
            obj.gameObject.SetActive(true);
        }

        private Cube CreateCube()
        {
            return Instantiate(cubePrefab);
        }

        private void OnReleaseObstacle(Obstacle obj)
        {
            obj.KillTweens();
            obj.gameObject.SetActive(false);
        }

        private void OnGetObstacle(Obstacle obj)
        {
            obj.gameObject.SetActive(true);
        }

        private Obstacle CreateObstacle()
        {
            return Instantiate(obstaclePrefab);
        }

        private void OnReleaseTnt(Tnt obj)
        {
            obj.KillTweens();
            obj.gameObject.SetActive(false);
        }

        private void OnGetTnt(Tnt obj)
        {
            obj.gameObject.SetActive(true);
        }

        private Tnt CreateTnt()
        {
            return Instantiate(tntPrefab);
        }

        private void OnReleaseBlaster(Blaster obj)
        {
            obj.KillTweens();
            obj.gameObject.SetActive(false);
        }

        private void OnGetBlaster(Blaster obj)
        {
            obj.gameObject.SetActive(true);
        }

        private Blaster CreateBlaster()
        {
            return Instantiate(blasterPrefab);
        }

        private void OnReleaseCell(Cell obj)
        {
            obj.gameObject.SetActive(false);
        }

        private void OnGetCell(Cell obj)
        {
            obj.gameObject.SetActive(true);
        }

        private Cell CreateCell()
        {
            return Instantiate(cellPrefab);
        }
    }
}