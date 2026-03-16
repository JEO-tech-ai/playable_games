using UnityEngine;

namespace LegendChef.Visual
{
    /// <summary>
    /// 프로시저럴 Texture2D 프레임으로 AnimationClipData를 빌드하는 정적 클래스
    /// </summary>
    public static class AnimationFrameBuilder
    {
        // ── 캐시된 클립 ──
        private static AnimationClipData _chefIdle;
        private static AnimationClipData _chefAttack;
        private static AnimationClipData _chefHit;
        private static AnimationClipData _enemyWalk;
        private static AnimationClipData _enemyHit;
        private static AnimationClipData _bossIdle;
        private static AnimationClipData _bossRage;
        private static AnimationClipData _slashEffect;
        private static AnimationClipData _critBurst;
        private static AnimationClipData _bossDeath;

        // ── 색상 팔레트 (PixelArtGenerator와 동일) ──
        private static readonly Color Skin = new Color(0.96f, 0.80f, 0.64f);
        private static readonly Color White = Color.white;
        private static readonly Color Outfit = new Color(0.95f, 0.95f, 0.95f);
        private static readonly Color DarkGray = new Color(0.3f, 0.3f, 0.3f);
        private static readonly Color Brown = new Color(0.55f, 0.35f, 0.17f);
        private static readonly Color Silver = new Color(0.78f, 0.78f, 0.82f);
        private static readonly Color Red = new Color(0.9f, 0.2f, 0.2f);
        private static readonly Color Clear = new Color(0, 0, 0, 0);

        // 버섯 색상
        private static readonly Color CapMain = new Color(0.91f, 0.30f, 0.24f);
        private static readonly Color CapDark = new Color(0.75f, 0.22f, 0.17f);
        private static readonly Color CapSpot = new Color(0.95f, 0.85f, 0.70f);
        private static readonly Color Stem = new Color(0.95f, 0.91f, 0.82f);
        private static readonly Color EyeWhite = Color.white;

        // 보스 색상
        private static readonly Color BossBody = new Color(0.56f, 0.27f, 0.68f);
        private static readonly Color BossBodyLight = new Color(0.68f, 0.40f, 0.80f);
        private static readonly Color BossDarkPurple = new Color(0.40f, 0.15f, 0.50f);
        private static readonly Color BossSucker = new Color(0.80f, 0.55f, 0.85f);
        private static readonly Color BossCrown = new Color(1f, 0.84f, 0f);

        // ═══════════════════════════════════════
        // Chef IDLE (2 frames, 4 FPS, Loop)
        // ═══════════════════════════════════════
        public static AnimationClipData GetChefIdle()
        {
            if (_chefIdle != null) return _chefIdle;

            Sprite frame0 = BuildChefBase(0);
            Sprite frame1 = BuildChefBase(2); // 2px upward body offset (breathing bob)

            _chefIdle = new AnimationClipData
            {
                Name = "ChefIdle",
                Frames = new[] { frame0, frame1 },
                FPS = 4f,
                Mode = PlayMode.Loop
            };
            return _chefIdle;
        }

