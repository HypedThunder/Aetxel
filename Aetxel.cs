using System;
using System.Collections;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using EntityStates;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using EntityStates.Aetxel.Weapon;
using EntityStates.Aetxel.Weapon2;
using EntityStates.Aetxel.Weapon3;
using EntityStates.Aetxel.Weapon4;
using EntityStates.Aetxel.Weapon5;
using RoR2.Projectile;

namespace Aetxel
{
    [BepInDependency("com.bepis.r2api")]

    [BepInPlugin(MODUID, "Aetxel", "0.0.2")]
    [R2APISubmoduleDependency(nameof(PrefabAPI), nameof(SurvivorAPI), nameof(LoadoutAPI), nameof(LanguageAPI), nameof(BuffAPI), nameof(EffectAPI))]

    public class AetxelMod : BaseUnityPlugin
    {
        public const string MODUID = "com.Ruxbieno.Aetxel";

        internal static AetxelMod instance;

        public static GameObject myCharacter;
        public static GameObject characterDisplay;
        public static GameObject doppelganger;

        public static GameObject aetxelCrosshair;

        private static readonly Color CHAR_COLOR = new Color(0.6f, 0.15f, 0.6f);

        private static ConfigEntry<float> baseHealth;
        private static ConfigEntry<float> healthGrowth;
        private static ConfigEntry<float> baseArmor;
        private static ConfigEntry<float> baseDamage;
        private static ConfigEntry<float> damageGrowth;
        private static ConfigEntry<float> baseRegen;
        private static ConfigEntry<float> regenGrowth;
        private static ConfigEntry<float> baseSpeed;

        public static GameObject Bomb;

        public static GameObject Tornado;

        public static BuffIndex syringebuff;

        public void Awake()
        {
            instance = this;

            ReadConfig();
            RegisterBuffs();
            RegisterStates();
            RegisterCharacter();
            Skins.RegisterSkins();
            CreateMaster();
            RegisterProjectiles();
        }

        private void ReadConfig()
        {
            baseHealth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Health"), 130f, new ConfigDescription("Base health", null, Array.Empty<object>()));
            healthGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Health growth"), 32f, new ConfigDescription("Health per level", null, Array.Empty<object>()));
            baseArmor = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Armor"), 10f, new ConfigDescription("Base armor", null, Array.Empty<object>()));
            baseDamage = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Damage"), 12f, new ConfigDescription("Base damage", null, Array.Empty<object>()));
            damageGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Damage growth"), 2.4f, new ConfigDescription("Damage per level", null, Array.Empty<object>()));
            baseRegen = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Regen"), 1f, new ConfigDescription("Base HP regen", null, Array.Empty<object>()));
            regenGrowth = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Regen growth"), 0.5f, new ConfigDescription("HP regen per level", null, Array.Empty<object>()));
            baseSpeed = base.Config.Bind<float>(new ConfigDefinition("01 - General Settings", "Speed"), 7f, new ConfigDescription("Base speed", null, Array.Empty<object>()));
        }


        private void RegisterBuffs()
        {
            BuffDef buffDef = new BuffDef
            {
                name = "Adrenaline2",
                iconPath = "Textures/BuffIcons/texBuffGenericShield",
                buffColor = AetxelMod.CHAR_COLOR,
                canStack = true,
                isDebuff = false,
                eliteIndex = EliteIndex.None
            };
            CustomBuff customBuff = new CustomBuff(buffDef);
            AetxelMod.syringebuff = BuffAPI.Add(customBuff);
        }

