using Intersect.Client.Core.Sounds;
using Intersect.Client.Entities;
using Intersect.Client.Framework.Audio;
using Intersect.Client.Framework.Database;
using Intersect.Client.General;
using System;
using System.Collections.Generic;
using UE = UnityEngine;

namespace Intersect.Client.Core
{

    public static class Audio
    {

        private static string sCurrentSong = string.Empty;

        private static float sFadeRate;

        private static long sFadeTimer;

        private static bool sFadingOut;

        //Sounds
        private static List<Sound> sGameSounds = new List<Sound>();

        private static bool sIsInitialized;

        private static float sQueuedFade;

        private static bool sQueuedLoop;

        //Music
        private static string sQueuedMusic = string.Empty;

        private static UE.AudioSource musicSource;
        private static UE.Audio.AudioMixer audioMixer;

        //Init
        public static void Init(UE.AudioSource musicSource, UE.Audio.AudioMixer audioMixer)
        {
            if (sIsInitialized == true)
            {
                return;
            }

            if (musicSource is null)
            {
                throw new ArgumentNullException(nameof(musicSource));
            }

            Audio.musicSource = musicSource;
            Audio.audioMixer = audioMixer;
            Globals.ContentManager.LoadAudio();
            sIsInitialized = true;
        }

        internal static void UpdateGlobalVolume()
        {
            audioMixer.SetFloat("Music", (float)Math.Log10(Globals.Database.MusicVolume / 100f + 0.0001f) * 20f);
            audioMixer.SetFloat("Sound", (float)Math.Log10(Globals.Database.SoundVolume / 100f + 0.0001f) * 20f);
        }

        public static void Update()
        {
            if (musicSource != null)
            {
                if (sFadeTimer != 0 && sFadeTimer < Globals.System.GetTimeMs())
                {
                    if (sFadingOut)
                    {
                        musicSource.volume -= .01f;
                        if (musicSource.volume <= .01f)
                        {
                            StopMusic();
                            PlayMusic(sQueuedMusic, 0f, sQueuedFade, sQueuedLoop);
                        }
                        else
                        {
                            sFadeTimer = Globals.System.GetTimeMs() + (long)(sFadeRate / 1000);
                        }
                    }
                    else
                    {
                        musicSource.volume += .01f;
                        if (musicSource.volume < 1f)
                        {
                            sFadeTimer = Globals.System.GetTimeMs() + (long)(sFadeRate / 1000);
                        }
                        else
                        {
                            sFadeTimer = 0;
                        }
                    }
                }
            }

            for (int i = 0; i < sGameSounds.Count; i++)
            {
                sGameSounds[i].Update();
                if (!sGameSounds[i].Loaded)
                {
                    sGameSounds.RemoveAt(i);
                }
            }
        }

        //Music
        public static void PlayMusic(string filename, float fadeout = 0f, float fadein = 0f, bool loop = false)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                //Entered a map with no music selected, fade out any music that's already playing.
                StopMusic(3f);

                return;
            }

            ClearQueue();

            if (musicSource != null)
            {
                if (fadeout < .01f ||
                    !musicSource.isPlaying ||
                    musicSource.volume <= .01f)
                {
                    StopMusic();
                    StartMusic(filename, fadein, loop);
                }
                else
                {
                    //Start fadeout
                    if (!string.Equals(sCurrentSong, filename, StringComparison.CurrentCultureIgnoreCase) || sFadingOut)
                    {
                        sFadeRate = musicSource.volume / (fadeout / 100f);
                        sFadeTimer = Globals.System.GetTimeMs() + (long)(sFadeRate / 1000);
                        sFadingOut = true;
                        sQueuedMusic = filename;
                        sQueuedFade = fadein;
                        sQueuedLoop = loop;
                    }
                }
            }
            else
            {
                StartMusic(filename, fadein, loop);
            }
        }

        private static void ClearQueue()
        {
            sQueuedMusic = null;
            sQueuedLoop = false;
            sQueuedFade = -1;
        }

        private static void StartMusic(string filename, float fadein = 0f, bool loop = false)
        {
            GameAudioSource music = Globals.ContentManager.GetMusic(filename);
            if (music == null)
            {
                return;
            }

            sCurrentSong = filename;
            musicSource.clip = music.Clip;
            musicSource.volume = 0f;
            musicSource.Play();
            musicSource.loop = loop;
            sFadeRate = 100 / fadein;
            sFadeTimer = Globals.System.GetTimeMs() + (long)(sFadeRate / 1000) + 1;
            sFadingOut = false;
        }

        public static void StopMusic(float fadeout = 0f)
        {
            if (musicSource == null)
            {
                return;
            }

            if (Math.Abs(fadeout) < .01f || !musicSource.isPlaying || musicSource.volume <= .01f)
            {
                sCurrentSong = string.Empty;
                musicSource.Stop();
                sFadeTimer = 0;
            }
            else
            {
                //Start fadeout
                sFadeRate = musicSource.volume / (fadeout / 100f);
                sFadeTimer = Globals.System.GetTimeMs() + (long)(sFadeRate / 1000);
                sFadingOut = true;
            }
        }

        //Sounds
        public static MapSound AddMapSound(string filename, int x, int y, Guid mapId, bool loop, int loopInterval, int distance, Entity parent = null)
        {
            if (sGameSounds?.Count > 128 || string.IsNullOrWhiteSpace(filename))
            {
                return null;
            }

            MapSound sound = new MapSound(filename, x, y, mapId, loop, loopInterval, distance, parent);
            sGameSounds?.Add(sound);

            return sound;
        }

        public static Sound AddGameSound(string filename, bool loop)
        {
            if (sGameSounds?.Count > 128)
            {
                return null;
            }

            Sound sound = new Sound(filename, loop, 0);
            sGameSounds?.Add(sound);

            return sound;
        }

        public static void StopSound(MapSound sound)
        {
            sound?.Stop();
        }

        public static void StopAllSounds()
        {
            for (int i = 0; i < sGameSounds.Count; i++)
            {
                if (sGameSounds[i] != null)
                {
                    sGameSounds[i].Stop();
                }
            }
        }

    }

}
