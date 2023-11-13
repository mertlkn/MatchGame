using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "BoardConstants", menuName = "ScriptableObjects/BoardConstants", order = 5)]
    public class BoardConstants : ScriptableObject
    {
        public float shiftDurationMultiplier;
        public int amountRequiredToCreateTnt;
        public int amountRequiredToCreateBlaster;
        public int amountRequiredToMatch;
        public AnimationCurve shiftAnimationCurve;
    }
}