           private void RegisterCharacter()
        {
            //create a clone of the grovetender prefab
            myCharacter = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterBodies/LemurianBody"), "Prefabs/CharacterBodies/AetxelBody", true);
            //create a display prefab
            characterDisplay = PrefabAPI.InstantiateClone(myCharacter.GetComponent<ModelLocator>().modelBaseTransform.gameObject, "AetxelDisplay", true);
            myCharacter.GetComponent<SetStateOnHurt>().canBeHitStunned = false;
            myCharacter.tag = "Player";

            //add custom menu animation script
            characterDisplay.AddComponent<MenuAnim>();


            CharacterBody charBody = myCharacter.GetComponent<CharacterBody>();
            charBody.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;

            //swap to generic mainstate to fix clunky controls
            myCharacter.GetComponent<EntityStateMachine>().mainStateType = new SerializableEntityStateType(typeof(GenericCharacterMain));

            myCharacter.GetComponentInChildren<Interactor>().maxInteractionDistance = 5f;

            //crosshair stuff
            charBody.SetSpreadBloom(0, false);
            charBody.spreadBloomCurve = Resources.Load<GameObject>("Prefabs/CharacterBodies/BanditBody").GetComponent<CharacterBody>().spreadBloomCurve;
            charBody.spreadBloomDecayTime = Resources.Load<GameObject>("Prefabs/CharacterBodies/BanditBody").GetComponent<CharacterBody>().spreadBloomDecayTime;

            charBody.hullClassification = HullClassification.Human;




            characterDisplay.transform.localScale = Vector3.one * 1.2f;
            characterDisplay.AddComponent<NetworkIdentity>();

            //create the custom crosshair
            aetxelCrosshair = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/Crosshair/BanditCrosshair"), "AetxelCrosshair", true);
            aetxelCrosshair.AddComponent<NetworkIdentity>();

            //networking

            if (myCharacter) PrefabAPI.RegisterNetworkPrefab(myCharacter);
            if (characterDisplay) PrefabAPI.RegisterNetworkPrefab(characterDisplay);
            if (doppelganger) PrefabAPI.RegisterNetworkPrefab(doppelganger);
            if (aetxelCrosshair) PrefabAPI.RegisterNetworkPrefab(aetxelCrosshair);



            string desc = "A'etxel is a heavy close range bruiser that makes use of fire and scavenged tools to crush enemies.<color=#CCD3E0>" + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Scavenged Scattergun fires faster if you mash." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Scorching Winds always deals the same amount of damage, no matter how far you are from the target." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Heated Hook is good for mobility and offense." + Environment.NewLine + Environment.NewLine;
            desc = desc + "< ! > Molten Earth is great for clearing out crowds easily.</color>" + Environment.NewLine;

            LanguageAPI.Add("AETXEL_NAME", "A'etxel");
            LanguageAPI.Add("AETXEL_DESCRIPTION", desc);
            LanguageAPI.Add("AETXEL_SUBTITLE", "Fortunate Scavenger");
            LanguageAPI.Add("AETXEL_OUTRO_FLAVOR", "...and so he left, satisfied with his final haul.");


            charBody.name = "AetxelBody";
            charBody.baseNameToken = "AETXEL_NAME";
            charBody.subtitleNameToken = "AETXEL_SUBTITLE";
            charBody.crosshairPrefab = aetxelCrosshair;

            charBody.baseMaxHealth = baseHealth.Value;
            charBody.levelMaxHealth = healthGrowth.Value;
            charBody.baseRegen = baseRegen.Value;
            charBody.levelRegen = regenGrowth.Value;
            charBody.baseDamage = baseDamage.Value;
            charBody.levelDamage = damageGrowth.Value;
            charBody.baseArmor = baseArmor.Value;
            charBody.baseMoveSpeed = baseSpeed.Value;
            charBody.levelArmor = 0;
            charBody.baseCrit = 1;

            charBody.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/CharacterBodies/CrocoBody").GetComponent<CharacterBody>().preferredPodPrefab;


            //create a survivordef for our grovetender
            SurvivorDef survivorDef = new SurvivorDef
            {
                name = "AETXEL_NAME",
                unlockableName = "",
                descriptionToken = "AETXEL_DESCRIPTION",
                primaryColor = CHAR_COLOR,
                bodyPrefab = myCharacter,
                displayPrefab = characterDisplay,
                outroFlavorToken = "AETXEL_OUTRO_FLAVOR"
            };


            SurvivorAPI.AddSurvivor(survivorDef);


            SkillSetup();


            //add it to the body catalog
            BodyCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(myCharacter);
            };
        }

