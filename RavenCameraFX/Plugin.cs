using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using UnityEngine;

using BepInEx;
using HarmonyLib;

using RavenCameraFX;

using Cinemachine;

namespace RavenCameraFX
{
    [BepInPlugin("com.personperhaps.ravenimpactfx", "ravenimpactfx", "1.3")]
    public class Plugin : BaseUnityPlugin
    {

        public static Plugin plugin;

        public Settings settings;

        public List<PropertyInfo> fields;

        private void Start()
        {
            Debug.Log("ravenimpactfx: Loading!");
            Harmony harmony = new Harmony("ravencamerafx");
            harmony.PatchAll();

            plugin = this;

            settings = new Settings();

            fields = typeof(Settings).GetProperties().ToList();

            UpdateConfig();
        }

        public void UpdateConfig()
        {
            foreach (PropertyInfo field in fields)
            {
                SettingAttribute attribute = field.GetCustomAttribute(typeof(SettingAttribute)) as SettingAttribute;
                Config.Bind(attribute.Category, attribute.Name, (float)field.GetValue(settings));
            }
        }

        public float DeltaTime()
        {
            return Time.deltaTime * Time.timeScale;
        }


        public float aiming = 0;
        public float Aiming(bool immediate = false)
        {

            if (FpsActorController.instance != null)
            {
                if (immediate)
                {
                    return FpsActorController.instance.Aiming() ? 0 : 1;
                }
                aiming = Mathf.Lerp(aiming, FpsActorController.instance.Aiming() ? 0 : 1, DeltaTime() * 4f);
                return aiming;
            }
            return 0;
        }

        public float grounded = 0;
        public float Grounded(bool immediate = false)
        {

            if (FpsActorController.instance != null)
            {
                if (immediate)
                {
                    return FpsActorController.instance.OnGround() ? 1 : 0;
                }
                grounded = Mathf.Lerp(grounded, FpsActorController.instance.OnGround() ? 1 : 0, DeltaTime() * 3f);
                return grounded;
            }
            return 0;
        }

