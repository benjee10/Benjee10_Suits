using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using WearableProps.Utils;
using WearableProps.IVA;

namespace WearableProps
{
    public class SuitComboLoader : LoadingSystem
    {
        public const string DISPLAYNAME = "WearableProps";

        private string progressTitle = string.Empty;
        private float progressFraction = 0.0f;

        private bool finished = false;

        public override void StartLoad()
        {
            StartCoroutine(LoadConfigNodes());        
        }

        private IEnumerator LoadConfigNodes()
        {
            ConfigNode[] configNodes = GameDatabase.Instance.GetConfigNodes("SUITCOMBOS");
            foreach (ConfigNode configNode in configNodes)
            {
                var suitNodes = configNode.GetNodes("SUITCOMBO");
                foreach (ConfigNode suitNode in suitNodes)
                {
                    // Load IVA Node
                    ConfigNode ivaNode = new ConfigNode();
                    if (suitNode.TryGetNode("IVA", ref ivaNode))
                    {                   
                        IVASuit ivaSuit = new IVASuit();

                        ivaNode.TryGetValue("suitTexture", ref ivaSuit.suitTexture);
                        ivaNode.TryGetValue("normalTexture", ref ivaSuit.normalTexture);
                        suitNode.TryGetValue("suitType", ref ivaSuit.suitType);
                        suitNode.TryGetValue("gender", ref ivaSuit.gender);
                        suitNode.TryGetValue("suitTexture", ref ivaSuit.stockPath);

                        progressTitle = ivaSuit.suitTexture;
                        bool successTexture = GameDatabase.Instance.ExistsTexture(ivaSuit.suitTexture);
                        bool successNormal = GameDatabase.Instance.ExistsTexture(ivaSuit.normalTexture);

                        if (successTexture && successNormal)
                        {
                            SuitComboDatabase.Instance.ivaSuits.Add(ivaSuit);
                        }
                        else if (!successTexture)
                        {
                            Debug.LogWarning($"[{DISPLAYNAME}] Could not find texture '{ivaSuit.suitTexture}'");
                        }
                        else if (!successNormal)
                        {
                            Debug.LogWarning($"[{DISPLAYNAME}] Could not find normal map '{ivaSuit.normalTexture}'");
                        }
                            
                        yield return null;
                    }
                    else continue;
                } 
            }

            finished = true;
        }

        public override bool IsReady()
        {
            return finished;
        }

        public override float ProgressFraction()
        {
            return progressFraction;
        }

        public override string ProgressTitle()
        {
            return "Loading IVA Suit Texture: " + progressTitle;
        }
    }
}
