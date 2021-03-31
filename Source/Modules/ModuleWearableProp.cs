using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WearableProps
{
    public enum PropType
    {
        PROP,
        HELMET,
        HELMETPROP
    }

    public class ModuleWearableProp : PartModule
    {
        [KSPField]
        public string moduleId;

        [KSPField]
        public string attachTransform;

        [KSPField]
        public Vector3 positionOffset;

        [KSPField]
        public Vector3 rotationOffset;

        [KSPField]
        public string visorAnimationName;

        [KSPField]
        public float camOffset = 0f;

        [KSPField]
        public PropType propType = PropType.PROP;

        public List<string> lightNames = new List<string>();

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (node.HasNode("HEADLIGHT"))
            {
                ConfigNode configNode = node.GetNode("HEADLIGHT");
                lightNames = configNode.GetValues("lightName").ToList();
            }         
        }

        public override string GetInfo()
        {
            string str = string.Empty;

            switch(propType)
            {
                case PropType.HELMET:
                    str += $"To equip this item, put it into an inventory slot. This will replace the helmet.";
                    break;
                case PropType.HELMETPROP:
                    str += $"To equip this item, put it into an inventory slot.";
                    break;
                case PropType.PROP:
                    str += $"To equip this item, put it into an inventory slot.";
                    break;
            }

            return str;
        }

        public override string GetModuleDisplayName()
        {
            string str = $"Equipable Item";
            return str;
        } 
    }
}