        // ═══════════════════════════════════════
        // Chef ATTACK (3 frames, 12 FPS, Once)
        // ═══════════════════════════════════════
        public static AnimationClipData GetChefAttack()
        {
            if (_chefAttack != null) return _chefAttack;

            Sprite[] frames = new Sprite[3];

            // Frame 0: windup - cleaver raised above head
            {
                int size = 64;
                Color[] px = CreateClearPixels(size);
                DrawChefBody(px, size, 0);
                // 오른쪽 팔 (높이 올린 상태)
                FillRect(px, size, 42, 20, 6, 14, Skin);
                // 식칼 - 머리 위
                FillRect(px, size, 44, 48, 4, 6, Brown);
                FillRect(px, size, 44, 54, 4, 10, Silver);
                FillRect(px, size, 48, 56, 2, 8, Silver);
                frames[0] = BuildSprite(px, size, size, 64f);
            }

            // Frame 1: mid-swing - cleaver at 45 angle
            {
                int size = 64;
                Color[] px = CreateClearPixels(size);
                DrawChefBody(px, size, 0);
                FillRect(px, size, 42, 14, 6, 14, Skin);
                // 식칼 - 45도 대각선 (근사)
                FillRect(px, size, 50, 28, 4, 4, Brown);
                FillRect(px, size, 54, 22, 3, 8, Silver);
                FillRect(px, size, 52, 18, 3, 6, Silver);
                frames[1] = BuildSprite(px, size, size, 64f);
            }

            // Frame 2: impact - cleaver fully down
            {
                int size = 64;
                Color[] px = CreateClearPixels(size);
                DrawChefBody(px, size, 0);
                FillRect(px, size, 42, 12, 6, 14, Skin);
                // 식칼 - 아래로 내려친 상태
                FillRect(px, size, 48, 4, 4, 6, Brown);
                FillRect(px, size, 48, 10, 4, 8, Silver);
                FillRect(px, size, 52, 8, 2, 10, Silver);
                // impact pose: 살짝 앞으로 기울인 셰프
                FillRect(px, size, 24, 30, 16, 14, Skin); // 머리 약간 앞으로
                frames[2] = BuildSprite(px, size, size, 64f);
            }

            _chefAttack = new AnimationClipData
            {
                Name = "ChefAttack",
                Frames = frames,
                FPS = 12f,
                Mode = PlayMode.Once
            };
            return _chefAttack;
        }

        // ═══════════════════════════════════════
        // Chef HIT (2 frames, 8 FPS, Once)
        // ═══════════════════════════════════════
        public static AnimationClipData GetChefHit()
        {
            if (_chefHit != null) return _chefHit;

            // Frame 0: recoil left (4px shift, red tint overlay)
            {
                // We'll build both frames
            }

            int size = 64;
            Sprite[] frames = new Sprite[2];

            // Frame 0: shifted 4px left + red tint
            {
                Color[] px = CreateClearPixels(size);
                DrawChefBody(px, size, 0, -4); // shift left by 4px
                DrawChefArmsAndCleaver(px, size, -4);
                ApplyColorOverlay(px, size, new Color(1f, 0.3f, 0.3f, 0.4f));
                frames[0] = BuildSprite(px, size, size, 64f);
            }

            // Frame 1: recovery (back to normal, slight red)
            {
                Color[] px = CreateClearPixels(size);
                DrawChefBody(px, size, 0, 0);
                DrawChefArmsAndCleaver(px, size, 0);
                ApplyColorOverlay(px, size, new Color(1f, 0.3f, 0.3f, 0.15f));
                frames[1] = BuildSprite(px, size, size, 64f);
            }

            _chefHit = new AnimationClipData
            {
                Name = "ChefHit",
                Frames = frames,
                FPS = 8f,
                Mode = PlayMode.Once
            };
            return _chefHit;
        }

        // ═══════════════════════════════════════
        // Enemy WALK (2 frames, 6 FPS, Loop)
        // ═══════════════════════════════════════
        public static AnimationClipData GetEnemyWalk()
        {
            if (_enemyWalk != null) return _enemyWalk;

            int size = 64;
            Sprite[] frames = new Sprite[2];

            // Frame 0: left foot forward
            {
                Color[] px = CreateClearPixels(size);
                DrawMushroomBody(px, size);
                // 왼발 앞으로
                FillRect(px, size, 14, 0, 8, 4, Stem);
                FillRect(px, size, 40, 0, 8, 4, Stem);
                frames[0] = BuildSprite(px, size, size, 64f);
            }

            // Frame 1: right foot forward (feet shifted)
            {
                Color[] px = CreateClearPixels(size);
                DrawMushroomBody(px, size);
                // 오른발 앞으로
                FillRect(px, size, 20, 0, 8, 4, Stem);
                FillRect(px, size, 36, 0, 8, 4, Stem);
                frames[1] = BuildSprite(px, size, size, 64f);
            }

            _enemyWalk = new AnimationClipData
            {
                Name = "EnemyWalk",
                Frames = frames,
                FPS = 6f,
                Mode = PlayMode.Loop
            };
            return _enemyWalk;
        }

