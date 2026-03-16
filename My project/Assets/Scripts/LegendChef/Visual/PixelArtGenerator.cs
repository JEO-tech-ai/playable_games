using UnityEngine;

namespace LegendChef.Visual
{
    /// <summary>
    /// 프로시저럴 픽셀 아트 스프라이트 생성기
    /// </summary>
    public static class PixelArtGenerator
    {
        private static Sprite _cachedWhiteSprite;

        // ── 캐시된 스프라이트 ──
        private static Sprite _cachedPlayerSprite;
        private static Sprite _cachedEnemyBasicSprite;
        private static Sprite _cachedEnemyBossSprite;
        private static Sprite _cachedCoinSprite;
        private static Sprite _cachedSpiceSprite;

        /// <summary>
        /// 4x4 흰색 스프라이트 (캐시)
        /// </summary>
        public static Sprite CreateWhiteSprite()
        {
            if (_cachedWhiteSprite != null) return _cachedWhiteSprite;

            Texture2D tex = new Texture2D(4, 4);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[16];
            for (int i = 0; i < 16; i++) pixels[i] = Color.white;
            tex.SetPixels(pixels);
            tex.Apply();

            _cachedWhiteSprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            return _cachedWhiteSprite;
        }

        /// <summary>
        /// 64x64 셰프 캐릭터 (흰 모자 + 식칼)
        /// </summary>
        public static Sprite CreatePlayerSprite()
        {
            if (_cachedPlayerSprite != null) return _cachedPlayerSprite;

            int size = 64;
            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];

            // 배경 투명
            Color clear = new Color(0, 0, 0, 0);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            Color skin = new Color(0.96f, 0.80f, 0.64f);     // 살색
            Color white = Color.white;                          // 모자
            Color outfit = new Color(0.95f, 0.95f, 0.95f);     // 유니폼
            Color darkGray = new Color(0.3f, 0.3f, 0.3f);      // 눈, 윤곽
            Color brown = new Color(0.55f, 0.35f, 0.17f);      // 식칼 손잡이
            Color silver = new Color(0.78f, 0.78f, 0.82f);     // 칼날
            Color red = new Color(0.9f, 0.2f, 0.2f);           // 넥타이/앞치마 장식

            // 몸통 (유니폼) - 중앙 하단
            FillRect(pixels, size, 22, 8, 20, 22, outfit);
            // 앞치마 장식 줄
            FillRect(pixels, size, 30, 8, 4, 22, red);

            // 머리 (살색)
            FillRect(pixels, size, 24, 32, 16, 14, skin);

            // 눈
            FillRect(pixels, size, 28, 38, 3, 3, darkGray);
            FillRect(pixels, size, 35, 38, 3, 3, darkGray);

            // 입 (미소)
            FillRect(pixels, size, 30, 34, 6, 2, new Color(0.85f, 0.45f, 0.35f));

            // 셰프 모자 (토크 블랑쉬)
            FillRect(pixels, size, 22, 46, 20, 4, white);  // 모자 밴드
            FillRect(pixels, size, 24, 50, 16, 8, white);  // 모자 윗부분
            FillRect(pixels, size, 26, 58, 12, 4, white);  // 모자 꼭대기

            // 왼쪽 팔
            FillRect(pixels, size, 16, 14, 6, 14, skin);

            // 오른쪽 팔 (칼 들고 있음)
            FillRect(pixels, size, 42, 14, 6, 14, skin);

            // 식칼 (오른손)
            FillRect(pixels, size, 48, 10, 4, 6, brown);   // 손잡이
            FillRect(pixels, size, 48, 16, 4, 18, silver);  // 칼날
            FillRect(pixels, size, 52, 18, 2, 14, silver);  // 칼날 넓은 부분

            // 다리
            FillRect(pixels, size, 24, 2, 7, 6, darkGray);
            FillRect(pixels, size, 33, 2, 7, 6, darkGray);

            tex.SetPixels(pixels);
            tex.Apply();

