using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WearableProps.IVA;

namespace WearableProps
{
    [KSPAddon(KSPAddon.Startup.Instantly, false)]
    public class SuitComboDatabase : MonoBehaviour
    {
        public List<IVASuit> ivaSuits = new List<IVASuit>();

        public static SuitComboDatabase Instance { get; private set; }

        public void Awake()
        {
            Instance = this;

            // Add new loading process
            List<LoadingSystem> loadingSystems = LoadingScreen.Instance.loaders;
            int index = loadingSystems.FindIndex(l => l is PartLoader);

            GameObject loader = new GameObject("SuitComboLoader");
            SuitComboLoader suitComboLoader = loader.AddComponent<SuitComboLoader>();
            loadingSystems.Insert(index, suitComboLoader);
        }
    }
}
