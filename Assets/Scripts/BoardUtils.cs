using Items;
using Pools;
using ScriptableObjects;
using UnityEngine;

public static class BoardUtils
{
    public static GameItem GetGameItem(LevelPoolManager poolManager, string type)
    {
        return type switch
        {
            "r" => poolManager.GetCube(CubeType.Red),
            "g" => poolManager.GetCube(CubeType.Green),
            "b" => poolManager.GetCube(CubeType.Blue),
            "y" => poolManager.GetCube(CubeType.Yellow),
            "t" => poolManager.GetTnt(),
            "bo" => poolManager.GetObstacle(ObstacleType.Box),
            "s" => poolManager.GetObstacle(ObstacleType.Stone),
            "v" => poolManager.GetObstacle(ObstacleType.Vase),
            "rand" => GetRandomCube(poolManager),
            _ => null
        };
    }

    public static BlasterType GetBlasterType(CubeType cubeType)
    {
        return cubeType switch
        {
            CubeType.Red => BlasterType.Red,
            CubeType.Green => BlasterType.Green,
            CubeType.Blue => BlasterType.Blue,
            CubeType.Yellow => BlasterType.Yellow,
            _ => BlasterType.Red
        };
    }

    public static Cube GetRandomCube(LevelPoolManager poolManager)
    {
        return poolManager.GetCube((CubeType)Random.Range(0, 4));
    }
    
    public static CubeScriptableObject GetCubeScriptableObject(CubeType cubeType)
    {
        return ScriptableObjectLoader.CubeScriptableObjects[cubeType];
    }
    
    public static ObstacleScriptableObject GetObstacleScriptableObject(ObstacleType obstacleType)
    {
        return ScriptableObjectLoader.ObstacleScriptableObjects[obstacleType];
    }
    
    public static TntScriptableObject GetTntScriptableObject()
    {
        return ScriptableObjectLoader.TntScriptableObject;
    }

    public static int GetTntDamageArea(bool combo)
    {
        return combo
            ? ScriptableObjectLoader.TntScriptableObject.comboDamageArea
            : ScriptableObjectLoader.TntScriptableObject.damageArea;
    }
    
    public static int GetMinimumRequiredAmountToMatch()
    {
        return ScriptableObjectLoader.BoardConstants.amountRequiredToMatch;
    }
    
    public static int GetMinimumRequiredAmountToCreateTnt()
    {
        return ScriptableObjectLoader.BoardConstants.amountRequiredToCreateTnt;
    }
    
    public static float GetShiftDurationMultiplier()
    {
        return ScriptableObjectLoader.BoardConstants.shiftDurationMultiplier;
    }

    public static AnimationCurve GetShiftAnimationCurve()
    {
        return ScriptableObjectLoader.BoardConstants.shiftAnimationCurve;
    }

    public static BlasterScriptableObject GetBlasterScriptableObject()
    {
        return ScriptableObjectLoader.BlasterScriptableObject;
    }

    public static int GetMinimumRequiredAmountToCreateBlaster()
    {
        return ScriptableObjectLoader.BoardConstants.amountRequiredToCreateBlaster;
    }
}