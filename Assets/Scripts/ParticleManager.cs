using ServiceLocator;
using UnityEngine;

public class ParticleManager : IService
{
    [SerializeField] private ParticleSystem redCubeExplosion;
    [SerializeField] private ParticleSystem greenCubeExplosion;
    [SerializeField] private ParticleSystem blueCubeExplosion;
    [SerializeField] private ParticleSystem yellowCubeExplosion;
    [SerializeField] private ParticleSystem boxExplosion;
    [SerializeField] private ParticleSystem stoneExplosion;
    [SerializeField] private ParticleSystem vaseExplosion;
    [SerializeField] private ParticleSystem tntWaveExplosion;
    [SerializeField] private ParticleSystem tntExplosion;
    [SerializeField] private ParticleSystem blasterExplosion;
    
    public void PlayCubeExplosion(CubeType cubeType, Vector3 position)
    {
        var particleSystemEmitParams = new ParticleSystem.EmitParams()
        {
            position = position,
            applyShapeToPosition = true
        };
        
        var randomCount = Random.Range(0, 5);
        
        if (cubeType == CubeType.Red)
        {
            redCubeExplosion.Emit(particleSystemEmitParams, randomCount);
        }
        else if (cubeType == CubeType.Green)
        {
            greenCubeExplosion.Emit(particleSystemEmitParams, randomCount);
        }
        else if (cubeType == CubeType.Blue)
        {
            blueCubeExplosion.Emit(particleSystemEmitParams, randomCount);
        }
        else if (cubeType == CubeType.Yellow)
        {
            yellowCubeExplosion.Emit(particleSystemEmitParams, randomCount);
        }
    }
    
    public void PlayObstacleExplosion(ObstacleType obstacleType, Vector3 position)
    {
        var particleSystemEmitParams = new ParticleSystem.EmitParams()
        {
            position = position,
            applyShapeToPosition = true
        };
        
        var randomCount = Random.Range(0, 5);
        
        if (obstacleType == ObstacleType.Box)
        {
            boxExplosion.Emit(particleSystemEmitParams, randomCount);
        }
        else if (obstacleType == ObstacleType.Stone)
        {
            stoneExplosion.Emit(particleSystemEmitParams, randomCount);
        }
        else if (obstacleType == ObstacleType.Vase)
        {
            vaseExplosion.Emit(particleSystemEmitParams, randomCount);
        }
    }
    
    public void PlayTntExplosion(Vector3 position)
    {
        var particleSystemEmitParams = new ParticleSystem.EmitParams()
        {
            position = position,
            applyShapeToPosition = true
        };
        
        tntWaveExplosion.Emit(particleSystemEmitParams, 1);
        tntExplosion.Emit(particleSystemEmitParams, 1);
    }

    public void PlayBlasterExplosion(Vector3 position)
    {
        var particleSystemEmitParams = new ParticleSystem.EmitParams()
        {
            position = position,
            applyShapeToPosition = true
        };

        blasterExplosion.Emit(particleSystemEmitParams, 1);
    } 
}