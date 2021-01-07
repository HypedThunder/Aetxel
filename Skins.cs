using System;
using UnityEngine;
using R2API;
using RoR2;
using R2API.Utils;

namespace Aetxel
{
    public static class Skins
    {
        public static void RegisterSkins()
        {
            GameObject bodyPrefab = AetxelMod.myCharacter;

            GameObject model = bodyPrefab.GetComponentInChildren<ModelLocator>().modelTransform.gameObject;
            CharacterModel characterModel = model.GetComponent<CharacterModel>();

            if (model.GetComponent<ModelSkinController>()) AetxelMod.Destroy(model.GetComponent<ModelSkinController>());

            ModelSkinController skinController = model.AddComponent<ModelSkinController>();
            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            SkinnedMeshRenderer mainRenderer = Reflection.GetFieldValue<SkinnedMeshRenderer>(characterModel, "mainSkinnedMeshRenderer");

            LanguageAPI.Add("A'ETXEL_DEFAULT_SKIN_NAME", "Default");

            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];

            skinDefInfo.GameObjectActivations = Array.Empty<SkinDef.GameObjectActivation>();

            skinDefInfo.Icon = LoadoutAPI.CreateSkinIcon(new Color(0.4f, 0f, 0.5f), new Color(0.7f, 0f, 0.4f), new Color(0.3f, 0.05f, 0.3f), new Color(0.8f, 0.8f, 0.65f));
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[0];
            skinDefInfo.Name = "A'ETXEL_DEFAULT_SKIN_NAME";
            skinDefInfo.NameToken = "A'ETXEL_DEFAULT_SKIN_NAME";
            skinDefInfo.RendererInfos = characterModel.baseRendererInfos;
            skinDefInfo.RootObject = model;
            skinDefInfo.UnlockableName = "";

            SkinDef skinDef = LoadoutAPI.CreateNewSkinDef(skinDefInfo);

            skinController.skins = new SkinDef[1]
            {
                skinDef
            };
        }
    }
}