        // ═══════════════════════════════════════
        // Enemy HIT (2 frames, 16 FPS, Once)
        // ═══════════════════════════════════════
        public static AnimationClipData GetEnemyHit()
        {
            if (_enemyHit != null) return _enemyHit;

            int size = 64;
            Sprite[] frames = new Sprite[2];

            // Frame 0: flash white (solid white overlay)
            {
                Color[] px = CreateClearPixels(size);
                DrawMushroomBody(px, size);
                DrawMushroomFeet(px, size);
                ApplyWhiteFlash(px, size);
                frames[0] = BuildSprite(px, size, size, 64f);
            }

            // Frame 1: red tint version
            {
                Color[] px = CreateClearPixels(size);
                DrawMushroomBody(px, size);
                DrawMushroomFeet(px, size);
                ApplyColorOverlay(px, size, new Color(1f, 0.2f, 0.2f, 0.5f));
                frames[1] = BuildSprite(px, size, size, 64f);
            }

            _enemyHit = new AnimationClipData
            {
                Name = "EnemyHit",
                Frames = frames,
                FPS = 16f,
                Mode = PlayMode.Once
            };
            return _enemyHit;
        }

        // ═══════════════════════════════════════
        // Boss IDLE (3 frames, 4 FPS, Loop)
        // ═══════════════════════════════════════
        public static AnimationClipData GetBossIdle()
        {
            if (_bossIdle != null) return _bossIdle;

            int size = 128;
            Sprite[] frames = new Sprite[3];

            // Frame 0: tentacles at rest
            frames[0] = BuildBossFrame(size, 0f);

            // Frame 1: tentacles slightly raised
            frames[1] = BuildBossFrame(size, 4f);

            // Frame 2: tentacles back down
            frames[2] = BuildBossFrame(size, -2f);

            _bossIdle = new AnimationClipData
            {
                Name = "BossIdle",
                Frames = frames,
                FPS = 4f,
                Mode = PlayMode.Loop
            };
            return _bossIdle;
        }

        // ═══════════════════════════════════════
        // Boss RAGE (2 frames, 8 FPS, PingPong)
        // ═══════════════════════════════════════
        public static AnimationClipData GetBossRage()
        {
            if (_bossRage != null) return _bossRage;

            int size = 128;
            Sprite[] frames = new Sprite[2];

            // Frame 0: normal boss
            frames[0] = BuildBossFrame(size, 0f);

            // Frame 1: red-tinted boss
            {
                Color[] px = CreateClearPixels(size);
                DrawBossBody(px, size, 0f);
                ApplyColorOverlay(px, size, new Color(1f, 0.1f, 0.1f, 0.4f));
                frames[1] = BuildSprite(px, size, size, 128f);
            }

            _bossRage = new AnimationClipData
            {
                Name = "BossRage",
                Frames = frames,
                FPS = 8f,
                Mode = PlayMode.PingPong
            };
            return _bossRage;
        }

        // ═══════════════════════════════════════
        // SLASH Effect (3 frames, 24 FPS, Once)
        // ═══════════════════════════════════════
        public static AnimationClipData GetSlashEffect()
        {
            if (_slashEffect != null) return _slashEffect;

            int size = 64;
            Sprite[] frames = new Sprite[3];

            // Frame 0: thick white diagonal arc
            {
                Color[] px = CreateClearPixels(size);
                DrawDiagonalArc(px, size, Color.white, 4);
                frames[0] = BuildSprite(px, size, size, 64f);
            }

            // Frame 1: medium yellow arc
            {
                Color[] px = CreateClearPixels(size);
                DrawDiagonalArc(px, size, new Color(1f, 0.9f, 0.2f), 3);
                frames[1] = BuildSprite(px, size, size, 64f);
            }

            // Frame 2: thin semi-transparent yellow arc
            {
                Color[] px = CreateClearPixels(size);
                DrawDiagonalArc(px, size, new Color(1f, 0.9f, 0.2f, 0.5f), 2);
                frames[2] = BuildSprite(px, size, size, 64f);
            }

            _slashEffect = new AnimationClipData
            {
                Name = "SlashEffect",
                Frames = frames,
                FPS = 24f,
                Mode = PlayMode.Once
            };
            return _slashEffect;
        }

