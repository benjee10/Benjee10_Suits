using KSP.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WearableProps.PropComponents;

namespace WearableProps.Components
{
    public class PropComponent : MonoBehaviour
    {
        public KerbalEVA kerbal;
        public ModuleWearableProp module;

        public bool isHelmetAttachment;
        public bool isEquiped;
        public List<Light> lights;

        public void Start()
        {
            LightComponent lightComponent = gameObject.AddComponent<LightComponent>();
            lightComponent.kerbal = this.kerbal;
            lightComponent.lights = lights;
            lightComponent.isHelmet = false;
            
            GameEvents.OnHelmetChanged.Add(OnHelmetChanged);
        }

        private void OnHelmetChanged(KerbalEVA kerbal, bool helmet, bool neckring)
        {
            if (this.kerbal != kerbal)
                return;

            if (!isHelmetAttachment)
                return;

            if (!isEquiped)
                return;

            gameObject.SetActive(helmet);
        }

        public void OnDestroy()
        {
            GameEvents.OnHelmetChanged.Remove(OnHelmetChanged);
        }      
    }
}
