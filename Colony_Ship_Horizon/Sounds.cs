using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace Colony_Ship_Horizon
{
    public class Sounds
    {
        public SoundEffect incomingCallRing;

        public SoundEffect menuOpen;
        public SoundEffect menuClosed;
        public SoundEffect menuBuilt;
        public SoundEffect menuSelect;

        public SoundEffect gunShot;
        public SoundEffect lazerGunShot;
        public SoundEffect blasterShot;
        public SoundEffect blasterShot2;
        public SoundEffect blasterReload;
        public SoundEffect blasterReload2;
        public SoundEffect sniperShot;
        public SoundEffect shotgunShot;
        public SoundEffect shotgunPump;
        public SoundEffect pulseGunShot;

        public SoundEffect bulletPing;

        public SoundEffect doorSound;
        public SoundEffect leavesRustle;
        public SoundEffect waterPour;

        public SoundEffect playerWalk;
        public SoundEffect playerClimb;

        public SoundEffect gutCrunch;
        public SoundEffect alienWalk;
        public SoundEffect alienAttack;

        // ---------------------------map noise--------------
        //starship
        public SoundEffect starshipAmbience;
        public SoundEffect starshipJump;
        public SoundEffect starshipArrival;
        public SoundEffect starshipLanding;
        //other
        public SoundEffect rainAmbience;
        public SoundEffect barAmbience;

        public Song engineRoomSong;
        public Song gardenRoomSong;
        public Song mainRoomSong;
        public Song menuSong;
        public Song attackSong1;
        public Song attackSong2;

        private List<SoundEffectInstance> incomingCallRingPool;

        private List<SoundEffectInstance> menuOpenPool;
        private List<SoundEffectInstance> menuClosedPool;
        private List<SoundEffectInstance> menuBuiltPool;
        private List<SoundEffectInstance> menuSelectPool;

        private List<SoundEffectInstance> doorSoundPool;
        private List<SoundEffectInstance> leavesRustlePool;
        private List<SoundEffectInstance> waterPoorPool;

        private List<SoundEffectInstance> gunShotPool;
        private List<SoundEffectInstance> gunReloadPool;
        private List<SoundEffectInstance> blasterShotPool;
        private List<SoundEffectInstance> blasterShot2Pool;
        private List<SoundEffectInstance> sniperShotPool;
        private List<SoundEffectInstance> shotgunShotPool;
        private List<SoundEffectInstance> shotgunPumpPool;
        private List<SoundEffectInstance> pulseGunShotPool;
        private List<SoundEffectInstance> lazerGunShotPool;

        private List<SoundEffectInstance> bulletPingPool;

        private List<SoundEffectInstance> playerWalkPool;

        private List<SoundEffectInstance> gutCrunchPool;
        private List<SoundEffectInstance> alienWalkPool;
        private List<SoundEffectInstance> alienAttackPool;

        // map noise
        private List<SoundEffectInstance> rainPool;
        private List<SoundEffectInstance> starshipPool;
        private List<SoundEffectInstance> barPool;

        private Random random = new Random();
        public string previousType = "";
        public string previousAmbientSound = "";
        private float volumeFadeInterval;
        private TimeSpan totalTime;
        private TimeSpan previousTime;
        public bool isTransitioningSongs;

        public Sounds()
        {
            totalTime = TimeSpan.FromMilliseconds(400f);
            previousTime = TimeSpan.FromMilliseconds(0);

            incomingCallRingPool = new List<SoundEffectInstance>();

            menuOpenPool = new List<SoundEffectInstance>();
            menuClosedPool = new List<SoundEffectInstance>();
            menuBuiltPool = new List<SoundEffectInstance>();
            menuSelectPool = new List<SoundEffectInstance>();

            leavesRustlePool = new List<SoundEffectInstance>();
            waterPoorPool = new List<SoundEffectInstance>();
            doorSoundPool = new List<SoundEffectInstance>();

            gunShotPool = new List<SoundEffectInstance>();
            gunReloadPool = new List<SoundEffectInstance>();
            lazerGunShotPool = new List<SoundEffectInstance>();
            pulseGunShotPool = new List<SoundEffectInstance>();
            blasterShotPool = new List<SoundEffectInstance>();
            blasterShot2Pool = new List<SoundEffectInstance>();
            sniperShotPool = new List<SoundEffectInstance>();
            shotgunPumpPool = new List<SoundEffectInstance>();
            shotgunShotPool = new List<SoundEffectInstance>();

            bulletPingPool = new List<SoundEffectInstance>();

            playerWalkPool = new List<SoundEffectInstance>();

            gutCrunchPool = new List<SoundEffectInstance>();
            alienAttackPool = new List<SoundEffectInstance>();
            alienWalkPool = new List<SoundEffectInstance>();

            rainPool = new List<SoundEffectInstance>();
            starshipPool = new List<SoundEffectInstance>();
            barPool = new List<SoundEffectInstance>();
        }

        public void Load()
        {
            // misc. sound effect pools
            for (int i = 0; i < 5; i++)
                leavesRustlePool.Add(leavesRustle.CreateInstance());
            for (int i = 0; i < 5; i++)
                waterPoorPool.Add(waterPour.CreateInstance());
            for (int i = 0; i < 6; i++)
                doorSoundPool.Add(doorSound.CreateInstance());
            
            for (int i = 0; i < 1; i++)
                incomingCallRingPool.Add(incomingCallRing.CreateInstance());

            // menu sound effects
            for (int i = 0; i < 1; i++)
                menuOpenPool.Add(menuOpen.CreateInstance());
            for (int i = 0; i < 6; i++)
                menuClosedPool.Add(menuClosed.CreateInstance());
            for (int i = 0; i < 1; i++)
                menuSelectPool.Add(menuSelect.CreateInstance());
            for (int i = 0; i < 2; i++)
                menuBuiltPool.Add(menuBuilt.CreateInstance());

            // weapon shot sound effects
            for (int i = 0; i < 20; i++)
            {
                gunShotPool.Add(gunShot.CreateInstance());
                gunShotPool[i].Volume = .15f;
            }
            for (int i = 0; i < 1; i++)
            {
                gunReloadPool.Add(blasterReload.CreateInstance());
                gunReloadPool.Add(blasterReload2.CreateInstance());
                gunReloadPool[0].Volume = .5f;
                gunReloadPool[1].Volume = .5f;
            }
            for (int i = 0; i < 20; i++)
                blasterShotPool.Add(blasterShot.CreateInstance());
            for (int i = 0; i < 20; i++)
                blasterShot2Pool.Add(blasterShot2.CreateInstance());
            for (int i = 0; i < 20; i++)
                sniperShotPool.Add(sniperShot.CreateInstance());
            for (int i = 0; i < 20; i++)
                shotgunShotPool.Add(shotgunShot.CreateInstance());
            for (int i = 0; i < 20; i++)
                shotgunPumpPool.Add(shotgunPump.CreateInstance());
            for (int i = 0; i < 20; i++)
                pulseGunShotPool.Add(pulseGunShot.CreateInstance());
            for (int i = 0; i < 20; i++)
                lazerGunShotPool.Add(lazerGunShot.CreateInstance());

            // weapon impact sound effects
            for (int i = 0; i < 20; i++)
                bulletPingPool.Add(bulletPing.CreateInstance());

            // player
            for (int i = 0; i < 1; i++)
                playerWalkPool.Add(playerWalk.CreateInstance());

            // alien sound effects
            for (int i = 0; i < 25; i++)
                alienAttackPool.Add(alienAttack.CreateInstance());
            for (int i = 0; i < 25; i++)
                alienWalkPool.Add(alienWalk.CreateInstance());
            for (int i = 0; i < 5; i++)
            {
                gutCrunchPool.Add(gutCrunch.CreateInstance());
                gutCrunchPool[i].Volume = .66f;
            }

            // map noise effects
            for (int i = 0; i < 1; i++)
                starshipPool.Add(starshipAmbience.CreateInstance());
            for (int i = 0; i < 1; i++)
                starshipPool.Add(starshipJump.CreateInstance());
            for (int i = 0; i < 1; i++)
                starshipPool.Add(starshipArrival.CreateInstance());
            for (int i = 0; i < 1; i++)
                starshipPool.Add(starshipLanding.CreateInstance());
            for (int i = 0; i < 1; i++)
                rainPool.Add(rainAmbience.CreateInstance());
            for (int i = 0; i < 1; i++)
                barPool.Add(barAmbience.CreateInstance());
        }

        /// <summary>
        /// Updates the sound using the given type to play or stop songs or sounds
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="type">The type of sound or song to play. Input "stopSong" to stop playing all songs or "stopSound"
        /// to stop playing all sounds.</param>
        /// <param name="inCombat">Is the player currently in combat?</param>
        public void Update(GameTime gameTime, string type, string currentMapName, bool stopNpcIndex = false, int npcIndex = -1, bool inCombat = false)
        {
            float randomNumber = (float)random.NextDouble();

            switch (type)
            {
                // MAP NOISE
                case "rainAmbience":
                    {
                        // don't play the rain ambience until the landing sound is complete
                        if (!IsSoundEffectPlaying())
                        {
                            rainPool[0].Volume = .3f;
                            rainPool[0].Play();
                            rainPool[0].IsLooped = true;
                        }
                        break;
                    }
                case "starshipAmbience":
                    {
                        if (currentMapName == "personalStarship" || currentMapName.Contains("playerRoom"))
                        {
                            starshipPool[0].Volume = .3f;
                            starshipPool[0].Play();
                            starshipPool[0].IsLooped = true;
                        }
                        break;
                    }
                case "starshipJump":
                    {
                        //starshipPool[1].Volume = .3f;
                        starshipPool[1].Play();
                        break;
                    }
                case "starshipArrival":
                    {
                        //starshipPool[2].Volume = .3f;
                        starshipPool[2].Play();
                        break;
                    }
                case "starshipLanding":
                    {
                        //starshipPool[3].Volume = .3f;
                        starshipPool[3].Play();
                        break;
                    }
                case "barAmbience":
                    {
                        barPool[0].Volume = .2f;
                        barPool[0].Play();
                        barPool[0].IsLooped = true;
                        break;
                    }
                    // stop all sounds by stopping all instances of sound in each pool
                case "stopSound":
                    {
                        rainPool[0].Stop();
                        foreach (SoundEffectInstance effect in starshipPool)
                            effect.Stop();
                        barPool[0].Stop();
                        foreach (SoundEffectInstance effect in alienWalkPool)
                            effect.Stop();
                        break;
                    }
                // SONGS
                case "stopSong":
                    {
                        MediaPlayer.Stop();
                        break;
                    }
                case "attackSong2":
                    {
                        if (previousType != "attackSong2") // don't play if it's already playing
                        {
                            volumeFadeInterval = 1f;
                            // if the song has changed, stop the previous one
                            if (previousType != type)
                                MediaPlayer.Stop();
                            MediaPlayer.Volume = 1f;
                            MediaPlayer.Play(attackSong2);
                            MediaPlayer.IsRepeating = true;
                        }
                        break;
                    }
                case "attackSong":
                    {
                        if (previousType != "attackSong") // don't play if it's already playing
                        {
                            volumeFadeInterval = 1f;
                            // if the song has changed, stop the previous one
                            if (previousType != type)
                                MediaPlayer.Stop();
                            MediaPlayer.Volume = 1f;
                            MediaPlayer.Play(attackSong1);
                            MediaPlayer.IsRepeating = true;
                        }
                        break;
                    }
                case "transitionSong":
                    {
                        if (previousType != "transitionSong") // don't play if it's already playing
                        {
                            volumeFadeInterval = 1f;
                            // if the song has changed, stop the previous one
                            if (previousType != type)
                                MediaPlayer.Stop();
                            MediaPlayer.Volume = 1f;
                            MediaPlayer.Play(attackSong1);
                            MediaPlayer.IsRepeating = true;
                        }
                        break;
                    }
                case "transitionBetweenSongs":
                    {
                        if (MediaPlayer.State == MediaState.Playing)
                        {
                            isTransitioningSongs = true;
                            if (gameTime.TotalGameTime - previousTime > totalTime && volumeFadeInterval >= 0f)
                            {
                                previousTime = gameTime.TotalGameTime;
                                MediaPlayer.Volume = volumeFadeInterval;
                                volumeFadeInterval -= .15f;
                            }
                            else if (volumeFadeInterval <= 0f)
                            {
                                MediaPlayer.Stop();
                                isTransitioningSongs = false;
                            }
                        }
                        break;
                    }
                case "engineRoomSong":
                    {
                        if (!isTransitioningSongs)
                        {
                            // reset the interval used to fade a song/sound out
                            volumeFadeInterval = 1f;
                            if (previousType != type)
                                MediaPlayer.Stop();
                            MediaPlayer.Volume = 1f;
                            MediaPlayer.Play(engineRoomSong);
                            MediaPlayer.IsRepeating = true;
                        }
                        break;
                    }
                case "gardenRoomSong":
                    {
                        if (!isTransitioningSongs)
                        {
                            volumeFadeInterval = 1f;
                            if (previousType != type)
                                MediaPlayer.Stop();
                            MediaPlayer.Volume = 1f;
                            MediaPlayer.Play(gardenRoomSong);
                            MediaPlayer.IsRepeating = true;
                        }
                        break;
                    }
                case "mainRoomSong":
                    {
                        if (!isTransitioningSongs)
                        {
                            volumeFadeInterval = 1f;
                            if (previousType != type)
                                MediaPlayer.Stop();
                            MediaPlayer.Volume = 1f;
                            MediaPlayer.Play(mainRoomSong);
                            MediaPlayer.IsRepeating = true;
                        }
                        break;
                    }
                case "menuSong":
                    {
                        MediaPlayer.Volume = 1f;
                        MediaPlayer.Play(menuSong);
                        MediaPlayer.IsRepeating = true;
                        break;
                    }

                    // MISC
                case "door":
                    {
                        for (int i = 0; i < doorSoundPool.Count; i++)
                        {
                            if (doorSoundPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                doorSoundPool[i].Volume = .25f;
                                doorSoundPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "waterPour":
                    {
                        for (int i = 0; i < waterPoorPool.Count; i++)
                        {
                            if (waterPoorPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                //leavesRustlePool[i].Volume = .25f;
                                waterPoorPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "itemPickUp":
                    {
                        menuBuiltPool[1].Pitch = .25f;
                        menuBuiltPool[1].Play();
                        menuBuiltPool[1].IsLooped = false;
                        break;
                    }
                case "plantHarvest":
                    {
                        for (int i = 0; i < leavesRustlePool.Count; i++)
                        {
                            if (leavesRustlePool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                leavesRustlePool[i].Volume = .5f;
                                leavesRustlePool[i].Play();
                                break;
                            }
                        }
                        break;
                    }

                // ALIEN
                case "gutCrunch":
                    {
                        for (int i = 0; i < gutCrunchPool.Count; i++)
                        {
                            gutCrunchPool[i].Pitch = randomNumber;
                            gutCrunchPool[i].IsLooped = false;
                            if (gutCrunchPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                gutCrunchPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "alienWalk":
                    {
                        if (npcIndex >= 0)
                        {
                            alienWalkPool[npcIndex].Pitch = randomNumber;
                            alienWalkPool[npcIndex].IsLooped = false;
                            alienWalkPool[npcIndex].Volume = .5f;
                            if (!stopNpcIndex)
                            {
                                if (alienWalkPool[npcIndex].State == SoundState.Stopped) // use inactive sounds from pool
                                {
                                    alienWalkPool[npcIndex].Play();
                                }
                            }
                            else
                                alienWalkPool[npcIndex].Stop();
                        }
                        break;
                    }
                case "alienAttack":
                    {
                        if (npcIndex >= 0)
                        {
                            alienAttackPool[npcIndex].Pitch = randomNumber;
                            alienAttackPool[npcIndex].IsLooped = false;
                            alienAttackPool[npcIndex].Volume = .5f;
                            if (!stopNpcIndex)
                            {
                                if (alienAttackPool[npcIndex].State == SoundState.Stopped) // use inactive sounds from pool
                                {
                                    alienAttackPool[npcIndex].Play();
                                }
                            }
                            else
                                alienAttackPool[npcIndex].Stop();
                        }
                        break;
                    }
                // PLAYER
                case "playerWalk":
                    {
                        //playerWalkPool[0].Pitch = randomNumber;
                        playerWalkPool[0].Volume = .5f;
                        playerWalkPool[0].IsLooped = true;
                        playerWalkPool[0].Play();
                        break;
                    }
                case "playerStationary":
                    {
                        playerWalkPool[0].Stop();
                        break;
                    }

                // WEAPON
                case "blasterReload":
                    {
                        if (gunReloadPool[0].State == SoundState.Stopped) // use inactive sounds from pool
                        {
                            gunReloadPool[0].IsLooped = false;
                            //gunReloadPool[0].Pitch = randomNumber;
                            gunReloadPool[0].Play();
                            break;
                        }
                        break;
                    }
                case "blaster2Reload":
                    {
                        if (gunReloadPool[1].State == SoundState.Stopped) // use inactive sounds from pool
                        {
                            gunReloadPool[1].IsLooped = false;
                            //gunReloadPool[0].Pitch = randomNumber;
                            gunReloadPool[1].Play();
                            break;
                        }
                        break;
                    }
                case "gunShot":
                    {
                        for (int i = 0; i < gunShotPool.Count; i++)
                        {
                            if (gunShotPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                gunShotPool[i].IsLooped = false;
                                gunShotPool[i].Pitch = randomNumber;
                                gunShotPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "sniperShot":
                    {
                        for (int i = 0; i < sniperShotPool.Count; i++)
                        {
                            if (sniperShotPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                sniperShotPool[i].IsLooped = false;
                                sniperShotPool[i].Pitch = .5f;
                                sniperShotPool[i].Volume = .35f;
                                sniperShotPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "shotgunShot":
                    {
                        for (int i = 0; i < shotgunShotPool.Count; i++)
                        {
                            if (shotgunShotPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                shotgunShotPool[i].IsLooped = false;
                                shotgunShotPool[i].Pitch = randomNumber;
                                shotgunShotPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "shotgunPump":
                    {
                        for (int i = 0; i < shotgunPumpPool.Count; i++)
                        {
                            if (shotgunPumpPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                shotgunPumpPool[i].IsLooped = false;
                                //shotgunPumpPool[i].Pitch = randomNumber;
                                shotgunPumpPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "blasterShot":
                    {
                        for (int i = 0; i < blasterShotPool.Count; i++)
                        {
                            if (blasterShotPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                blasterShotPool[i].Volume = .5f;
                                blasterShotPool[i].IsLooped = false;
                                //blasterShotPool[i].Pitch = randomNumber;
                                blasterShotPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "blasterShot2":
                    {
                        for (int i = 0; i < blasterShot2Pool.Count; i++)
                        {
                            if (blasterShot2Pool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                blasterShot2Pool[i].Volume = .35f;
                                blasterShot2Pool[i].IsLooped = false;
                                //blasterShot2Pool[i].Pitch = randomNumber;
                                blasterShot2Pool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "pulseGunShot":
                    {
                        for (int i = 0; i < pulseGunShotPool.Count; i++)
                        {
                            if (pulseGunShotPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                pulseGunShotPool[i].IsLooped = false;
                                pulseGunShotPool[i].Pitch = randomNumber;
                                pulseGunShotPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "lazerGunShot":
                    {
                        for (int i = 0; i < lazerGunShotPool.Count; i++)
                        {
                            if (lazerGunShotPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                lazerGunShotPool[i].IsLooped = false;
                                lazerGunShotPool[i].Pitch = randomNumber;
                                lazerGunShotPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                    // ring the player's phone
                case "incomingCallRing":
                    {
                        for (int i = 0; i < incomingCallRingPool.Count; i++)
                        {
                            incomingCallRingPool[i].IsLooped = false;
                            if (incomingCallRingPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                incomingCallRingPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                    // player answered the phone, stop ringing
                case "incomingCallAnswered":
                    {
                        for (int i = 0; i < incomingCallRingPool.Count; i++)
                        {
                            incomingCallRingPool[i].IsLooped = false;
                            if (incomingCallRingPool[i].State == SoundState.Playing) // use inactive sounds from pool
                            {
                                incomingCallRingPool[i].Stop();
                                break;
                            }
                        }
                        break;
                    }

                // MENU
                case "menuOpen":
                    {
                        for (int i = 0; i < menuOpenPool.Count; i++)
                        {
                            menuOpenPool[i].IsLooped = false;
                            if (menuOpenPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                menuOpenPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "menuClosed":
                    {
                        for (int i = 0; i < menuClosedPool.Count; i++)
                        {
                            menuClosedPool[i].IsLooped = false;
                            if (menuClosedPool[i].State == SoundState.Stopped) // use inactive sounds from pool
                            {
                                menuClosedPool[i].Play();
                                break;
                            }
                        }
                        break;
                    }
                case "menuSelect":
                    {
                        menuSelectPool[0].Play();
                        menuSelectPool[0].IsLooped = false;
                        break;
                    }
                case "menuBuilt":
                    {
                        menuBuiltPool[0].Play();
                        menuBuiltPool[0].IsLooped = false;
                        break;
                    }
                default:
                    break;
            }
            if (type != null)
            {
                // if the current type is a song, record it
                if (type.Contains("Song") && type != "stopSong" && type != "transitionBetweenSongs")
                    previousType = type;
                if (type.Contains("Ambience"))
                    previousAmbientSound = type;
            }
        }

        // adjust total game volume
        public void AdjustVolume()
        {
            //if (SoundEffect.MasterVolume == 0.0f)
            //    SoundEffect.MasterVolume = 1.0f;
            //else
            //    SoundEffect.MasterVolume = 0.0f;

            if (MediaPlayer.Volume == 0.0f)
                MediaPlayer.Volume = 1.0f;
            else
                MediaPlayer.Volume = 0.0f;
        }

        public bool IsSoundEffectPlaying()
        {
            bool isPlaying = false;

            if (starshipPool[3].State == SoundState.Playing)
                isPlaying = true;

            return isPlaying;
        }
    }
}