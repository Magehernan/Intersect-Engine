using Intersect.Client.Framework.Audio;
using Intersect.Client.General;
using Intersect.Client.UnityGame;
using Intersect.Client.UnityGame.Audio;

namespace Intersect.Client.Core.Sounds
{

    public class Sound
    {

        public bool Loaded;

        protected string mFilename;

        protected bool mLoop;

        protected int mLoopInterval;

        protected AudioPlayer soundPlayer;

        protected float mVolume;

        private long mStoppedTime = -1;

        public Sound(string filename, bool loop, int loopInterval)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return;
            }

            mFilename = filename;
            mLoop = loop;
            mLoopInterval = loopInterval;
            GameAudioSource sound = Globals.ContentManager.GetSound(mFilename);
            if (sound != null)
            {
                soundPlayer = UnityFactory.GetAudioPlayer(filename);
                soundPlayer.SetClip(sound.Clip);
                soundPlayer.SetLoop(loop && mLoopInterval <= 0);
                soundPlayer.SetVolume(Globals.Database.SoundVolume);
                soundPlayer.Play();
                Loaded = true;
            }
        }

        public bool Loop
        {
            get => mLoop;
            set
            {
                mLoop = value;
                if (soundPlayer != null)
                {
                    soundPlayer.SetLoop(value);
                }
            }
        }

        public virtual bool Update()
        {
            if (!Loaded)
            {
                return false;
            }

            if (mLoop && mLoopInterval > 0 && !soundPlayer.IsPlaying)
            {
                if (mStoppedTime == -1)
                {
                    mStoppedTime = Globals.System.GetTimeMs();
                }
                else
                {
                    if (mStoppedTime + mLoopInterval < Globals.System.GetTimeMs())
                    {
                        soundPlayer.Play();
                        mStoppedTime = -1;
                    }
                }
                return true;
            }
            else if (mLoop || soundPlayer.IsPlaying)
            {
                return true;
            }

            Stop();

            return false;
        }

        public virtual void Stop()
        {
            if (!Loaded)
            {
                return;
            }

            soundPlayer.Destroy();
            soundPlayer = null;
            Loaded = false;
        }

    }

}
