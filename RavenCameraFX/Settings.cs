using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime;


using System.Reflection;

using System.ComponentModel;

namespace RavenCameraFX
{
    public class Settings
    {

        [SettingAttribute("Camera Bob Intensity", "Misc", MaxVal = 2)]
        public float cameraBobIntensity { get; set; } = 1.7f;
        //camera leaning

        [SettingAttribute("Camera Lean Amount", "Misc", MaxVal = 10)]
        public float cameraLeanAmount { get; set; } = 1;

        [SettingAttribute("Camera Lean Speed", "Misc", MaxVal = 10)]
        public float cameraLeanSpeed { get; set; } = 5;

        //weapon leaning

        [SettingAttribute("Weapon Lean Amount", "Misc", MaxVal = 20)]
        public float weaponLeanAmount { get; set; } = 10;

        [SettingAttribute("Weapon Lean Amount", "Misc", MaxVal = 10)]
        public float weaponLeanSpeed { get; set; } = 4;


        [SettingAttribute("Weapon Downward Offset", "Misc", MaxVal = -0.1f)]
        public float weaponDownwardOffset { get; set; } = -0.05f;

        [SettingAttribute("Weapon Downward Offset Speed", "Misc", MaxVal = 10)]
        public float weaponDownwardOffsetSpeed { get; set; } = 4;


        [SettingAttribute("Camera Recoil Power", "Recoil.Camera", MaxVal = 5f)]
        public float cameraRecoilPower { get; set; }  = 3f;

        [SettingAttribute("Camera FOV Recoil Power", "Recoil.Camera", MaxVal = 2f)]
        public float cameraRecoilFovPower { get; set; } = 1.2f;


        [SettingAttribute("Camera Vertical Recoil", "Recoil.Camera", MaxVal = 20f)]
        public float cameraRecoilUpRotationPower { get; set; } = 10f;

        [SettingAttribute("Camera Mouse Movement", "Recoil.Camera", MaxVal = 1f)]
        public float cameraRecoilMoveCameraPower { get; set; } = 0.5f;

        [SettingAttribute("Weapon Recoil Pushback", "Recoil.Weapon", MaxVal = 50f)]
        public float weaponRecoilPushbackPower { get; set; } = 30;

        [SettingAttribute("Weapon Recoil Random Rotation", "Recoil.Weapon", MaxVal = 25f)]
        public float weaponRecoilYZRotation { get; set; } = 10;

        [SettingAttribute("Weapon Vertical Recoil", "Recoil.Weapon", MaxVal = 25f)]
        public float weaponRecoilUpRotationPower { get; set; } = 10;



    }
}
