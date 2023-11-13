using System;
using UnityEngine;

namespace Area
{
    public enum TaskState
    {
        Locked,
        Unlocked,
        Completed
    }
    
    [Serializable]
    public class AreaTask
    {
        public int taskID;
        public int unlockLevel;
        public Sprite taskSprite;
        public Vector2 taskPosition;
    }
}