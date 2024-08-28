using System;
using UnityEngine;

using BepInEx;
using HarmonyLib;

using RavenCameraFX;

namespace RavenCameraFX
{
    [BepInPlugin("com.personperhaps.ravenimpactfx", "ravenimpactfx", "1.3")]
    public class Plugin : BaseUnityPlugin
    {

        public static Plugin plugin;

        private void Start()
        {
            Debug.Log("ravenimpactfx: Loading!");
            Harmony harmony = new Harmony("ravencamerafx");
            harmony.PatchAll();

            plugin = this;
        }


        public SecondOrder movementLean = new SecondOrder(0.5f, 0.7f, 0f, Vector3.zero);

        public static float SlightLeaning()
        {
            return plugin.movementLean.Update(Time.unscaledDeltaTime * 5f, Vector3.left * SteelInput.GetAxis(SteelInput.KeyBinds.Horizontal)).x;
        }

        [HarmonyPatch(typeof(PlayerFpParent), "FixedUpdate")]
        public class CameraUpdatePatch
        {
            static void Prefix()
            {
                
            }
        }


    }
}
