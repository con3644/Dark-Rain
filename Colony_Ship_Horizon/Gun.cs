using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colony_Ship_Horizon
{
    public struct Gun
    {
        public string _gunType;
        public int _damage;
        public int _force;
        public float _bulletSpeed;
        public bool _isAutomatic;
        public TimeSpan _rateOfFire;
        public Texture2D _gunTexture;
        public Texture2D _bulletTexture;
        public Texture2D _muzzleFlash;
        public string _gunShotAnim;
        public string _gunShotSound;
        public int _bulletSize;
        public float _bulletScale;
        public float _bulletSpread;
        public int _bulletCount;
        public float _reloadSpeed;
        public int _clipSize;
        public int _bulletsInClip;
        public int _penetrationPower;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gunType"></param>
        /// <param name="damage"></param>
        /// <param name="force"></param>
        /// <param name="bulletSpeed"></param>
        /// <param name="isAutomatic"></param>
        /// <param name="rateOfFire"></param>
        /// <param name="gunTexture"></param>
        /// <param name="bulletTexture"></param>
        /// <param name="muzzleFlash"></param>
        /// <param name="gunShotAnim"></param>
        /// <param name="gunShotSound"></param>
        /// <param name="bulletSize"></param>
        /// <param name="bulletScale"></param>
        /// <param name="bulletSpread"></param>
        /// <param name="bulletCount"></param>
        /// <param name="reloadSpeed"></param>
        /// <param name="clipSize">Total bullets gun can hold (bulletsInClip: is the amount of bullets left in current clip)</param>
        /// <param name="penetrationPower"></param>
        public Gun(string gunType,int damage, int force, float bulletSpeed, bool isAutomatic, TimeSpan rateOfFire, Texture2D gunTexture, Texture2D bulletTexture, Texture2D muzzleFlash,
            string gunShotAnim, string gunShotSound, int bulletSize, float bulletScale, float bulletSpread, int bulletCount, float reloadSpeed, 
            int clipSize, int penetrationPower)
        {
            _gunType = gunType;
            _damage = damage;
            _force = force;
            _bulletSpeed = bulletSpeed;
            _isAutomatic = isAutomatic;
            _rateOfFire = rateOfFire;
            _gunTexture = gunTexture;
            _bulletTexture = bulletTexture;
            _muzzleFlash = muzzleFlash;
            _gunShotAnim = gunShotAnim;
            _gunShotSound = gunShotSound;
            _bulletSize = bulletSize;
            _bulletScale = bulletScale;
            _bulletSpread = bulletSpread;
            _bulletCount = bulletCount;
            _reloadSpeed = reloadSpeed;
            _clipSize = clipSize;
            _penetrationPower = penetrationPower;

            // give gun a full clip of ammo
            _bulletsInClip = clipSize;
        }
    }
}
