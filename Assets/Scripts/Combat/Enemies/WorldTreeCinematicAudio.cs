using UnityEngine;

namespace Combat.Enemies
{
    // 연출 소리는 연출 호스트의 자식으로만 존재한다. 스킵/취소가 긴 꼬리음을 남기지 않는다.
    public sealed class WorldTreeCinematicAudio
    {
        readonly GameObject root;
        readonly AudioSource flash, flight, landing, gathering;
        readonly AudioSource[] sources;
        bool flashed, flew, landed, gathered, paused;

        public WorldTreeCinematicAudio(Transform owner)
        {
            root = new GameObject("WorldTreeCinematicAudio");
            root.transform.SetParent(owner, false);
            flash = Source("Flash");
            flight = Source("Approach");
            landing = Source("Landing");
            gathering = Source("CoreRise");
            sources = new[] { flash, flight, landing, gathering };
        }
        AudioSource Source(string name)
        {
            var child = new GameObject(name);
            child.transform.SetParent(root.transform, false);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.clip = Resources.Load<AudioClip>("Combat/WorldTreeAudio/" + name);
            return source;
        }
        static void Cue(ref bool played, AudioSource source, float time, float start, float until, float volume)
        {
            if (played || time < start) return;
            played = true;
            // 큰 프레임 지연 뒤 지나간 소리가 한꺼번에 터지지 않는다.
            if (time >= until || source == null || source.clip == null) return;
            source.volume = volume;
            source.time = Mathf.Min(time - start, source.clip.length - .01f);
            source.Play();
        }
        public void Entrance(float time, float pan)
        {
            Cue(ref flashed, flash, time, WorldTreeEntrance.FlashAt, WorldTreeEntrance.FlightAt, .32f);
            Cue(ref flew, flight, time, WorldTreeEntrance.FlightAt, WorldTreeEntrance.LandAt, .025f);
            float progress = Mathf.InverseLerp(WorldTreeEntrance.FlightAt, WorldTreeEntrance.LandAt, time);
            if (flight != null)
            {
                flight.volume = Mathf.Lerp(.025f, .65f, progress * progress * progress) * Mathf.Clamp01((1 - progress) / .06f);
                flight.panStereo = Mathf.Lerp(0, pan, progress * progress);
                if (time >= WorldTreeEntrance.LandAt) flight.Stop();
            }
            if (landing != null) landing.panStereo = pan;
            Cue(ref landed, landing, time, WorldTreeEntrance.LandAt, WorldTreeEntrance.LandAt + .8f, .65f);
        }
        public void Transition(float time, float pan)
        {
            if (landing != null) landing.panStereo = pan;
            if (gathering != null) gathering.panStereo = pan;
            if (flash != null) flash.panStereo = pan;
            Cue(ref landed, landing, time, WorldTreeTransition.CrackAt, .8f, .22f);
            Cue(ref gathered, gathering, time, WorldTreeTransition.GatherStartAt, WorldTreeTransition.GatherAt + .8f, .42f);
            Cue(ref flashed, flash, time, WorldTreeTransition.GatherAt, WorldTreeTransition.GatherAt + .6f, .25f);
        }
        public void SetPaused(bool value)
        {
            if (root == null) return;
            foreach (var source in sources)
            {
                if (value != paused) { if (value) source.Pause(); else source.UnPause(); }
                if (!value) source.pitch = Mathf.Max(.01f, Time.timeScale);
            }
            paused = value;
        }
        public void Stop()
        {
            if (root == null) return;
            foreach (var source in sources) if (source != null) source.Stop();
            Object.Destroy(root);
        }
    }
}
