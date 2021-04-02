using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using WearableProps.IVA;

namespace WearableProps.Controllers
{
    public class KerbalSuitController : InternalModule
    {
        private const string DISPLAYNAME = "WearableProps";

        public void Awake()
        {
            if (Versioning.version_major == 1 && Versioning.version_minor < 10)
            {
                DestroyImmediate(this);
            }
        }

        public void Start()
        {
            CheckIVASuits(vessel);
            GameEvents.onVesselCrewWasModified.Add(CheckIVASuits);
            GameEvents.onVesselChange.Add(CheckIVASuits);
            GameEvents.onVesselWasModified.Add(CheckIVASuits);
        }


        public void CheckIVASuits(Vessel vessel)
        {
            if (this.vessel != vessel)
                return;

            try
            {
                foreach (Kerbal kerbal in internalModel.transform.GetComponentsInChildren<Kerbal>(false))
                {
                    if (CheckKerbalSuit(kerbal, out IVASuit ivaSuit))
                    {
                        Texture suitTexture = GameDatabase.Instance.GetTexture(ivaSuit.suitTexture, false);
                        Texture normalTexture = GameDatabase.Instance.GetTexture(ivaSuit.normalTexture, true);

                        foreach (SkinnedMeshRenderer smr in kerbal.transform.GetComponentsInChildren<SkinnedMeshRenderer>(false))
                        {
                            switch (smr.name)
                            {
                                case "body01":
                                case "helmet":
                                case "mesh_female_kerbalAstronaut01_body01":
                                case "mesh_female_kerbalAstronaut01_helmet":
                                    smr.material.SetTexture("_MainTex", suitTexture);
                                    smr.material.SetTextureScale("_MainTex", new Vector2(1f, -1f));
                                    smr.material.SetTexture("_BumpMap", normalTexture);
                                    smr.material.SetTextureScale("_BumpMap", new Vector2(1f, -1f));
                                    break;
                            }
                        }
                        kerbal.textureStandard = suitTexture as Texture2D;
                        kerbal.textureVeteran = suitTexture as Texture2D;
                    }
                }
            }
            catch(Exception)
            {
                Debug.Log($"[{DISPLAYNAME}] Error finding IVA Kerbals. Continuing...");
            }                   
        }

        public bool CheckKerbalSuit(Kerbal kerbal, out IVASuit ivaSuit)
        {
            string suitType = kerbal.protoCrewMember.suit.ToString();
            string gender = kerbal.protoCrewMember.gender.ToString();
            string suitTexture = kerbal.protoCrewMember.SuitTexturePath;

            ivaSuit = SuitComboDatabase.Instance.ivaSuits.Where(iva => iva.suitType == suitType && iva.gender == gender && iva.stockPath == suitTexture)?.FirstOrDefault();

            if (ivaSuit == null)
                return false;
            else return true;
        }

        public void OnDestroy()
        {
            GameEvents.onVesselCrewWasModified.Remove(CheckIVASuits);
            GameEvents.onVesselChange.Remove(CheckIVASuits);
            GameEvents.onVesselWasModified.Remove(CheckIVASuits);
        }
    }
}
