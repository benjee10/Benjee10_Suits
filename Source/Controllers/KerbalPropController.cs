using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using WearableProps.Components;
using WearableProps.PropComponents;
using WearableProps.Utils;

namespace WearableProps.Controllers
{
    public class KerbalPropController : PartModule
    {
        private const string MODULENAME = "KerbalPropController";

        private KerbalEVA kerbal;
        private ModuleEvaChute evaChute;
        private ModuleInventoryPart inventory;

        private Dictionary<string, ModuleWearableProp> props;

        private Dictionary<string, HelmetComponent> helmetsComponents;
        private Dictionary<string, PropComponent> propComponents;

        private List<StoredPart> storedParts = new List<StoredPart>();
        private List<StoredPart> storedAttachments = new List<StoredPart>();

        private bool updatePack = false;

        private bool isInitialized = false;

        public override void OnAwake()
        {
            base.OnAwake();

            // If Breaking Ground is installed, all the different suits are handled as completely seperate parts,
            // this makes ModuleManager apply the Module to all those different suits. After MM runs SQUAD 
            // merges the different suits configs together to one config, which leads to duplicates of Part Modules
            // on one Kerbal. This ensures that only one KerbalPropController is active, by removing the duplicates
            List<KerbalPropController> modules = part.Modules.GetModules<KerbalPropController>().ToList();
            foreach (KerbalPropController module in modules)
            {
                if (module != this)
                {
                    Debug.Log($"[{MODULENAME}] Deleting duplicate Part Module");
                    part.RemoveModule(module);
                }
            }
        }

        public void Start()
        {
            kerbal = part.Modules.GetModule<KerbalEVA>();
            evaChute = part.Modules.GetModule<ModuleEvaChute>();
            inventory = kerbal.ModuleInventoryPartReference;

            props = PartLoader.LoadedPartsList.Where(p => p.partPrefab.FindModuleImplementing<ModuleWearableProp>())
                                              .ToDictionary(x => x.name, x => x.partPrefab.FindModuleImplementing<ModuleWearableProp>());

            InitializeProps();
        }
     
        public override void OnStartFinished(StartState state)
        {
            base.OnStartFinished(state);
            StartCoroutine(LateStart());
        }

        private IEnumerator LateStart()
        {
            // Wait one frame
            yield return null;
         
            OnModuleInventoryChanged(kerbal.ModuleInventoryPartReference);
            GameEvents.onModuleInventoryChanged.Add(OnModuleInventoryChanged);
            GameEvents.onModuleInventorySlotChanged.Add(OnModuleSlotChanged);
        }

        private void InitializeProps()
        {
            helmetsComponents = new Dictionary<string, HelmetComponent>();
            propComponents = new Dictionary<string, PropComponent>();

            foreach (var partProp in props)
            {
                ModuleWearableProp module = partProp.Value;
                Transform attachTransform = Utilities.GetTransformAlias(kerbal, module.attachTransform);
                GameObject partPrefab = module.part.partInfo.partPrefab.FindModelTransform("model").gameObject;

                Vector3 positionOffset = attachTransform.TransformPoint(module.positionOffset);
                Quaternion rotationOffset = attachTransform.rotation * Quaternion.Euler(module.rotationOffset);

                GameObject prop = Instantiate(partPrefab, positionOffset, rotationOffset, kerbal.transform); 
                prop.name = module.moduleId;

                TrackRigidbody trackRigidbody = prop.gameObject.AddComponent<TrackRigidbody>();
                trackRigidbody.attachTransform = attachTransform;
                trackRigidbody.positionOffset = module.positionOffset;
                trackRigidbody.rotationOffset = module.rotationOffset;
                prop.SetActive(false);

                List<Collider> colliders = prop.GetComponentsInChildren<Collider>(true).ToList();
                foreach (Collider collider in colliders)
                {
                    DestroyImmediate(collider);
                }
                    
                List<Light> lights = new List<Light>();
                if (Utilities.CheckForLights(prop, out lights))
                {
                    lights = lights.Where(l => module.lightNames.Contains(l.name)).ToList();                   
                }

                if (module.propType == PropType.HELMET)
                {
                    HelmetComponent helmetComponent = prop.AddComponent<HelmetComponent>();
                    helmetComponent.kerbal = this.kerbal;
                    helmetComponent.lights = lights;
                    helmetComponent.module = module;
                    helmetsComponents.Add(partProp.Key, helmetComponent);
                }        
                else
                {
                    PropComponent propComponent = prop.AddComponent<PropComponent>();
                    propComponent.isHelmetAttachment = module.propType == PropType.HELMETPROP ? true : false;
                    propComponent.kerbal = this.kerbal;
                    propComponent.lights = lights;
                    propComponent.module = module;
                    propComponents.Add(partProp.Key, propComponent);
                }
            }

            isInitialized = true;
        }