        private void RegisterStates()
        {
            LoadoutAPI.AddSkill(typeof(Scattergun));
            LoadoutAPI.AddSkill(typeof(Rifle));
            LoadoutAPI.AddSkill(typeof(Fireball));
            LoadoutAPI.AddSkill(typeof(Speed));
            LoadoutAPI.AddSkill(typeof(Bomb));
        }

        private void SkillSetup()
        {
            foreach (GenericSkill obj in myCharacter.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();
        }

        private void PrimarySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Fire a spread of bullets, dealing <style=cIsDamage>7x90% damage</style>.";

            LanguageAPI.Add("AETXEL_PRIMARY_SHOTGUN_NAME", "Scavenged Scattergun");
            LanguageAPI.Add("AETXEL_PRIMARY_SHOTGUN_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Scattergun));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 0f;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon1;
            mySkillDef.skillDescriptionToken = "AETXEL_PRIMARY_SHOTGUN_DESCRIPTION";
            mySkillDef.skillName = "AETXEL_PRIMARY_SHOTGUN_NAME";
            mySkillDef.skillNameToken = "AETXEL_PRIMARY_SHOTGUN_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);
            component.primary = myCharacter.AddComponent<GenericSkill>();
            SkillFamily skillFamily = ScriptableObject.CreateInstance<SkillFamily>();
            skillFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(skillFamily);
            Reflection.SetFieldValue<SkillFamily>(component.primary, "_skillFamily", skillFamily);
            SkillFamily skillFamily2 = component.primary.skillFamily;
            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
            LanguageAPI.Add("AETXEL_PRIMARY_RIFLE_NAME", "Scavenged Rifle");
            LanguageAPI.Add("AETXEL_PRIMARY_RIFLE_DESCRIPTION", "Fire a round of piercing bullets, dealing <style=cIsDamage>3x100%</style> damage.");
            SkillDef skillDef5 = ScriptableObject.CreateInstance<SkillDef>();
            skillDef5.activationState = new SerializableEntityStateType(typeof(Rifle));
            skillDef5.activationStateMachineName = "Weapon";
            skillDef5.baseMaxStock = 1;
            skillDef5.baseRechargeInterval = 0f;
            skillDef5.beginSkillCooldownOnSkillEnd = true;
            skillDef5.canceledFromSprinting = false;
            skillDef5.fullRestockOnAssign = true;
            skillDef5.interruptPriority = InterruptPriority.Any;
            skillDef5.isBullets = false;
            skillDef5.isCombatSkill = true;
            skillDef5.mustKeyPress = false;
            skillDef5.noSprint = true;
            skillDef5.rechargeStock = 1;
            skillDef5.requiredStock = 1;
            skillDef5.shootDelay = 0f;
            skillDef5.stockToConsume = 1;
            skillDef5.icon = Assets.icon1b;
            skillDef5.skillDescriptionToken = "AETXEL_PRIMARY_RIFLE_DESCRIPTION";
            skillDef5.skillName = "AETXEL_PRIMARY_RIFLE_NAME";
            skillDef5.skillNameToken = "AETXEL_PRIMARY_RIFLE_NAME";
            LoadoutAPI.AddSkillDef(skillDef5);
            Array.Resize<SkillFamily.Variant>(ref skillFamily2.variants, skillFamily2.variants.Length + 1);
            skillFamily2.variants[skillFamily2.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = skillDef5,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(skillDef5.skillNameToken, false, null)
            };
        }


        private void SecondarySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Vomit out a tornado of flame, dealing <style=cIsDamage>200% damage</style> and <style=cIsUtility>igniting enemies</style>.";

            LanguageAPI.Add("AETXEL_SECONDARY_FIREBALL_NAME", "Scorching Winds");
            LanguageAPI.Add("AETXEL_SECONDARY_FIREBALL_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Fireball));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 4f;
            mySkillDef.beginSkillCooldownOnSkillEnd = false;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Skill;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon2;
            mySkillDef.skillDescriptionToken = "AETXEL_SECONDARY_FIREBALL_DESCRIPTION";
            mySkillDef.skillName = "AETXEL_SECONDARY_FIREBALL_NAME";
            mySkillDef.skillNameToken = "AETXEL_SECONDARY_FIREBALL_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.secondary = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.secondary.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily2 = component.secondary.skillFamily;

            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }


        private void UtilitySetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Throw a hook forward, dealing <style=cIsDamage>500% damage</style> and <style=cIsUtility>pulling yourself</style> towards the location of the hook.";

            LanguageAPI.Add("AETXEL_UTILITY_HOOK_NAME", "Heated Hook");
            LanguageAPI.Add("AETXEL_UTILITY_HOOK_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Speed));
            mySkillDef.activationStateMachineName = "Body";
            mySkillDef.baseRechargeInterval = 7;
            mySkillDef.baseMaxStock = 1;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.Any;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = false;
            mySkillDef.mustKeyPress = false;
            mySkillDef.noSprint = false;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon3;
            mySkillDef.skillDescriptionToken = "AETXEL_UTILITY_HOOK_DESCRIPTION";
            mySkillDef.skillName = "AETXEL_UTILITY_HOOK_NAME";
            mySkillDef.skillNameToken = "AETXEL_UTILITY_HOOK_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.utility = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.utility.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily = component.utility.skillFamily;

            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        private void SpecialSetup()
        {
            SkillLocator component = myCharacter.GetComponent<SkillLocator>();

            string desc = "Quickly dig down into the ground, obscuring yourself from sight and increasing movement speed by 40%. After 3 seconds, emerge from the ground with a roaring flame, dealing 800% damage.";

            LanguageAPI.Add("AETXEL_SPECIAL_BURROW_NAME", "Molten Earth");
            LanguageAPI.Add("AETXEL_SPECIAL_BURROW_DESCRIPTION", desc);

            SkillDef mySkillDef = ScriptableObject.CreateInstance<SkillDef>();
            mySkillDef.activationState = new SerializableEntityStateType(typeof(Bomb));
            mySkillDef.activationStateMachineName = "Weapon";
            mySkillDef.baseMaxStock = 1;
            mySkillDef.baseRechargeInterval = 8;
            mySkillDef.beginSkillCooldownOnSkillEnd = true;
            mySkillDef.canceledFromSprinting = false;
            mySkillDef.fullRestockOnAssign = true;
            mySkillDef.interruptPriority = InterruptPriority.PrioritySkill;
            mySkillDef.isBullets = false;
            mySkillDef.isCombatSkill = true;
            mySkillDef.mustKeyPress = true;
            mySkillDef.noSprint = true;
            mySkillDef.rechargeStock = 1;
            mySkillDef.requiredStock = 1;
            mySkillDef.shootDelay = 0f;
            mySkillDef.stockToConsume = 1;
            mySkillDef.icon = Assets.icon4;
            mySkillDef.skillDescriptionToken = "AETXEL_SPECIAL_BURROW_DESCRIPTION";
            mySkillDef.skillName = "AETXEL_SPECIAL_BURROW_NAME";
            mySkillDef.skillNameToken = "AETXEL_SPECIAL_BURROW_NAME";

            LoadoutAPI.AddSkillDef(mySkillDef);

            component.special = myCharacter.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            LoadoutAPI.AddSkillFamily(newFamily);
            component.special.SetFieldValue("_skillFamily", newFamily);
            SkillFamily skillFamily2 = component.special.skillFamily;

            skillFamily2.variants[0] = new SkillFamily.Variant
            {
                skillDef = mySkillDef,
                unlockableName = "",
                viewableNode = new ViewablesCatalog.Node(mySkillDef.skillNameToken, false, null)
            };
        }

        private void CreateMaster()
        {
            //create the doppelganger, uses commando ai bc i can't be bothered writing my own
            doppelganger = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("Prefabs/CharacterMasters/MercMonsterMaster"), "AetxelMonsterMaster", true);

            MasterCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(doppelganger);
            };

            CharacterMaster component = doppelganger.GetComponent<CharacterMaster>();
            component.bodyPrefab = myCharacter;
        }