            _cachedPlayerSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 64f);
            return _cachedPlayerSprite;
        }

        /// <summary>
        /// 64x64 화난 버섯 몬스터
        /// </summary>
        public static Sprite CreateEnemyBasicSprite()
        {
            if (_cachedEnemyBasicSprite != null) return _cachedEnemyBasicSprite;

            int size = 64;
            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];

            Color clear = new Color(0, 0, 0, 0);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            Color capMain = new Color(0.91f, 0.30f, 0.24f);    // #e74c3c 빨간 갓
            Color capDark = new Color(0.75f, 0.22f, 0.17f);    // 갓 어두운 부분
            Color capSpot = new Color(0.95f, 0.85f, 0.70f);    // 갓 점무늬
            Color stem = new Color(0.95f, 0.91f, 0.82f);       // 줄기
            Color darkGray = new Color(0.2f, 0.2f, 0.2f);      // 눈
            Color eyeWhite = Color.white;

            // 줄기
            FillRect(pixels, size, 22, 2, 20, 18, stem);

            // 버섯 갓 (큰 반원 모양)
            FillEllipse(pixels, size, 32, 38, 28, 22, capMain);
            // 갓 하단 테두리
            FillRect(pixels, size, 6, 18, 52, 4, capDark);
            // 점무늬
            FillRect(pixels, size, 14, 36, 6, 6, capSpot);
            FillRect(pixels, size, 36, 42, 6, 6, capSpot);
            FillRect(pixels, size, 46, 32, 5, 5, capSpot);
            FillRect(pixels, size, 24, 46, 5, 5, capSpot);

            // 눈 (화난 표정)
            // 왼쪽 눈
            FillRect(pixels, size, 20, 26, 8, 6, eyeWhite);
            FillRect(pixels, size, 22, 26, 4, 4, darkGray);
            // 오른쪽 눈
            FillRect(pixels, size, 36, 26, 8, 6, eyeWhite);
            FillRect(pixels, size, 38, 26, 4, 4, darkGray);

            // 화난 눈썹 (대각선)
            FillRect(pixels, size, 18, 32, 10, 2, darkGray);
            FillRect(pixels, size, 36, 32, 10, 2, darkGray);

            // 입 (찡그린)
            FillRect(pixels, size, 26, 20, 12, 3, darkGray);

            // 발
            FillRect(pixels, size, 18, 0, 8, 4, stem);
            FillRect(pixels, size, 38, 0, 8, 4, stem);

            tex.SetPixels(pixels);
            tex.Apply();

            _cachedEnemyBasicSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 64f);
            return _cachedEnemyBasicSprite;
        }

        /// <summary>
        /// 128x128 보스 문어
        /// </summary>
        public static Sprite CreateEnemyBossSprite()
        {
            if (_cachedEnemyBossSprite != null) return _cachedEnemyBossSprite;

            int size = 128;
            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];

            Color clear = new Color(0, 0, 0, 0);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            Color body = new Color(0.56f, 0.27f, 0.68f);       // #8e44ad 보라색
            Color bodyLight = new Color(0.68f, 0.40f, 0.80f);
            Color darkPurple = new Color(0.40f, 0.15f, 0.50f);
            Color eyeWhite = Color.white;
            Color eyePupil = new Color(0.15f, 0.15f, 0.15f);
            Color sucker = new Color(0.80f, 0.55f, 0.85f);
            Color crown = new Color(1f, 0.84f, 0f);            // 보스 왕관

            // 머리 (큰 타원)
            FillEllipse(pixels, size, 64, 80, 44, 36, body);
            FillEllipse(pixels, size, 64, 82, 36, 28, bodyLight);

            // 왕관
            FillRect(pixels, size, 42, 110, 44, 6, crown);
            // 왕관 뾰족
            FillRect(pixels, size, 48, 116, 8, 8, crown);
            FillRect(pixels, size, 60, 116, 8, 10, crown);
            FillRect(pixels, size, 72, 116, 8, 8, crown);

            // 눈
            FillEllipse(pixels, size, 48, 82, 10, 8, eyeWhite);
            FillEllipse(pixels, size, 80, 82, 10, 8, eyeWhite);
            FillRect(pixels, size, 46, 80, 6, 6, eyePupil);
            FillRect(pixels, size, 78, 80, 6, 6, eyePupil);

            // 화난 눈썹
            FillRect(pixels, size, 36, 92, 20, 3, darkPurple);
            FillRect(pixels, size, 72, 92, 20, 3, darkPurple);

            // 입
            FillRect(pixels, size, 52, 68, 24, 6, darkPurple);
            FillRect(pixels, size, 56, 66, 16, 2, new Color(0.9f, 0.3f, 0.3f));

            // 다리 (8개 촉수)
            int[] legXPositions = { 16, 28, 40, 52, 64, 76, 88, 100 };
            for (int i = 0; i < legXPositions.Length; i++)
            {
                int lx = legXPositions[i];
                // 웨이브 형태의 다리
                for (int dy = 0; dy < 44; dy++)
                {
                    int waveOffset = (int)(Mathf.Sin(dy * 0.3f + i * 0.8f) * 3f);
                    int px = lx + waveOffset;
                    int py = 44 - dy;
                    if (px >= 0 && px < size - 6 && py >= 0 && py < size)
                    {
                        FillRect(pixels, size, px, py, 6, 1, body);
                        // 빨판
                        if (dy % 6 == 0)
                        {
                            FillRect(pixels, size, px + 1, py, 4, 1, sucker);
                        }
                    }
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            _cachedEnemyBossSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 128f);
            return _cachedEnemyBossSprite;
        }

        /// <summary>
        /// 32x32 골드 코인
        /// </summary>
        public static Sprite CreateCoinSprite()
        {
            if (_cachedCoinSprite != null) return _cachedCoinSprite;

            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];

            Color clear = new Color(0, 0, 0, 0);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            Color gold = new Color(0.95f, 0.77f, 0.06f);       // #f1c40f
            Color goldDark = new Color(0.80f, 0.63f, 0.05f);
            Color goldLight = new Color(1f, 0.88f, 0.30f);

            FillEllipse(pixels, size, 16, 16, 14, 14, gold);
            FillEllipse(pixels, size, 16, 16, 10, 10, goldDark);
            FillEllipse(pixels, size, 16, 17, 8, 8, goldLight);

            // $ 마크 (간단한 세로줄)
            FillRect(pixels, size, 15, 10, 2, 12, goldDark);
            FillRect(pixels, size, 12, 18, 8, 2, goldDark);
            FillRect(pixels, size, 12, 14, 8, 2, goldDark);

            tex.SetPixels(pixels);
            tex.Apply();

            _cachedCoinSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 32f);
            return _cachedCoinSprite;
        }

        /// <summary>
        /// 32x32 향신료 (빨간 고추)
        /// </summary>
        public static Sprite CreateSpiceSprite()
        {
            if (_cachedSpiceSprite != null) return _cachedSpiceSprite;

            int size = 32;
            Texture2D tex = new Texture2D(size, size);
            tex.filterMode = FilterMode.Point;
            Color[] pixels = new Color[size * size];

            Color clear = new Color(0, 0, 0, 0);
            for (int i = 0; i < pixels.Length; i++) pixels[i] = clear;

            Color spice = new Color(0.90f, 0.49f, 0.13f);      // #e67e22
            Color spiceDark = new Color(0.75f, 0.35f, 0.10f);
            Color spiceLight = new Color(1f, 0.60f, 0.20f);
            Color green = new Color(0.18f, 0.80f, 0.25f);

            // 고추 몸통
            FillEllipse(pixels, size, 16, 12, 8, 12, spice);
            FillEllipse(pixels, size, 14, 12, 6, 10, spiceLight);
            FillRect(pixels, size, 12, 4, 4, 4, spiceDark); // 꼬리

            // 꼭지
            FillRect(pixels, size, 14, 24, 4, 6, green);

            tex.SetPixels(pixels);
            tex.Apply();

            _cachedSpiceSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 32f);
            return _cachedSpiceSprite;
        }

        /// <summary>
        /// 배경 스프라이트
        /// </summary>
        public static Sprite CreateBackgroundSprite(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h);
            tex.filterMode = FilterMode.Point;
            Color bgColor = new Color(0.17f, 0.24f, 0.31f); // #2c3e50
            Color[] pixels = new Color[w * h];

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // 약간의 그라데이션
                    float gradient = (float)y / h * 0.1f;
                    pixels[y * w + x] = new Color(
                        bgColor.r + gradient,
                        bgColor.g + gradient,
                        bgColor.b + gradient);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
        }

        // ── 유틸리티 ──

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
    }
}
