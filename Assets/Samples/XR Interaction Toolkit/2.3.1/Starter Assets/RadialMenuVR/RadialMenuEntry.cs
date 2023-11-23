using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace VR
{
    [Serializable]
    public struct RadialMenuEntry
    {
        public string label;
        public Sprite icon;
        public UnityEvent uEvent;
    }
}
