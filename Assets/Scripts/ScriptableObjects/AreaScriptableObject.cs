using System.Collections.Generic;
using Area;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Area", menuName = "ScriptableObjects/AreaScriptableObject", order = 4)]
    public class AreaScriptableObject : ScriptableObject
    {
        public int areaID;
        public Sprite backgroundSprite;
        public List<AreaTask> tasks;
    }
}