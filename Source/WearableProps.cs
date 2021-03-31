using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WearableProps
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class WearableProps : MonoBehaviour
    {
        public const string DISPLAYNAME = "WearableProps";
       
        public void Awake()
        {
            if (Versioning.version_major == 1 && Versioning.version_minor < 11)
            {
                Debug.LogWarning($"[{DISPLAYNAME}] Inventory Props disabled, please use 1.11 or above!");
            }

            if (Versioning.version_major == 1 && Versioning.version_minor < 10)
            {
                Debug.LogWarning($"[{DISPLAYNAME}] Custom IVA Suits disabled, please use 1.10 or above!");
            }

            DestroyImmediate(this);
        }
    }
}
