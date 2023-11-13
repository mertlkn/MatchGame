using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LevelTargetItem : MonoBehaviour
    {
        [SerializeField] private Image targetImage;
        [SerializeField] private TextMeshProUGUI targetCountText;
        [SerializeField] private GameObject tickImage;

        private ObstacleType _obstacleType;
        public ObstacleType ObstacleType => _obstacleType;
        
        public void SetTarget(ObstacleType type, int targetCount)
        {
            _obstacleType = type;
            targetImage.sprite = BoardUtils.GetObstacleScriptableObject(type).obstacleSprites[^1];
            targetCountText.text = $"{targetCount}";
            tickImage.SetActive(false);
        }
        
        public void SetTargetCount(int targetCount)
        {
            if (targetCount == 0)
            {
                tickImage.SetActive(true);
                targetCountText.text = "";
            }
            else
            {
                targetCountText.text = $"{targetCount}";
            }
        }
    }
}