        public float moving = 0;
        public float Moving(bool immediate = false)
        {

            if (FpsActorController.instance != null)
            {
                if (immediate)
                {
                    return FpsActorController.instance.Velocity().magnitude > 10 ? 1 : 0;
                }
                moving = Mathf.Lerp(moving, Mathf.Min(FpsActorController.instance.Velocity().magnitude / 20f, 1), DeltaTime() * 5);
                return moving;
            }
            return 0;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------

        public float cameraLean = 0;
        public Vector3 CameraLeaning()
        {
            cameraLean = Mathf.Lerp(cameraLean, SteelInput.GetAxis(SteelInput.KeyBinds.Horizontal), DeltaTime() * settings.cameraLeanSpeed);
            return (cameraLean * settings.cameraLeanAmount) * Vector3.forward;
        }


        //------------------------------------------------------------------------------------------------------------------------------------------------

        public float weaponLean = 0;
        public Vector3 WeaponLeaning()
        {
            weaponLean = Mathf.Lerp(weaponLean, SteelInput.GetAxis(SteelInput.KeyBinds.Horizontal), DeltaTime() * settings.weaponLeanSpeed);
            return (weaponLean * settings.weaponLeanAmount) * Vector3.forward * plugin.Aiming();
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------

        public SecondOrder hipfireOffset = new SecondOrder(0.3f, 0.6f, 0.1f, Vector3.zero);
        public Vector3 DownwardOffset()
        {
            return hipfireOffset.Update(DeltaTime() * settings.weaponDownwardOffsetSpeed, settings.weaponDownwardOffset * Aiming(true) * Vector3.up);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------


        public SecondOrder cameraRecoilZRotationSystem = new SecondOrder(1.6f, 0.4f, 0.1f, Vector3.zero);

        public void AddForceToCameraZRotation(Vector3 force)
        {
            cameraRecoilZRotationSystem.y = Vector3.Lerp(cameraRecoilZRotationSystem.y, force, 0.5f);
            cameraRecoilZRotationSystem.yd = force * 2f;
        }


        public Vector3 CameraRecoilZRotation()
        {
            return cameraRecoilZRotationSystem.Update(DeltaTime(), Vector3.zero) * settings.cameraRecoilPower;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        //                                                                                 
        //a snappy x axis rotation. to simulate the camera being bounced back

        public SecondOrder cameraRecoilXRotationSystem = new SecondOrder(2f, 0.4f, 0.1f, Vector3.zero);

        public void AddForceToCameraXRotation(Vector3 force)
        {
            cameraRecoilXRotationSystem.y = Vector3.Lerp(cameraRecoilXRotationSystem.y, force, 0.1f);
            cameraRecoilXRotationSystem.yd = force * 2f;
        }


        public Vector3 CameraRecoilXRotation()
        {
            return cameraRecoilXRotationSystem.Update(DeltaTime(), Vector3.zero) * settings.cameraRecoilUpRotationPower;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------


        //                                                                                 
        //an additional weapon snap. looks better

        public SecondOrder weaponRecoilXRotationSystem = new SecondOrder(2f, 0.1f, 0.1f, Vector3.zero);

        public void AddForceToweaponXRotation(Vector3 force)
        {
            weaponRecoilXRotationSystem.y = Vector3.Lerp(weaponRecoilXRotationSystem.y, force, 0.1f);
            weaponRecoilXRotationSystem.yd = force * 2f;
        }


        public Vector3 weaponRecoilXRotation()
        {
            return weaponRecoilXRotationSystem.Update(DeltaTime(), Vector3.zero) * settings.weaponRecoilUpRotationPower;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        public SecondOrder cameraDipSpring = new SecondOrder(2f, 0.5f, 0.1f, Vector3.zero);
        public Vector3 CameraDip()
        {
            return plugin.cameraDipSpring.Update(DeltaTime(), Vector3.zero);



        }

        //------------------------------------------------------------------------------------------------------------------------------------------------

        public SecondOrder weaponPushbackSystem = new SecondOrder(3f, 0.35f, 0.1f, Vector3.zero);

        public Vector3 WeaponPushback()
        {

            Vector3 pushback = weaponPushbackSystem.Update(DeltaTime(), Vector3.zero) * settings.weaponRecoilPushbackPower * 0.05f;


            //we gotta soft limit the pushback, or things will look funky

            if (pushback.z < -0.025f)
            {
                pushback = Vector3.Lerp(pushback, Vector3.back * 0.025f, 0.8f);
            }

            return pushback;
        }

        public void AddForceToWeaponPushback(Vector3 force, float cooldown = -1)
        {
            //we can also cap here


            weaponPushbackSystem.y = Vector3.Lerp(weaponPushbackSystem.y, force, 0.2f);
            weaponPushbackSystem.yd = force * 1.5f;

            if (cooldown != -1)
            {

                weaponPushbackSystem.ChangeArgs(Mathf.Lerp(0.4f / (cooldown + 0.01f), 3, 0.5f), 0.4f, 0.1f);
            }
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        // the weapon pushback can be used in junction with some random z weapon rotation

        public SecondOrder weaponZRotation = new SecondOrder(2f, 0.2f, 0.05f, Vector3.zero);

        public Vector3 WeaponZRotation()
        {
            Vector3 rotation = weaponZRotation.Update(DeltaTime(), Vector3.zero) * settings.weaponRecoilYZRotation;
            Debug.Log(rotation);
            return rotation;
        }

        public void AddForceToWeaponZRotation(Vector3 force)
        {
            weaponZRotation.y = Vector3.Lerp(weaponZRotation.y, force, 0.1f);
            weaponZRotation.yd = force * 1.3f;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------
        // run of the mill camera bob

        public Vector3 CameraWalkBob()
        {
            float t = FpsActorController.instance.GetStepPhase();
            Vector3 targetBob = new Vector3(-0.05f * Mathf.Pow(Mathf.Abs(Mathf.Sin(3.14159f * t + 0.5f * 3.14159f) - 0.9f), 2), -0.05f * Mathf.Pow(Mathf.Abs(Mathf.Cos(3.14159f * t * 2) - 0.9f), 2), 0);

            if (FpsActorController.instance.actor.IsSeated())
            {
                return Vector3.zero;
            }

            return targetBob * Mathf.Lerp(0.2f, 1f, Aiming()) * Moving() * Grounded() * plugin.settings.cameraBobIntensity;
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------

        // here, we can physically move the camera around like actual recoil. to make things smooth, we make the camera move over time

        public Vector2 cameraPhysicalMovement = Vector2.zero;

        public void PhysicallyMoveCamera(Vector3 force)
        {
            Vector2 vec2Force = new Vector2(force.y, force.x);

            cameraPhysicalMovement += vec2Force;
        }

        public void UpdateCameraPhysicalMovement()
        {
            LocalPlayer.controller.controller.m_MouseLook.ApplyScriptedRotation(cameraPhysicalMovement * settings.cameraRecoilMoveCameraPower);
            cameraPhysicalMovement = Vector2.MoveTowards(cameraPhysicalMovement, Vector3.zero, 0.5f);
        }

        //------------------------------------------------------------------------------------------------------------------------------------------------

        static List<Tuple<Vector3, Quaternion>> armBones = new List<Tuple<Vector3, Quaternion>>();

        static public Dictionary<Transform, Tuple<Vector3, Vector3>> bones = new Dictionary<Transform, Tuple<Vector3, Vector3>>();

        static public Vector3 averageDelta = Vector3.zero;

        static public SecondOrder shakeCoil = new SecondOrder(2f, 0.2f, 0.05f, Vector3.zero);
        public Vector3 ProceduralCameraShake()
        {
            try
            {
                Vector3 targetAverage = Vector3.zero;
                if (FpsActorController.instance.actor != null && !GameManager.IsSpectating())
                {
                    if (FpsActorController.instance.actor.activeWeapon != null)
                    {
                        if (FpsActorController.instance.actor.activeWeapon.arms != null)
                        {
                            Dictionary<Transform, Tuple<Vector3, Vector3>> replaceBones = new Dictionary<Transform, Tuple<Vector3, Vector3>>();

                            List<Tuple<Vector3, Quaternion>> newArmBones = new List<Tuple<Vector3, Quaternion>>();
                            foreach (Transform bone in FpsActorController.instance.actor.activeWeapon.arms.bones)
                            {
                                newArmBones.Add(new Tuple<Vector3, Quaternion>(bone.localPosition, bone.localRotation));
                            }
                            armBones = newArmBones;
                            foreach (Transform transform in bones.Keys)
                            {
                                if (transform.localPosition == null)
                                {
                                    break;
                                }
                                if (bones.ContainsKey(transform))
                                {
                                    Vector3 inversePoint = transform.localPosition;
                                    replaceBones.Add(transform, new Tuple<Vector3, Vector3>(bones[transform].Item1, bones[transform].Item2));
                                    targetAverage += Vector3.ClampMagnitude((bones[transform].Item1 - transform.localRotation.eulerAngles) / 5, 50);
                                    targetAverage += Vector3.ClampMagnitude((transform.localPosition - bones[transform].Item2), 50);
                                }
                            }

                            Dictionary<Transform, Tuple<Vector3, Vector3>> dict = FpsActorController.instance.actor.activeWeapon.arms.bones.ToDictionary(x => x, x => new Tuple<Vector3, Vector3>(x.localRotation.eulerAngles, x.localPosition));

                            bones = dict;
                        }
                    }
                    if (averageDelta.sqrMagnitude > 0.1f && shakeCoil.y.magnitude < 20f)
                    {
                        shakeCoil.yd += averageDelta;
                    }
                    else
                    {
                        averageDelta = Vector3.zero;
                    }
                    if (FpsActorController.instance.actor.IsAiming())
                    {
                        averageDelta /= 10;
                    }
                    float scaleOnDistanceToZero = targetAverage.magnitude < averageDelta.magnitude ? (Mathf.Pow(2, (Vector3.Distance(averageDelta, Vector3.zero) * -0.4f) - 6f) * 400 + 1) : 1;
                    float scaleOnTargetDifference = targetAverage.magnitude > 0.2 ? (Mathf.Log10(Mathf.Abs((averageDelta - targetAverage).magnitude + 1f)) + 2.3f) : 1;
                    float returnSpeed = DeltaTime() * scaleOnTargetDifference * scaleOnDistanceToZero / 4;
                    averageDelta = Vector3.Lerp(averageDelta, targetAverage, returnSpeed);



                }
                return shakeCoil.Update(DeltaTime(), Vector3.zero) * 1.2f;
            }
            catch (Exception e)
            {
                Debug.Log("SHIT");
                bones.Clear();
                averageDelta = Vector3.zero;
                return Vector3.zero;
            }
        }
        [HarmonyPatch(typeof(LoadoutUi), "LoadSlotEntry")]
        public class AnyWeaponSlot
        {
            public static bool CanUseWeaponEntry(WeaponManager.WeaponEntry entry)
            {
                return (GameManager.GameParameters().playerHasAllWeapons || GameManager.instance.gameInfo.team[GameManager.PlayerTeam()].IsWeaponEntryAvailable(entry));
            }
            public static bool Prefix(LoadoutUi __instance, ref WeaponManager.WeaponEntry __result, WeaponManager.WeaponSlot entrySlot, string keyName)
            {
                return true;
                if (!PlayerPrefs.HasKey(keyName))
                {
                    foreach (WeaponManager.WeaponEntry weaponEntry in WeaponManager.instance.allWeapons)
                    {
                        if (CanUseWeaponEntry(weaponEntry))
                        {
                            __result = weaponEntry;
                            return false;
                        }
                    }
                    __result = null;
                    return false;
                }
                int @int = PlayerPrefs.GetInt(keyName);
                if (@int == -1)
                {
                    __result = null;
                    return false;
                }
                WeaponManager.WeaponEntry weaponEntry2 = null;
                foreach (WeaponManager.WeaponEntry weaponEntry3 in WeaponManager.instance.allWeapons)
                {
                    if (CanUseWeaponEntry(weaponEntry3))
                    {
                        if (weaponEntry3.nameHash == @int)
                        {
                            __result = weaponEntry3;
                            return false;
                        }
                        if (weaponEntry2 == null)
                        {
                            weaponEntry2 = weaponEntry3;
                        }
                    }
                }
                __result = weaponEntry2;
                return false;
            }
        }
        [HarmonyPatch(typeof(WeaponManager), "GetWeaponTagDictionary")]
        public class AnyWeaponSlot2
        {
            public static bool Prefix(WeaponManager __instance, ref bool allSlots)
            {
                return true;
                allSlots = true;
                return true;
            }
        }


        [HarmonyPatch(typeof(FpsActorController), "OnLand")]
        public class OnLandPatch
        {
            public static bool Prefix(FpsActorController __instance)
            {
                if (__instance.actor.IsSeated())
                {
                    return false;
                }
                float num = Mathf.Clamp((-__instance.actor.Velocity().y - 2f) * 0.3f, 0f, 2f);
                if (__instance.IsSprinting())
                {
                    num *= 2f;
                }
                __instance.fpParent.ApplyRecoil(new Vector3(0f, -num * 0.3f, 0f), true);
                __instance.fpParent.KickCamera(new Vector3(num, 0f, 0f));
                Vector3 impact = __instance.actor.Velocity();
                impact = new Vector3(0, impact.y, 0);
                plugin.cameraDipSpring.yd += Vector3.ClampMagnitude(impact, 7);


                plugin.AddForceToWeaponZRotation(new Vector3(Mathf.Lerp(-1, 1f, UnityEngine.Random.value), Mathf.Lerp(-1, 1f, UnityEngine.Random.value), Mathf.Lerp(-1, 1f, UnityEngine.Random.value))
                    * 1 * impact.magnitude);

                plugin.AddForceToWeaponPushback(Vector3.down * impact.magnitude * 0.01f, -1);

                return false;
            }
        }


        [HarmonyPatch(typeof(PlayerFpParent), "FixedUpdate")]
        public class WeaponUpdatePatch
        {
            static void Postfix(PlayerFpParent __instance)
            {
                __instance.shoulderParent.localEulerAngles += plugin.WeaponLeaning() + plugin.WeaponZRotation();

                __instance.shoulderParent.localPosition += plugin.DownwardOffset();

                __instance.fpCamera.fieldOfView += Mathf.Abs(plugin.CameraRecoilZRotation().z) * plugin.settings.cameraRecoilFovPower;


            }
        }

        public Vector3 storedWeaponParentLocation; //store this to revert the position before the next update.

        [HarmonyPatch(typeof(PlayerFpParent), "LateUpdate")]
        public class CameraUpdatePatch
        {
            static void Prefix(PlayerFpParent __instance)
            {
                if (plugin.storedWeaponParentLocation == null)
                    return;

                __instance.weaponParent.transform.localPosition = plugin.storedWeaponParentLocation;


            }

            static void Postfix(PlayerFpParent __instance)
            {
                plugin.storedWeaponParentLocation = __instance.weaponParent.transform.localPosition;

                __instance.fpCameraParent.localEulerAngles += plugin.CameraLeaning() + plugin.CameraRecoilZRotation() * 2 + plugin.CameraRecoilXRotation() + plugin.ProceduralCameraShake();

                __instance.fpCameraParent.position += plugin.CameraWalkBob() + plugin.CameraDip();



                __instance.weaponParent.transform.localPosition += plugin.WeaponPushback();

                plugin.UpdateCameraPhysicalMovement();

                plugin.recoilControl = Mathf.MoveTowards(plugin.recoilControl, 1, 0.1f * plugin.DeltaTime());
            }
        }

        public float recoilControl = 1;

        [HarmonyPatch(typeof(Weapon), "ApplyRecoil")]
        public class RecoilPatch
        {
            static void Prefix(Weapon __instance)
            {
                if (__instance.UserIsPlayer())
                {
                    if (__instance.configuration.autoReloadDelay <= 0)
                    {
                        __instance.configuration.autoReloadDelay = 0.1f;
                    }

                    float randomDirection = UnityEngine.Random.value > 0.5f ? 1 : -1;

                    float kickback = __instance.configuration.kickback * (__instance.user.controller.Prone() ? __instance.configuration.kickbackProneMultiplier : 1) * plugin.recoilControl;

                    plugin.recoilControl += kickback * 0.1f;

                    plugin.AddForceToCameraZRotation(Vector3.back * kickback * randomDirection);

                    plugin.AddForceToCameraXRotation(Vector3.left * kickback);

                    Vector3 randomKnockAround = new Vector3(UnityEngine.Random.Range(-1, 1) * __instance.configuration.randomKick, UnityEngine.Random.Range(0.5f, 1) * __instance.configuration.randomKick, UnityEngine.Random.Range(-1, 1) * __instance.configuration.randomKick);

                    plugin.AddForceToWeaponPushback(Vector3.back * Mathf.Min(kickback, 0.5f) + randomKnockAround * 0.2f, __instance.configuration.cooldown);

                    plugin.AddForceToCameraXRotation(Vector3.left * kickback);



                    plugin.AddForceToWeaponZRotation(new Vector3(Mathf.Lerp(-1, 1f, UnityEngine.Random.value), Mathf.Lerp(-1, 1f, UnityEngine.Random.value), Mathf.Lerp(-1, 1f, UnityEngine.Random.value))
                    * Mathf.Lerp(__instance.configuration.kickback, __instance.configuration.randomKick, 0.8f) * 4);

                    plugin.PhysicallyMoveCamera(Vector3.right * kickback);
                }
            }
        }

        Vector2 scrollPos;
        void OnGUI()
        {
            var lobbyStyle = new GUIStyle(GUI.skin.box);
            if(Options.instance == null)
            {
                return;
            }
            if (Options.IsOpen())
            {
                
                GUILayout.BeginArea(new Rect(0, Screen.height - 500, 300f, 400f), string.Empty);
                GUILayout.BeginVertical(lobbyStyle);
                GUILayout.BeginHorizontal();

                scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(280), GUILayout.Height(500));


                GUILayout.Label("<b>Raven Camera FX 2</b>");


                if (GUILayout.Button("Save Config"))
                {
                    UpdateConfig();
                }

                foreach (PropertyInfo field in fields)
                {
                    GUILayout.FlexibleSpace();

                    if(field.Name.StartsWith("divider"))
                    {
                        GUILayout.Space(5);
                        GUILayout.Label("<b>" + (string)field.GetValue(settings) + "</b>");
                        GUILayout.Space(2);
                        continue;
                    }

                    SettingAttribute attribute = field.GetCustomAttribute(typeof(SettingAttribute)) as SettingAttribute;

                    

                    float fieldValue = Mathf.Round((float)field.GetValue(settings) * 100) / 100;

                    GUILayout.Label(attribute.Name + ": "+ fieldValue);
                    field.SetValue(settings, GUILayout.HorizontalSlider(fieldValue, 0, attribute.MaxVal));

                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndScrollView();

                

                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.EndArea();
                return;
            }
        }
    }
    [System.AttributeUsage(System.AttributeTargets.Property)
]
    public class SettingAttribute : System.Attribute
    {
        public string Name;
        public float MaxVal;

        public string Category;

        public SettingAttribute(string name, string category)
        {
           
            Name = name;
            Category = category;
            MaxVal = 1f;
        }
    }
}
