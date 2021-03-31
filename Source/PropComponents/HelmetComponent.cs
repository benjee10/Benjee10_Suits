using KSP.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WearableProps.PropComponents;

namespace WearableProps.Components
{
    public class HelmetComponent : MonoBehaviour
    {
        public const string DISPLAYNAME = "WearableProps";

        public KerbalEVA kerbal;
        public ModuleWearableProp module;

        public bool isEquiped;
        public List<Light> lights;

        private bool hasAnimation;
        private Animation visorAnimation;

        private KerbalEVA.VisorStates visorState;

        // Defines the render queues for the visor inner 
        // and outer material to stop clipping when viewing
        // from some steep angles
        public int visorInnerRenderQueue = 3000;
        public int visorOuterRenderQueue = 4000;

        public string visorInner = "visor";
        public string visorOuter = "visorBlack";

        public void Awake()
        {
            Transform visorInner = transform.GetComponentsInChildren<Transform>()
                                            .Where(t => t.name == this.visorInner).FirstOrDefault();
            if (visorInner != null)
            {
                MeshRenderer renderer = visorInner.GetComponent<MeshRenderer>();
                renderer.material.renderQueue = visorInnerRenderQueue;
            }

            Transform visorOuter = transform.GetComponentsInChildren<Transform>()
                                            .Where(t => t.name == this.visorOuter).FirstOrDefault();
            if (visorInner != null)
            {
                MeshRenderer renderer = visorOuter.GetComponent<MeshRenderer>();
                renderer.material.renderQueue = visorOuterRenderQueue;
            }
        }

        public void Start()
        {
            if (!string.IsNullOrEmpty(module.visorAnimationName))
            {
                List<Animation> animators = transform.GetComponentsInChildren<Animation>().ToList();
                foreach (Animation animation in animators)
                {
                    AnimationClip animationClip = animation.GetClip(module.visorAnimationName);

                    if (animationClip == null)
                        continue;

                    visorAnimation = animation;
                    break;
                }
                
                if (visorAnimation == null)
                {
                    Debug.LogWarning($"[{DISPLAYNAME}] Could not find visor animation '{visorAnimation}' on first pass");
                    visorAnimation = kerbal.part.FindModelAnimator(module.visorAnimationName);
                }
                
                if (visorAnimation == null)
                {
                    Debug.LogWarning($"[{DISPLAYNAME}] Could not find visor animation '{visorAnimation}' on second pass");
                    hasAnimation = false;
                }
                else
                {
                    hasAnimation = true;
                    visorState = kerbal.VisorState;
                    StartFSM();
                }
            }
            else
            {
                hasAnimation = false;   
            }

            LightComponent lightComponent = gameObject.AddComponent<LightComponent>();
            lightComponent.kerbal = this.kerbal;
            lightComponent.lights = lights;
            lightComponent.isHelmet = true;
            
            GameEvents.OnVisorLowering.Add(OnVisorLowering);
            GameEvents.OnVisorRaising.Add(OnVisorRaised);
            GameEvents.OnHelmetChanged.Add(OnHelmetChanged);
        }

        public void Update()
        {
            UpdateFSM();
        }

        private void OnHelmetChanged(KerbalEVA kerbal, bool helmet, bool neckring)
        {
            if (this.kerbal != kerbal)
                return;

            if (!isEquiped)
                return;

            gameObject.SetActive(helmet);
            kerbal.helmetMesh.enabled = false;
        }
       
        private void OnVisorLowering(KerbalEVA kerbal)
        {
            if (this.kerbal != kerbal)
                return;

            LowerVisor();
        }

        private void OnVisorRaised(KerbalEVA kerbal)
        {
            if (this.kerbal != kerbal)
                return;

            RaiseVisor();
        }

        private void StartFSM()
        {
            if (!hasAnimation)
                return;

            switch(visorState)
            {
                case KerbalEVA.VisorStates.Lowered:
                    visorAnimation[module.visorAnimationName].wrapMode = WrapMode.ClampForever;
                    visorAnimation[module.visorAnimationName].normalizedTime = 0.0f;
                    visorAnimation[module.visorAnimationName].enabled = true;
                    visorAnimation[module.visorAnimationName].weight = 1.0f;
                    visorAnimation.Stop(module.visorAnimationName);
                    break;
                case KerbalEVA.VisorStates.Raised:
                    visorAnimation[module.visorAnimationName].wrapMode = WrapMode.ClampForever;
                    visorAnimation[module.visorAnimationName].normalizedTime = 1.0f;
                    visorAnimation[module.visorAnimationName].enabled = true;
                    visorAnimation[module.visorAnimationName].weight = 1.0f;
                    break;
            }
            
        }

        private void RaiseVisor()
        {
            if (!hasAnimation)
                return;

            visorAnimation[module.visorAnimationName].speed = 1f;
            visorAnimation[module.visorAnimationName].normalizedTime = 0.0f;
            visorAnimation[module.visorAnimationName].enabled = true;
            visorAnimation.Play(module.visorAnimationName);
            visorState = KerbalEVA.VisorStates.Raising;
        }

        private void LowerVisor()
        {
            if (!hasAnimation)
                return;

            visorAnimation[module.visorAnimationName].speed = -1f;
            visorAnimation[module.visorAnimationName].normalizedTime = 1.0f;
            visorAnimation[module.visorAnimationName].enabled = true;
            visorAnimation.Play(module.visorAnimationName);
            visorState = KerbalEVA.VisorStates.Lowering;
        }

        public void UpdateFSM()
        {
            if (!hasAnimation)
                return;

            switch (visorState)
            {
                case KerbalEVA.VisorStates.Raising:
                    if (visorAnimation[module.visorAnimationName].normalizedTime >= 1.0f)
                    {
                        visorAnimation.Stop(module.visorAnimationName);
                        visorState = KerbalEVA.VisorStates.Raised;
                    }
                    break;
                case KerbalEVA.VisorStates.Lowering:
                    if (visorAnimation[module.visorAnimationName].normalizedTime <= 0.0f)
                    {
                        visorAnimation.Stop(module.visorAnimationName);
                        visorState = KerbalEVA.VisorStates.Lowered;
                    }
                    break;
            }
        }

        public void OnEnable()
        {
            StartFSM();

            kerbal.ToggleHelmetAndNeckRing(true, true);
            kerbal.headLamp.SetActive(false);
            kerbal.helmetMesh.enabled = false;
            kerbal.VisorRenderer.enabled = false;        
        }

        public void OnDisable()
        {
            kerbal.helmetMesh.enabled = true;
            kerbal.VisorRenderer.enabled = true;
            kerbal.headLamp.SetActive(true);
        }      
    }
}
