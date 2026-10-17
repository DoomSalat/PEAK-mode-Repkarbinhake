using UnityEngine;

namespace Repkarbinhake.Sandbox
{
    public sealed class CarabinerAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource _source;
        [SerializeField] private AudioClip _lockClip;
        [SerializeField] private AudioClip _unlockClip;

        public void PlayLock(Vector3 position)
        {
            Play(_lockClip, position);
        }

        public void PlayUnlock(Vector3 position)
        {
            Play(_unlockClip, position);
        }

        private void Play(AudioClip clip, Vector3 position)
        {
            _source.transform.position = position;
            _source.PlayOneShot(clip);
        }
    }
}