        // ═══════════════════════════════════════
        // CRIT BURST (4 frames, 20 FPS, Once)
        // ═══════════════════════════════════════
        public static AnimationClipData GetCritBurst()
        {
            if (_critBurst != null) return _critBurst;

            int size = 64;
            Sprite[] frames = new Sprite[4];
            Color burstColor = new Color(1f, 0.7f, 0.1f);

            int[] radii = { 4, 10, 18, 28 };
            for (int i = 0; i < 4; i++)
            {
                Color[] px = CreateClearPixels(size);
                float alpha = 1f - i * 0.2f;
                Color c = new Color(burstColor.r, burstColor.g, burstColor.b, alpha);
                FillEllipse(px, size, 32, 32, radii[i], radii[i], c);
                // 별 모양: 십자 추가
                int crossR = radii[i] + 4;
                FillRect(px, size, 32 - 2, 32 - crossR, 4, crossR * 2, c);
                FillRect(px, size, 32 - crossR, 32 - 2, crossR * 2, 4, c);
                frames[i] = BuildSprite(px, size, size, 64f);
            }

            _critBurst = new AnimationClipData
            {
                Name = "CritBurst",
                Frames = frames,
                FPS = 20f,
                Mode = PlayMode.Once
            };
            return _critBurst;
        }

        // ═══════════════════════════════════════
        // BOSS DEATH (5 frames, 16 FPS, Once)
        // ═══════════════════════════════════════
        public static AnimationClipData GetBossDeath()
        {
            if (_bossDeath != null) return _bossDeath;

            int size = 128;
            Sprite[] frames = new Sprite[5];
            int[] radii = { 10, 20, 35, 50, 70 };

            for (int i = 0; i < 5; i++)
            {
                Color[] px = CreateClearPixels(size);
                int r = radii[i];
                Color ringColor = new Color(0.8f, 0.5f, 1f, 1f - i * 0.15f);
                // 링 (외부 타원 - 내부 타원)
                FillEllipse(px, size, 64, 64, r, r, ringColor);
                if (r > 4)
                {
                    // 내부를 투명으로 (링 두께 약 4px)
                    int innerR = r - 4;
                    FillEllipse(px, size, 64, 64, innerR, innerR, Clear);
                }
                // 흰색 하이라이트 링
                Color highlight = new Color(1f, 1f, 1f, 0.5f - i * 0.1f);
                if (highlight.a > 0)
                {
                    FillEllipse(px, size, 64, 64, r + 2, r + 2, highlight);
                    FillEllipse(px, size, 64, 64, r, r, Clear);
                    // 원래 링 다시 그리기
                    FillEllipse(px, size, 64, 64, r, r, ringColor);
                    if (r > 4)
                    {
                        int innerR = r - 4;
                        FillEllipse(px, size, 64, 64, innerR, innerR, Clear);
                    }
                }
                frames[i] = BuildSprite(px, size, size, 128f);
            }

            _bossDeath = new AnimationClipData
            {
                Name = "BossDeath",
                Frames = frames,
                FPS = 16f,
                Mode = PlayMode.Once
            };
            return _bossDeath;
        }

        // ═══════════════════════════════════════
        // 셰프 그리기 헬퍼
        // ═══════════════════════════════════════

        private static Color[] CreateClearPixels(int size)
        {
            Color[] px = new Color[size * size];
            for (int i = 0; i < px.Length; i++) px[i] = Clear;
            return px;
        }

        /// <summary>
        /// 셰프 몸통 + 머리 + 모자 + 다리 그리기 (팔/칼 제외)
        /// </summary>
        private static void DrawChefBody(Color[] px, int size, int yOffset, int xOffset = 0)
        {
            // 몸통 (유니폼)
            FillRect(px, size, 22 + xOffset, 8 + yOffset, 20, 22, Outfit);
            // 앞치마 장식
            FillRect(px, size, 30 + xOffset, 8 + yOffset, 4, 22, Red);
            // 머리
            FillRect(px, size, 24 + xOffset, 32 + yOffset, 16, 14, Skin);
            // 눈
            FillRect(px, size, 28 + xOffset, 38 + yOffset, 3, 3, DarkGray);
            FillRect(px, size, 35 + xOffset, 38 + yOffset, 3, 3, DarkGray);
            // 입
            FillRect(px, size, 30 + xOffset, 34 + yOffset, 6, 2, new Color(0.85f, 0.45f, 0.35f));
            // 모자
            FillRect(px, size, 22 + xOffset, 46 + yOffset, 20, 4, White);
            FillRect(px, size, 24 + xOffset, 50 + yOffset, 16, 8, White);
            FillRect(px, size, 26 + xOffset, 58 + yOffset, 12, 4, White);
            // 왼쪽 팔
            FillRect(px, size, 16 + xOffset, 14 + yOffset, 6, 14, Skin);
            // 다리
            FillRect(px, size, 24 + xOffset, 2, 7, 6, DarkGray);
            FillRect(px, size, 33 + xOffset, 2, 7, 6, DarkGray);
        }

