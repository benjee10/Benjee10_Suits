using KSP.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace WearableProps.PropComponents
{
    public class LightComponent : MonoBehaviour
    {
        public KerbalEVA kerbal;
        public bool isHelmet;
        public List<Light> lights;

        private Light stockLight;

        public void Start()
        {
            UIButtonToggle toggle = GameObject.Find("ButtonActionGroupLights").GetComponent<UIButtonToggle>();
            toggle.onToggleOn.AddListener(OnLightsOn);
            toggle.onToggleOff.AddListener(OnLightsOff);
            ToggleLamp();
        }

        public void LateUpdate()
        {
            if (GameSettings.EVA_Lights.GetKeyDown(false))
                ToggleLamp();       
        }

        public void ToggleLamp()
        {
            if (kerbal.lampOn)
                OnLightsOn();
            else
                OnLightsOff();
        }

        public void OnLightsOn()
        {
            if (FlightGlobals.ActiveVessel != kerbal.vessel)
                return;

            if (isHelmet)
                kerbal.headLamp.SetActive(false);


            foreach (Light light in lights)
            {
                light.gameObject.SetActive(true);
            }
        }

        public void OnLightsOff()
        {
            if (FlightGlobals.ActiveVessel != kerbal.vessel)
                return;

            if (isHelmet)
                kerbal.headLamp.SetActive(false);

            foreach (Light light in lights)
            {
                light.gameObject.SetActive(false);
            }
        }
    }
}
