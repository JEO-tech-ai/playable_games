using System;
using System.Collections;
using UnityEngine;

namespace LegendChef.Visual
{
    public enum PlayMode { Loop, Once, PingPong }

    /// <summary>
    /// 애니메이션 클립 데이터
    /// </summary>
    public class AnimationClipData
    {
        public string Name;
        public Sprite[] Frames;
        public float FPS;
        public PlayMode Mode;
    }

    /// <summary>
    /// SpriteRenderer 기반 프레임 애니메이션 드라이버
    /// </summary>
    public class SpriteAnimator : MonoBehaviour
    {
        public event Action OnAnimationComplete;

        private SpriteRenderer _spriteRenderer;
        private AnimationClipData _currentClip;
        private Coroutine _playCoroutine;
        private bool _isPaused;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// 애니메이션 클립을 재생한다
        /// </summary>
        public void Play(AnimationClipData clip)
        {
            if (clip == null || clip.Frames == null || clip.Frames.Length == 0) return;
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null) return;

            Stop();
            _currentClip = clip;
            _isPaused = false;
            _playCoroutine = StartCoroutine(PlayCoroutine());
        }

        /// <summary>
        /// 재생 정지
        /// </summary>
        public void Stop()
        {
            if (_playCoroutine != null)
            {
                StopCoroutine(_playCoroutine);
                _playCoroutine = null;
            }
            _currentClip = null;
            _isPaused = false;
        }

        /// <summary>
        /// 일시 정지 / 재개
        /// </summary>
        public void Pause()
        {
            _isPaused = !_isPaused;
        }

        private IEnumerator PlayCoroutine()
        {
            if (_currentClip == null) yield break;

            Sprite[] frames = _currentClip.Frames;
            float interval = 1f / _currentClip.FPS;
            int frameCount = frames.Length;

            switch (_currentClip.Mode)
            {
                case PlayMode.Loop:
                    int loopIdx = 0;
                    while (true)
                    {
                        if (!_isPaused)
                        {
                            _spriteRenderer.sprite = frames[loopIdx % frameCount];
                            loopIdx++;
                        }
                        yield return new WaitForSeconds(interval);
                    }

                case PlayMode.Once:
                    for (int i = 0; i < frameCount; i++)
                    {
                        if (!_isPaused)
                        {
                            _spriteRenderer.sprite = frames[i];
                        }
                        yield return new WaitForSeconds(interval);
                    }
                    _playCoroutine = null;
                    OnAnimationComplete?.Invoke();
                    yield break;

                case PlayMode.PingPong:
                    int ppIdx = 0;
                    int direction = 1;
                    while (true)
                    {
                        if (!_isPaused)
                        {
                            _spriteRenderer.sprite = frames[ppIdx];
                            ppIdx += direction;
                            if (ppIdx >= frameCount)
                            {
                                ppIdx = frameCount - 2;
                                direction = -1;
                                if (ppIdx < 0) ppIdx = 0;
                            }
                            else if (ppIdx < 0)
                            {
                                ppIdx = 1;
                                direction = 1;
                                if (ppIdx >= frameCount) ppIdx = 0;
                            }
                        }
                        yield return new WaitForSeconds(interval);
                    }
            }
        }
    }
}
