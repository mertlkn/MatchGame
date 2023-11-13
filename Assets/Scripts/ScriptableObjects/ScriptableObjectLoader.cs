using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjects
{
    public static class ScriptableObjectLoader
    {
        private static Dictionary<CubeType, CubeScriptableObject> _cubeScriptableObjects;
        private static Dictionary<ObstacleType, ObstacleScriptableObject> _obstacleScriptableObjects;
        private static TntScriptableObject _tntScriptableObject;
        private static BlasterScriptableObject _blasterScriptableObject;
        private static BoardConstants _boardConstants;
        
        public static Dictionary<CubeType, CubeScriptableObject> CubeScriptableObjects
        {
            get
            {
                if (_cubeScriptableObjects == null)
                {
                    _cubeScriptableObjects = new Dictionary<CubeType, CubeScriptableObject>();
                    LoadScriptableObjects();
                }

                return _cubeScriptableObjects;
            }
        }
        
        public static Dictionary<ObstacleType, ObstacleScriptableObject> ObstacleScriptableObjects
        {
            get
            {
                if (_obstacleScriptableObjects == null)
                {
                    _obstacleScriptableObjects = new Dictionary<ObstacleType, ObstacleScriptableObject>();
                    LoadScriptableObjects();
                }

                return _obstacleScriptableObjects;
            }
        }
        
        public static TntScriptableObject TntScriptableObject
        {
            get
            {
                if (_tntScriptableObject == null)
                {
                    _tntScriptableObject = Resources.Load<TntScriptableObject>("Tnt");
                }

                return _tntScriptableObject;
            }
        }

        public static BlasterScriptableObject BlasterScriptableObject
        {
            get
            {
                if (_blasterScriptableObject == null)
                {
                    _blasterScriptableObject = Resources.Load<BlasterScriptableObject>("Blaster");
                }

                return _blasterScriptableObject;
            }
        }
        
        public static BoardConstants BoardConstants
        {
            get
            {
                if (_boardConstants == null)
                {
                    _boardConstants = Resources.Load<BoardConstants>("BoardConstants");
                }

                return _boardConstants;
            }
        }
        
        
        private static void LoadScriptableObjects()
        {
            _cubeScriptableObjects = new Dictionary<CubeType, CubeScriptableObject>();
            _obstacleScriptableObjects = new Dictionary<ObstacleType, ObstacleScriptableObject>();
            
            var cubeScriptableObjects = Resources.LoadAll<CubeScriptableObject>("Cubes");
            foreach (var cubeScriptableObject in cubeScriptableObjects)
            {
                _cubeScriptableObjects.Add(cubeScriptableObject.cubeType, cubeScriptableObject);
            }
        
            var obstacleScriptableObjects = Resources.LoadAll<ObstacleScriptableObject>("Obstacles");
            foreach (var obstacleScriptableObject in obstacleScriptableObjects)
            {
                _obstacleScriptableObjects.Add(obstacleScriptableObject.obstacleType, obstacleScriptableObject);
            }
            
            _tntScriptableObject = Resources.Load<TntScriptableObject>("Tnt");
            
            _boardConstants = Resources.Load<BoardConstants>("BoardConstants");
        }
    }
}