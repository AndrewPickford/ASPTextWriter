using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ASP
{
    public class SerializableConfigNode : ScriptableObject
    {
        [SerializeField]
        public ConfigNode node { get; set; }
    }
}