        private void OnModuleSlotChanged(ModuleInventoryPart moduleInventoryPart, int i) => OnModuleInventoryChanged(moduleInventoryPart);
        private void OnModuleInventoryChanged(ModuleInventoryPart moduleInventoryPart)
        {
            if (HighLogic.LoadedScene != GameScenes.FLIGHT)
                return;

            if (moduleInventoryPart != inventory)
                return;

            List<GameObject> equipedHelmets = new List<GameObject>();
            List<GameObject> equipedProps = new List<GameObject>();


            storedParts = inventory.storedParts.Values.Where(sp => !props.Keys.Contains(sp.partName)).ToList();
            storedAttachments = inventory.storedParts.Values.Where(sp => props.Keys.Contains(sp.partName)).ToList();

            foreach (var helmetsComponent in helmetsComponents)
            {
                if (storedAttachments.Any(sa => sa.partName == helmetsComponent.Key))
                {
                    helmetsComponent.Value.gameObject.SetActive(true);
                    helmetsComponent.Value.isEquiped = true;
                    equipedHelmets.Add(helmetsComponent.Value.gameObject);
                }
                else
                {
                    helmetsComponent.Value.gameObject.SetActive(false);
                    helmetsComponent.Value.isEquiped = false;
                }
            }

            foreach (var propComponent in propComponents)
            {

                if (storedAttachments.Any(sa => sa.partName == propComponent.Key))
                {
                    if (equipedHelmets.Count != 0 && propComponent.Value.module.propType == PropType.HELMETPROP)
                    {
                        propComponent.Value.gameObject.SetActive(false);
                        propComponent.Value.isEquiped = false;
                        
                    }
                    else
                    {
                        propComponent.Value.gameObject.SetActive(true);
                        propComponent.Value.isEquiped = true;
                        equipedProps.Add(propComponent.Value.gameObject);
                    }                 
                }
                else
                {
                    propComponent.Value.gameObject.SetActive(false);
                    propComponent.Value.isEquiped = false;
                }

                updatePack = true;
            } 
        }

        private void LateUpdate()
        {
            if (!isInitialized)
                return;

            if (updatePack)
            {
                UpdateKerbalPack(storedAttachments, storedParts);
                updatePack = false;                 
            }
        }

        private void UpdateKerbalPack(List<StoredPart> props, List<StoredPart> parts)
        {
            bool chute = parts.Any(p => p.partName == "evaChute");
            bool jetpack = parts.Any(p => p.partName == "evaJetpack");

            parts.RemoveAll(p => p.partName == "evaChute");
            parts.RemoveAll(p => p.partName == "evaJetpack");

            bool onlyProps = props.Count >= 1 && parts.Count == 0;

            if (onlyProps && chute)
            {
                kerbal.ChuteContainerTransform.gameObject.SetActive(false);
                kerbal.StorageTransform.gameObject.SetActive(false);
                kerbal.StorageSlimTransform.gameObject.SetActive(false);
                kerbal.BackpackStTransform.gameObject.SetActive(false);
                kerbal.ChuteStTransform.gameObject.SetActive(true);

                evaChute.SetCanopy(kerbal.ChuteStTransform);
                kerbal.HasParachute = true;
            }
            else if (onlyProps && jetpack)
            {
                kerbal.BackpackTransform.gameObject.SetActive(false);
                kerbal.JetpackTransform.gameObject.SetActive(true);
            }
            else if (onlyProps)
            {
                kerbal.StorageTransform.gameObject.SetActive(false);
                kerbal.StorageSlimTransform.gameObject.SetActive(false);
                kerbal.BackpackStTransform.gameObject.SetActive(false);
            }
        }
    }
}
