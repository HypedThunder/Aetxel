using R2API;
using System;
using UnityEngine;

namespace Aetxel
{
    public static class Assets
    {
        public static AssetBundle AssassinAssetBundle = LoadAssetBundle(RealAssassin.Properties.Resources.assassinassets);

        public static Texture charPortrait = AssassinAssetBundle.LoadAsset<Texture>("Icon");
        public static Sprite iconP = AssassinAssetBundle.LoadAsset<Sprite>("iconP");
        public static Sprite icon1 = AssassinAssetBundle.LoadAsset<Sprite>("icon1");
        public static Sprite icon2 = AssassinAssetBundle.LoadAsset<Sprite>("icon2");
        public static Sprite icon3 = AssassinAssetBundle.LoadAsset<Sprite>("icon3");
        public static Sprite icon4 = AssassinAssetBundle.LoadAsset<Sprite>("icon4");

        static AssetBundle LoadAssetBundle(Byte[] resourceBytes)
        {
            //Check to make sure that the byte array supplied is not null, and throw an appropriate exception if they are.
            if (resourceBytes == null) throw new ArgumentNullException(nameof(resourceBytes));

            //Actually load the bundle with a Unity function.
            var bundle = AssetBundle.LoadFromMemory(resourceBytes);

            return bundle;
        }
    }
}