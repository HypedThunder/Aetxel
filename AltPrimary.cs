using System;
using RoR2;
using UnityEngine;
using EntityStates.Commando.CommandoWeapon;
using EntityStates.Aetxel.Weapon;

namespace EntityStates.Aetxel.Weapon5
{
	// Token: 0x02000AAF RID: 2735
	public class Rifle : BaseState
	{
		// Token: 0x06003E45 RID: 15941 RVA: 0x001040B8 File Offset: 0x001022B8
		public override void OnEnter()
		{
			base.OnEnter();
			base.characterBody.SetSpreadBloom(0.2f, false);
			this.duration = FireBarrage.totalDuration;
			this.durationBetweenShots = FireBarrage.baseDurationBetweenShots / this.attackSpeedStat;
			this.bulletCount = 3;
			this.modelAnimator = base.GetModelAnimator();
			this.modelTransform = base.GetModelTransform();
			if (base.characterBody)
			{
				base.characterBody.SetAimTimer(2f);
			}
			this.FireBullet();
		}

		// Token: 0x06003E46 RID: 15942 RVA: 0x00104188 File Offset: 0x00102388
		private void FireBullet()
		{
			Ray aimRay = base.GetAimRay();
			string muzzleName = "MuzzleCenter";
			if (this.modelAnimator)
			{
				if (FireBarrage.effectPrefab)
				{
					EffectManager.SimpleMuzzleFlash(FireBarrage.effectPrefab, base.gameObject, muzzleName, false);
				}
				base.PlayAnimation("Gesture", "FireFireball", "FireFireball.playbackRate", 1f);
			}
			base.characterBody.AddSpreadBloom(FireShotgun.spreadBloomValue);
			if (base.isAuthority)
			{
				new BulletAttack
				{
					owner = base.gameObject,
					weapon = base.gameObject,
					origin = aimRay.origin,
					aimVector = aimRay.direction,
					minSpread = 0f,
					maxSpread = 0f,
					bulletCount = 1,
					damage = 1f * this.damageStat,
					force = FireBarrage.force,
					tracerEffectPrefab = FireShotgun.tracerEffectPrefab,
					muzzleName = muzzleName,
					hitEffectPrefab = FireShotgun.effectPrefab,
					stopperMask = LayerIndex.world.collisionMask,
					isCrit = Util.CheckRoll(this.critStat, base.characterBody.master),
					radius = 0.8f,
					smartCollision = true,
					damageType = DamageType.Generic
				}.Fire();
			}
			base.characterBody.AddSpreadBloom(FireBarrage.spreadBloomValue);
			this.totalBulletsFired++;
			Util.PlaySound(FireBarrage.fireBarrageSoundString, base.gameObject);
		}

		// Token: 0x06003E47 RID: 15943 RVA: 0x00032FA7 File Offset: 0x000311A7
		public override void OnExit()
		{
			base.OnExit();
		}

		// Token: 0x06003E48 RID: 15944 RVA: 0x00104314 File Offset: 0x00102514
		public override void FixedUpdate()
		{
			base.FixedUpdate();
			this.stopwatchBetweenShots += Time.fixedDeltaTime;
			if (this.stopwatchBetweenShots >= this.durationBetweenShots && this.totalBulletsFired < this.bulletCount)
			{
				this.stopwatchBetweenShots -= this.durationBetweenShots;
				this.FireBullet();
			}
			if (base.fixedAge >= this.duration && this.totalBulletsFired == this.bulletCount && base.isAuthority)
			{
				this.outer.SetNextStateToMain();
				return;
			}
		}

		// Token: 0x06003E49 RID: 15945 RVA: 0x0000CFF7 File Offset: 0x0000B1F7
		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.Skill;
		}

		// Token: 0x04003972 RID: 14706
		public static GameObject effectPrefab;

		// Token: 0x04003973 RID: 14707
		public static GameObject hitEffectPrefab;

		// Token: 0x04003974 RID: 14708
		public static GameObject tracerEffectPrefab;

		// Token: 0x04003975 RID: 14709
		public static float damageCoefficient;

		// Token: 0x04003976 RID: 14710
		public static float force;

		// Token: 0x04003977 RID: 14711
		public static float minSpread;

		// Token: 0x04003978 RID: 14712
		public static float maxSpread;

		// Token: 0x04003979 RID: 14713
		public static float baseDurationBetweenShots = 0.5f;

		// Token: 0x0400397A RID: 14714
		public static float totalDuration = 1f;

		// Token: 0x0400397B RID: 14715
		public static float bulletRadius = 1.5f;

		// Token: 0x0400397C RID: 14716
		public static int baseBulletCount = 3;

		// Token: 0x0400397D RID: 14717
		public static string fireBarrageSoundString;

		// Token: 0x0400397E RID: 14718
		public static float recoilAmplitude;

		// Token: 0x0400397F RID: 14719
		public static float spreadBloomValue;

		// Token: 0x04003980 RID: 14720
		private int totalBulletsFired;

		// Token: 0x04003981 RID: 14721
		private int bulletCount;

		// Token: 0x04003982 RID: 14722
		public float stopwatchBetweenShots;

		// Token: 0x04003983 RID: 14723
		private Animator modelAnimator;

		// Token: 0x04003984 RID: 14724
		private Transform modelTransform;

		// Token: 0x04003985 RID: 14725
		private float duration;

		// Token: 0x04003986 RID: 14726
		private float durationBetweenShots;
	}
}