        private static void DrawChefArmsAndCleaver(Color[] px, int size, int xOffset)
        {
            // 오른쪽 팔
            FillRect(px, size, 42 + xOffset, 14, 6, 14, Skin);
            // 식칼
            FillRect(px, size, 48 + xOffset, 10, 4, 6, Brown);
            FillRect(px, size, 48 + xOffset, 16, 4, 18, Silver);
            FillRect(px, size, 52 + xOffset, 18, 2, 14, Silver);
        }

        private static Sprite BuildChefBase(int bodyYOffset)
        {
            int size = 64;
            Color[] px = CreateClearPixels(size);
            DrawChefBody(px, size, bodyYOffset);
            DrawChefArmsAndCleaver(px, size, 0);
            return BuildSprite(px, size, size, 64f);
        }

        // ═══════════════════════════════════════
        // 버섯 그리기 헬퍼
        // ═══════════════════════════════════════

        private static void DrawMushroomBody(Color[] px, int size)
        {
            // 줄기
            FillRect(px, size, 22, 2, 20, 18, Stem);
            // 갓
            FillEllipse(px, size, 32, 38, 28, 22, CapMain);
            FillRect(px, size, 6, 18, 52, 4, CapDark);
            // 점무늬
            FillRect(px, size, 14, 36, 6, 6, CapSpot);
            FillRect(px, size, 36, 42, 6, 6, CapSpot);
            FillRect(px, size, 46, 32, 5, 5, CapSpot);
            FillRect(px, size, 24, 46, 5, 5, CapSpot);
            // 눈
            FillRect(px, size, 20, 26, 8, 6, EyeWhite);
            FillRect(px, size, 22, 26, 4, 4, DarkGray);
            FillRect(px, size, 36, 26, 8, 6, EyeWhite);
            FillRect(px, size, 38, 26, 4, 4, DarkGray);
            // 눈썹
            FillRect(px, size, 18, 32, 10, 2, DarkGray);
            FillRect(px, size, 36, 32, 10, 2, DarkGray);
            // 입
            FillRect(px, size, 26, 20, 12, 3, DarkGray);
        }

        private static void DrawMushroomFeet(Color[] px, int size)
        {
            FillRect(px, size, 18, 0, 8, 4, Stem);
            FillRect(px, size, 38, 0, 8, 4, Stem);
        }

        // ═══════════════════════════════════════
        // 보스 그리기 헬퍼
        // ═══════════════════════════════════════

        private static void DrawBossBody(Color[] px, int size, float tentacleOffset)
        {
            // 머리
            FillEllipse(px, size, 64, 80, 44, 36, BossBody);
            FillEllipse(px, size, 64, 82, 36, 28, BossBodyLight);

            // 왕관
            FillRect(px, size, 42, 110, 44, 6, BossCrown);
            FillRect(px, size, 48, 116, 8, 8, BossCrown);
            FillRect(px, size, 60, 116, 8, 10, BossCrown);
            FillRect(px, size, 72, 116, 8, 8, BossCrown);

            // 눈
            FillEllipse(px, size, 48, 82, 10, 8, EyeWhite);
            FillEllipse(px, size, 80, 82, 10, 8, EyeWhite);
            FillRect(px, size, 46, 80, 6, 6, new Color(0.15f, 0.15f, 0.15f));
            FillRect(px, size, 78, 80, 6, 6, new Color(0.15f, 0.15f, 0.15f));

            // 눈썹
            FillRect(px, size, 36, 92, 20, 3, BossDarkPurple);
            FillRect(px, size, 72, 92, 20, 3, BossDarkPurple);

            // 입
            FillRect(px, size, 52, 68, 24, 6, BossDarkPurple);
            FillRect(px, size, 56, 66, 16, 2, new Color(0.9f, 0.3f, 0.3f));

            // 촉수
            int[] legXPositions = { 16, 28, 40, 52, 64, 76, 88, 100 };
            for (int i = 0; i < legXPositions.Length; i++)
            {
                int lx = legXPositions[i];
                for (int dy = 0; dy < 44; dy++)
                {
                    int waveOffset = (int)(Mathf.Sin(dy * 0.3f + i * 0.8f) * 3f);
                    int px2 = lx + waveOffset;
                    int py = 44 - dy + (int)tentacleOffset;
                    if (px2 >= 0 && px2 < size - 6 && py >= 0 && py < size)
                    {
                        FillRect(px, size, px2, py, 6, 1, BossBody);
                        if (dy % 6 == 0)
                        {
                            FillRect(px, size, px2 + 1, py, 4, 1, BossSucker);
                        }
                    }
                }
            }
        }

