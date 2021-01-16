using System;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using EntityStates.LemurianBruiserMonster;
using Aetxel;

namespace EntityStates.Aetxel.Weapon6
{
	// Token: 0x02000AB3 RID: 2739
	public class Bomb : BaseState
	{
		// Token: 0x06003E59 RID: 15961 RVA: 0x0010473C File Offset: 0x0010293C
		public override void OnEnter()
		{
			base.OnEnter();
			this.duration = 1.3f / this.attackSpeedStat;
			Ray aimRay = base.GetAimRay();
			base.StartAimMode(aimRay, 2f, false);
			base.PlayAnimation("Gesture", "Bite", "Bite.playbackRate", this.duration);
			string muzzleName = "MuzzleMouth";
			Util.PlaySound(EntityStates.RoboBallBoss.Weapon.FireSuperEyeblast.attackString, base.gameObject);
			if (FireMegaFireball.muzzleflashEffectPrefab)
			{
				EffectManager.SimpleMuzzleFlash(FireMegaFireball.muzzleflashEffectPrefab, base.gameObject, muzzleName, false);
			}
			if (base.isAuthority)
			{
				ProjectileManager.instance.FireProjectile(AetxelMod.Bomb, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, this.damageStat * 12f, 0f, Util.CheckRoll(this.critStat, base.characterBody.master), DamageColorIndex.Default, null, -1f);
			}
		}

		// Token: 0x06003E5A RID: 15962 RVA: 0x00032FA7 File Offset: 0x000311A7
		public override void OnExit()
		{
			base.OnExit();
		}

		// Token: 0x06003E5B RID: 15963 RVA: 0x00104861 File Offset: 0x00102A61
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			if (base.fixedAge >= this.duration && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		// Token: 0x06003E5C RID: 15964 RVA: 0x0000CFF7 File Offset: 0x0000B1F7
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		// Token: 0x04003997 RID: 14743
		public static GameObject effectPrefab;

		// Token: 0x04003998 RID: 14744
		public static GameObject projectilePrefab;

		// Token: 0x04003999 RID: 14745
		public static float damageCoefficient;

		// Token: 0x0400399A RID: 14746
		public static float force;

		// Token: 0x0400399B RID: 14747
		public static float selfForce;

		// Token: 0x0400399C RID: 14748
		public static float baseDuration = 2f;

		// Token: 0x0400399D RID: 14749
		private float duration;

		// Token: 0x0400399E RID: 14750
		public int bulletCountCurrent = 1;
	}
}