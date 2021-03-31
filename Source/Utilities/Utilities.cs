using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WearableProps.Utils
{
    public static class Utilities
    {
        public static Transform GetTransformAlias(KerbalEVA kerbal, string name)
        {
            Transform transform = kerbal.part.transform;
            switch(name)
            {
                case "helmet":
                    transform = kerbal.part.GetComponentsInChildren<Transform>(true).Where(t => t.name == "bn_helmet01").FirstOrDefault();
                    break;
                default:
                    transform = kerbal.part.GetComponentsInChildren<Transform>(true).Where(t => t.name == name).FirstOrDefault();
                    break;
            }

            return transform;
        } 

        public static bool CheckForLights(GameObject gameObject, out List<Light> lights)
        {
            lights = gameObject.GetComponentsInChildren<Light>(true).ToList();
           
            if (lights.Count == 0)
                return false;
            else
                return true;
        }
    }
}
