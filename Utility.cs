using System;
using RoR2;
using UnityEngine;
using EntityStates.LemurianBruiserMonster;
using EntityStates.LemurianMonster;
using EntityStates.LunarGolem;

namespace EntityStates.Aetxel.Weapon3
{
	// Token: 0x020009BE RID: 2494
	public class Speed : BaseState
	{
		// Token: 0x060039A7 RID: 14759 RVA: 0x000EC864 File Offset: 0x000EAA64
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = 0.01f / this.attackSpeedStat;
			Util.PlaySound(EntityStates.HermitCrab.FireMortar.mortarSoundString, base.gameObject);
			base.PlayAnimation("Gesture", "ChargeFireball", "ChargeFireball.playbackRate", 0.2f);
			base.characterBody.AddTimedBuff(BuffIndex.CloakSpeed, 3f);
			base.characterBody.AddTimedBuff(BuffIndex.Warbanner, 3f);
			EffectManager.SimpleMuzzleFlash(FireMegaFireball.muzzleflashEffectPrefab, base.gameObject, "MuzzleMouth", false);
		}

		// Token: 0x060039A8 RID: 14760 RVA: 0x000EC8C8 File Offset: 0x000EAAC8
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= Shell.preShieldAnimDuration && !this.readyToActivate)
			{
				this.readyToActivate = true;
				Util.PlaySound(FireFireball.attackString, base.gameObject);
			}
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		// Token: 0x060039A9 RID: 14761 RVA: 0x00032FA7 File Offset: 0x000311A7
		public override void OnExit()
		{
			base.OnExit();
		}

		// Token: 0x04003310 RID: 13072
		public static float baseDuration;

		// Token: 0x04003311 RID: 13073
		public static float buffDuration;

		// Token: 0x04003312 RID: 13074
		public static float preShieldAnimDuration;

		// Token: 0x04003313 RID: 13075
		public static GameObject preShieldEffect;

		// Token: 0x04003314 RID: 13076
		public static string preShieldSoundString;

		// Token: 0x04003315 RID: 13077
		public static string shieldActivateSoundString;

		// Token: 0x04003316 RID: 13078
		private bool readyToActivate;

		// Token: 0x04003317 RID: 13079
		private float duration;
	}
}