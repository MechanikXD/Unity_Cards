using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Other.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Structure.Managers
{
    public class AudioManager : SingletonBase<AudioManager>
    {
        private CancellationTokenSource _cts = new CancellationTokenSource();
        [SerializeField] private AudioSource _musicSource;
        
        [SerializeField] private AudioSource _sourcePrefab;
        [SerializeField] private int _sourceCount = 10;
        [SerializeField] private float _fadeOutSpeed = 10f;
        private ObjectPool<AudioSource> _sourcePool;
        private LinkedList<AudioSource> _activeSources;

        protected override void Awake()
        {
            ToSingleton(true);
            Initialize();
        }

        protected override void Initialize()
        {
            _sourcePool = new ObjectPool<AudioSource>(_sourcePrefab, _sourceCount, 
                    RecordSource, ForgetSource, transform, true);
            _activeSources = new LinkedList<AudioSource>();
        }

        public void PlayMusic(AudioClip musicClip)
        {
            _musicSource.clip = musicClip;
            _musicSource.volume = 1f; // TODO: Pull From Settings
            _musicSource.Play();
            _activeSources.AddLast(_musicSource);
        }

        public void Play(AudioClip[] clips, Vector2 pitchRange) => 
            Play(clips.GetRandom(), pitchRange);

        public void Play(AudioClip clip, Vector2 pitchRange)
        {
            var source = _sourcePool.Pull();
            var pitch = Random.Range(pitchRange.x, pitchRange.y);
            PlayClip(source, clip, pitch).Forget();
        }

        public UniTask StopAllSources()
        {
            // This will cancel token and return a new one to be assigned
            _cts = _cts.Reset();
            // Snapshot before changing
            var sourcesSnapshot = _activeSources.ToArray();
            var tasks = new UniTask[sourcesSnapshot.Length];

            for (var i = 0; i < sourcesSnapshot.Length; i++)
                tasks[i] = FadeOutAudio(sourcesSnapshot[i], _fadeOutSpeed);
            
            return UniTask.WhenAll(tasks);
        }

        // This is called each time a source is pulled from _sourcePull (object pull)
        private void RecordSource(AudioSource source)
        {
            source.gameObject.SetActive(true);
            _activeSources.AddLast(source);
        }

        // This is called each time a source is returned to _sourcePull (object pull)
        private void ForgetSource(AudioSource source)
        {
            source.gameObject.SetActive(false);
            _activeSources.Remove(source);
        }

        private async UniTask PlayClip(AudioSource source, AudioClip clip, float pitch)
        {
            try
            {
                source.clip = clip;
                source.pitch = pitch;
                source.volume = 1f; // TODO: Pull from settings
                source.Play();

                await UniTask.WaitForSeconds(clip.length, cancellationToken: _cts.Token);
            }
            catch (OperationCanceledException)
            {
                // expected during StopAllSources
            }
            finally
            {
                CleanupSource(source);
            }
        }

        private async UniTask FadeOutAudio(AudioSource source, float fadeOutSpeed, float snapValue=0.05f)
        {
            try
            {
                while (source.volume > snapValue)
                {
                    source.volume -= fadeOutSpeed * Time.deltaTime;
                    await UniTask.NextFrame(_cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected
            }
            finally
            {
                CleanupSource(source);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _cts.Cancel();
        }
        
        private void CleanupSource(AudioSource source)
        {
            if (!_activeSources.Contains(source))
                return;

            source.Stop();
            _activeSources.Remove(source);
            _sourcePool.Return(source);
        }
    }
}