        private static Sprite BuildBossFrame(int size, float tentacleOffset)
        {
            Color[] px = CreateClearPixels(size);
            DrawBossBody(px, size, tentacleOffset);
            return BuildSprite(px, size, size, 128f);
        }

        // ═══════════════════════════════════════
        // 이펙트 그리기 헬퍼
        // ═══════════════════════════════════════

        /// <summary>
        /// 대각선 아크 그리기 (bottom-left to top-right)
        /// </summary>
        private static void DrawDiagonalArc(Color[] px, int size, Color color, int thickness)
        {
            for (int i = 0; i < size; i++)
            {
                int x = i;
                int y = i; // 대각선 bottom-left to top-right
                // 약간의 곡선 (arc)
                int curveOffset = (int)(Mathf.Sin((float)i / size * Mathf.PI) * 8f);
                int drawY = y + curveOffset;

                for (int t = -thickness; t <= thickness; t++)
                {
                    int px2 = x;
                    int py = drawY + t;
                    if (px2 >= 0 && px2 < size && py >= 0 && py < size)
                    {
                        px[py * size + px2] = color;
                    }
                }
            }
        }

        /// <summary>
        /// 흰색 플래시 오버레이 (불투명 픽셀을 흰색으로)
        /// </summary>
        private static void ApplyWhiteFlash(Color[] px, int size)
        {
            for (int i = 0; i < px.Length; i++)
            {
                if (px[i].a > 0.1f)
                {
                    px[i] = new Color(1f, 1f, 1f, px[i].a);
                }
            }
        }

        /// <summary>
        /// 색상 오버레이 적용 (불투명 픽셀에만)
        /// </summary>
        private static void ApplyColorOverlay(Color[] px, int size, Color overlay)
        {
            for (int i = 0; i < px.Length; i++)
            {
                if (px[i].a > 0.1f)
                {
                    px[i] = Color.Lerp(px[i], new Color(overlay.r, overlay.g, overlay.b, px[i].a), overlay.a);
                }
            }
        }

        // ═══════════════════════════════════════
        // 유틸리티 (PixelArtGenerator 패턴과 동일)
        // ═══════════════════════════════════════

        private static void FillRect(Color[] pixels, int texSize, int x, int y, int w, int h, Color color)
        {
            for (int py = y; py < y + h && py < texSize; py++)
            {
                for (int px = x; px < x + w && px < texSize; px++)
                {
                    if (px >= 0 && py >= 0)
                    {
                        pixels[py * texSize + px] = color;
                    }
                }
            }
        }

        private static void FillEllipse(Color[] pixels, int texSize, int cx, int cy, int rx, int ry, Color color)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    if (x < 0 || x >= texSize || y < 0 || y >= texSize) continue;
                    float dx = (float)(x - cx) / rx;
                    float dy = (float)(y - cy) / ry;
                    if (dx * dx + dy * dy <= 1f)
                    {
                        pixels[y * texSize + x] = color;
                    }
                }
            }
        }

        private static Sprite BuildSprite(Color[] pixels, int w, int h, float ppu)
        {
            Texture2D tex = new Texture2D(w, h);
            tex.filterMode = FilterMode.Point;
            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
        }
    }
}