        public void RegisterProjectiles()
        {
            AetxelMod.Bomb = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/CommandoGrenadeProjectile"), "prefabs/projectiles/AetxelBomb", true, "C:Aetxel.cs", "RegisterProjectiles", 422);
            AetxelMod.Bomb.GetComponent<ProjectileImpactExplosion>().impactEffect = Resources.Load<GameObject>("prefabs/effects/omnieffect/OmniExplosionVFXDroneDeath");
            AetxelMod.Bomb.GetComponent<ProjectileImpactExplosion>().blastProcCoefficient = 1f;
            AetxelMod.Bomb.GetComponent<ProjectileImpactExplosion>().blastDamageCoefficient = 1f;
            AetxelMod.Bomb.GetComponent<ProjectileImpactExplosion>().blastRadius = 25f;
            AetxelMod.Bomb.GetComponent<ProjectileImpactExplosion>().falloffModel = BlastAttack.FalloffModel.Linear;
            AetxelMod.Bomb.GetComponent<SphereCollider>().transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
            AetxelMod.Bomb.AddComponent<NetworkIdentity>();
            PrefabAPI.RegisterNetworkPrefab(AetxelMod.Bomb, "C:Aetxel.cs", "Prefabs/Projectiles/AetxelBomb", 43);
            ProjectileCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(AetxelMod.Bomb);
            };
            AetxelMod.Tornado = PrefabAPI.InstantiateClone(Resources.Load<GameObject>("prefabs/projectiles/Fireball"), "prefabs/projectiles/AetxelFireball", true, "C:Aetxel.cs", "RegisterProjectiles", 422);
            var boomerangComponent = Tornado.GetComponent<BoomerangProjectile>();
            boomerangComponent.canHitWorld = false;
            boomerangComponent.impactSpark = null;
            boomerangComponent.travelSpeed = 80;

