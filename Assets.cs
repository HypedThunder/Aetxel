using R2API;
using System;
using UnityEngine;

namespace Aetxel
{
    public static class Assets
    {
        public static AssetBundle AetxelAssetBundle = LoadAssetBundle(Aetxel.Properties.Resources.aetxelassets);

        public static Sprite icon1 = AetxelAssetBundle.LoadAsset<Sprite>("icon14");
        public static Sprite icon1b = AetxelAssetBundle.LoadAsset<Sprite>("icon1b 1");
        public static Sprite icon2 = AetxelAssetBundle.LoadAsset<Sprite>("icon12");
        public static Sprite icon3 = AetxelAssetBundle.LoadAsset<Sprite>("icon11");
        public static Sprite icon4 = AetxelAssetBundle.LoadAsset<Sprite>("icon13");

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