            AetxelMod.Tornado.GetComponent<SphereCollider>().transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            AetxelMod.Tornado.AddComponent<NetworkIdentity>();
            PrefabAPI.RegisterNetworkPrefab(AetxelMod.Tornado, "C:Aetxel.cs", "Prefabs/Projectiles/AetxelBomb", 43);
            ProjectileCatalog.getAdditionalEntries += delegate (List<GameObject> list)
            {
                list.Add(AetxelMod.Tornado);
            };
        }
        public class MenuAnim : MonoBehaviour
        {
            //animates him in character select
            internal void OnEnable()
            {
                bool flag = base.gameObject.transform.parent.gameObject.name == "CharacterPad";
                if (flag)
                {
                    base.StartCoroutine(this.SpawnAnim());
                }
            }

            private IEnumerator SpawnAnim()
            {
                Animator animator = base.GetComponentInChildren<Animator>();
                Transform effectTransform = base.gameObject.transform;

                ChildLocator component = base.gameObject.GetComponentInChildren<ChildLocator>();

                if (component) effectTransform = component.FindChild("Root");

                GameObject.Instantiate<GameObject>(EntityStates.HermitCrab.SpawnState.burrowPrefab, effectTransform.position, Quaternion.identity);


                PlayAnimation("Body", "Spawn", "Spawn.playbackRate", 3, animator);

                yield break;
            }


            private void PlayAnimation(string layerName, string animationStateName, string playbackRateParam, float duration, Animator animator)
            {
                int layerIndex = animator.GetLayerIndex(layerName);
                animator.SetFloat(playbackRateParam, 1f);
                animator.PlayInFixedTime(animationStateName, layerIndex, 0f);
                animator.Update(0f);
                float length = animator.GetCurrentAnimatorStateInfo(layerIndex).length;
                animator.SetFloat(playbackRateParam, length / duration);
            }
        }
    }


}