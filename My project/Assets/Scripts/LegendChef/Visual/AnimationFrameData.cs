using System.Collections.Generic;
using UnityEngine;

namespace LegendChef.Visual
{
    /// <summary>
    /// 전설의 불꽃 셰프용 프로시저럴 픽셀 애니메이션 프레임 데이터.
    /// DrawCommand 목록을 보관하고 필요 시 Color32[]로 바로 렌더링한다.
    /// </summary>
    public static class AnimationFrameData
    {
        public enum DrawShape
        {
            Rect,
            Ellipse
        }

        public readonly struct DrawCommand
        {
            public readonly DrawShape Shape;
            public readonly int X;
            public readonly int Y;
            public readonly int W;
            public readonly int H;
            public readonly Color32 Color;

            public DrawCommand(DrawShape shape, int x, int y, int w, int h, Color32 color)
            {
                Shape = shape;
                X = x;
                Y = y;
                W = w;
                H = h;
                Color = color;
            }
        }

        public readonly struct FrameData
        {
            public readonly int Width;
            public readonly int Height;
            public readonly DrawCommand[] Commands;

            public FrameData(int width, int height, DrawCommand[] commands)
            {
                Width = width;
                Height = height;
                Commands = commands;
            }
        }

        private static readonly Color32 Clear = new Color32(0, 0, 0, 0);
        private static readonly Color32 Outline = new Color32(44, 44, 52, 255);
        private static readonly Color32 Skin = new Color32(245, 204, 163, 255);
        private static readonly Color32 SkinShade = new Color32(224, 175, 132, 255);
        private static readonly Color32 HatWhite = new Color32(252, 252, 248, 255);
        private static readonly Color32 Uniform = new Color32(234, 238, 242, 255);
        private static readonly Color32 ApronRed = new Color32(220, 52, 58, 255);
        private static readonly Color32 KnifeMetal = new Color32(198, 206, 214, 255);
        private static readonly Color32 KnifeHighlight = new Color32(240, 245, 250, 255);
        private static readonly Color32 KnifeHandle = new Color32(120, 72, 42, 255);
        private static readonly Color32 MushroomCap = new Color32(208, 92, 84, 255);
        private static readonly Color32 MushroomCapDark = new Color32(150, 48, 48, 255);
        private static readonly Color32 MushroomSpot = new Color32(242, 226, 186, 255);
        private static readonly Color32 MushroomStem = new Color32(236, 222, 198, 255);
        private static readonly Color32 MushroomRedFlash = new Color32(255, 110, 110, 255);
        private static readonly Color32 BossBody = new Color32(126, 92, 196, 255);
        private static readonly Color32 BossBodyLight = new Color32(164, 128, 220, 255);
        private static readonly Color32 BossBodyDark = new Color32(86, 52, 146, 255);
        private static readonly Color32 BossSucker = new Color32(220, 164, 230, 255);
        private static readonly Color32 BossCrown = new Color32(248, 206, 82, 255);
        private static readonly Color32 RageRed = new Color32(255, 72, 72, 176);
        private static readonly Color32 FxWhite = new Color32(255, 255, 255, 255);
        private static readonly Color32 FxYellow = new Color32(255, 226, 92, 255);
        private static readonly Color32 FxOrange = new Color32(255, 164, 72, 220);
        private static readonly Color32 FxRed = new Color32(255, 84, 84, 180);
        private static readonly Color32 FxTransparentWhite = new Color32(255, 255, 255, 96);
        private static readonly Color32 FxTransparentYellow = new Color32(255, 226, 92, 88);

        public static readonly FrameData[] ChefIdle = CreateChefIdleFrames();
        public static readonly FrameData[] ChefAttack = CreateChefAttackFrames();
        public static readonly FrameData[] ChefHit = CreateChefHitFrames();

        public static readonly FrameData[] MushroomWalk = CreateMushroomWalkFrames();
        public static readonly FrameData[] MushroomHit = CreateMushroomHitFrames();

        public static readonly FrameData[] BossIdle = CreateBossIdleFrames();
        public static readonly FrameData[] BossRage = CreateBossRageFrames();

        public static readonly FrameData[] Slash = CreateSlashFrames();
        public static readonly FrameData[] CritBurst = CreateCritBurstFrames();
        public static readonly FrameData[] BossDeath = CreateBossDeathFrames();

        public static Color32[] BuildPixels(FrameData frame)
        {
            Color32[] pixels = new Color32[frame.Width * frame.Height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Clear;
            }

            for (int i = 0; i < frame.Commands.Length; i++)
            {
                DrawCommand command = frame.Commands[i];
                if (command.Shape == DrawShape.Rect)
                {
                    FillRect(pixels, frame.Width, frame.Height, command.X, command.Y, command.W, command.H, command.Color);
                }
                else
                {
                    FillEllipse(pixels, frame.Width, frame.Height, command.X, command.Y, command.W, command.H, command.Color);
                }
            }

            return pixels;
        }

        public static Color32[][] BuildPixels(FrameData[] frames)
        {
            Color32[][] result = new Color32[frames.Length][];
            for (int i = 0; i < frames.Length; i++)
            {
                result[i] = BuildPixels(frames[i]);
            }

            return result;
        }

        private static FrameData[] CreateChefIdleFrames()
        {
            return new[]
            {
                CreateChefFrame(0, 0, 0, 0, 0, false, false),
                CreateChefFrame(0, 1, -1, 1, 1, false, false)
            };
        }

        private static FrameData[] CreateChefAttackFrames()
        {
            return new[]
            {
                CreateChefFrame(-1, 0, 0, 10, 8, true, false),
                CreateChefFrame(1, -1, 3, 0, -6, false, true),
                CreateChefFrame(2, 0, 4, -6, -12, false, true)
            };
        }

        private static FrameData[] CreateChefHitFrames()
        {
            return new[]
            {
                CreateChefFrame(-4, 0, -2, -4, 4, false, false, true)
            };
        }

        private static FrameData[] CreateMushroomWalkFrames()
        {
            return new[]
            {
                CreateMushroomFrame(-2, 1, false),
                CreateMushroomFrame(2, -1, false)
            };
        }

        private static FrameData[] CreateMushroomHitFrames()
        {
            return new[]
            {
                CreateMushroomFrame(0, 0, true)
            };
        }

        private static FrameData[] CreateBossIdleFrames()
        {
            return new[]
            {
                CreateBossFrame(0, 0, false),
                CreateBossFrame(1, 1, false)
            };
        }

        private static FrameData[] CreateBossRageFrames()
        {
            return new[]
            {
                CreateBossFrame(0, 0, true)
            };
        }

        private static FrameData[] CreateSlashFrames()
        {
            return new[]
            {
                CreateSlashFrame(FxWhite, 0, 0),
                CreateSlashFrame(FxYellow, 2, -1),
                CreateSlashFrame(FxTransparentYellow, 4, -2)
            };
        }

        private static FrameData[] CreateCritBurstFrames()
        {
            return new[]
            {
                CreateCritBurstFrame(14, FxWhite, FxYellow),
                CreateCritBurstFrame(18, FxYellow, FxOrange),
                CreateCritBurstFrame(22, FxOrange, FxRed),
                CreateCritBurstFrame(26, FxTransparentWhite, FxTransparentYellow)
            };
        }

        private static FrameData[] CreateBossDeathFrames()
        {
            return new[]
            {
                CreateBossDeathFrame(12, 4, FxWhite, FxYellow),
                CreateBossDeathFrame(20, 6, FxYellow, FxOrange),
                CreateBossDeathFrame(30, 8, FxOrange, FxRed),
                CreateBossDeathFrame(42, 10, FxRed, FxTransparentYellow),
                CreateBossDeathFrame(54, 12, FxTransparentWhite, FxTransparentYellow)
            };
        }

        private static FrameData CreateChefFrame(int bodyOffsetX, int bodyOffsetY, int knifeOffsetX, int knifeOffsetY, int knifeTilt, bool windUp, bool followThrough, bool knockedBack = false)
        {
            List<DrawCommand> commands = new List<DrawCommand>(48);

            int ox = 32 + bodyOffsetX;
            int oy = 10 + bodyOffsetY;

            commands.Add(Rect(ox - 12, oy, 24, 22, Uniform));
            commands.Add(Rect(ox - 2, oy, 4, 22, ApronRed));
            commands.Add(Rect(ox - 8, oy - 2, 6, 3, Outline));
            commands.Add(Rect(ox + 2, oy - 2, 6, 3, Outline));

            commands.Add(Rect(ox - 8, oy + 24, 16, 14, Skin));
            commands.Add(Rect(ox - 6, oy + 22, 12, 4, SkinShade));
            commands.Add(Rect(ox - 4, oy + 28, 3, 3, Outline));
            commands.Add(Rect(ox + 2, oy + 28, 3, 3, Outline));
            commands.Add(Rect(ox - 1, oy + 24, 4, 2, new Color32(214, 116, 108, 255)));

            commands.Add(Rect(ox - 10, oy + 38, 20, 4, HatWhite));
            commands.Add(Rect(ox - 8, oy + 42, 16, 8, HatWhite));
            commands.Add(Rect(ox - 6, oy + 50, 12, 4, HatWhite));

            commands.Add(Rect(ox - 18, oy + 4, 6, 14, Skin));

            int rightArmY = windUp ? oy + 18 : (followThrough ? oy + 1 : oy + 7);
            int rightArmX = windUp ? ox + 8 : ox + 12;
            commands.Add(Rect(rightArmX, rightArmY, 6, 14, Skin));

            if (windUp)
            {
                AddKnife(commands, ox + 10 + knifeOffsetX, oy + 28 + knifeOffsetY, 6 + knifeTilt, 16, true);
            }
            else if (followThrough)
            {
                AddKnife(commands, ox + 18 + knifeOffsetX, oy + 10 + knifeOffsetY, -10 + knifeTilt, 14, false);
            }
            else
            {
                AddKnife(commands, ox + 16 + knifeOffsetX, oy + 10 + knifeOffsetY, knifeTilt, 18, false);
            }

            int leftLegX = knockedBack ? ox - 10 : ox - 8;
            int rightLegX = knockedBack ? ox + 1 : ox + 2;
            commands.Add(Rect(leftLegX, oy - 6, 7, 6, Outline));
            commands.Add(Rect(rightLegX, oy - 6, 7, 6, Outline));

            if (knockedBack)
            {
                commands.Add(Rect(ox - 14, oy + 24, 3, 8, Outline));
                commands.Add(Rect(ox - 14, oy + 20, 10, 2, new Color32(255, 240, 240, 180)));
            }

            return new FrameData(64, 64, commands.ToArray());
        }

        private static void AddKnife(List<DrawCommand> commands, int handleX, int handleY, int bladeTilt, int bladeLength, bool upward)
        {
            commands.Add(Rect(handleX, handleY, 4, 6, KnifeHandle));

            int direction = upward ? 1 : -1;
            for (int i = 0; i < bladeLength; i += 3)
            {
                int x = handleX + Mathf.RoundToInt(bladeTilt * (i / (float)bladeLength));
                int y = handleY + 6 + (i * direction);
                commands.Add(Rect(x, y, 4, 3, KnifeMetal));
                commands.Add(Rect(x + 1, y + 1, 2, 1, KnifeHighlight));
            }
        }

        private static FrameData CreateMushroomFrame(int swayX, int footShift, bool hitFlash)
        {
            List<DrawCommand> commands = new List<DrawCommand>(32);

            Color32 capMain = hitFlash ? MushroomRedFlash : MushroomCap;
            Color32 capShade = hitFlash ? new Color32(188, 36, 36, 255) : MushroomCapDark;
            int centerX = 32 + swayX;

            commands.Add(Ellipse(centerX, 38, 26, 21, capMain));
            commands.Add(Rect(centerX - 26, 18, 52, 4, capShade));
            commands.Add(Rect(centerX - 18, 34, 6, 6, MushroomSpot));
            commands.Add(Rect(centerX + 4, 40, 6, 6, MushroomSpot));
            commands.Add(Rect(centerX + 14, 30, 5, 5, MushroomSpot));

            commands.Add(Rect(centerX - 10, 4, 20, 17, MushroomStem));
            commands.Add(Rect(centerX - 11, 0, 8, 4, MushroomStem));
            commands.Add(Rect(centerX + 3, 0, 8, 4, MushroomStem));
            commands.Add(Rect(centerX - 13 + footShift, 0, 8, 3, MushroomStem));
            commands.Add(Rect(centerX + 5 - footShift, 0, 8, 3, MushroomStem));

            commands.Add(Rect(centerX - 12, 24, 8, 6, FxWhite));
            commands.Add(Rect(centerX - 10, 24, 4, 4, Outline));
            commands.Add(Rect(centerX + 4, 24, 8, 6, FxWhite));
            commands.Add(Rect(centerX + 6, 24, 4, 4, Outline));
            commands.Add(Rect(centerX - 14, 31, 10, 2, Outline));
            commands.Add(Rect(centerX + 4, 31, 10, 2, Outline));
            commands.Add(Rect(centerX - 6, 19, 12, 3, Outline));

            if (hitFlash)
            {
                commands.Add(Rect(centerX - 6, 18, 12, 2, new Color32(255, 210, 210, 255)));
            }

            return new FrameData(64, 64, commands.ToArray());
        }

        private static FrameData CreateBossFrame(int tentacleShift, int eyeShift, bool rage)
        {
            List<DrawCommand> commands = new List<DrawCommand>(120);

            commands.Add(Ellipse(64, 82, 42, 34, BossBody));
            commands.Add(Ellipse(64, 86, 34, 26, BossBodyLight));
            commands.Add(Rect(44, 108, 40, 6, BossCrown));
            commands.Add(Rect(50, 114, 8, 8, BossCrown));
            commands.Add(Rect(62, 114, 8, 10, BossCrown));
            commands.Add(Rect(74, 114, 8, 8, BossCrown));

            commands.Add(Ellipse(49, 84 + eyeShift, 10, 8, FxWhite));
            commands.Add(Ellipse(79, 84 - eyeShift, 10, 8, FxWhite));
            commands.Add(Rect(46, 82 + eyeShift, 6, 6, Outline));
            commands.Add(Rect(76, 82 - eyeShift, 6, 6, Outline));
            commands.Add(Rect(38, 93, 18, 3, BossBodyDark));
            commands.Add(Rect(72, 93, 18, 3, BossBodyDark));
            commands.Add(Rect(52, 70, 24, 6, BossBodyDark));
            commands.Add(Rect(58, 68, 12, 2, new Color32(255, 132, 132, 255)));

            AddTentacle(commands, 20, 18, 0 + tentacleShift);
            AddTentacle(commands, 32, 20, 3 + tentacleShift);
            AddTentacle(commands, 44, 22, 6 + tentacleShift);
            AddTentacle(commands, 56, 21, 2 - tentacleShift);
            AddTentacle(commands, 68, 21, -2 - tentacleShift);
            AddTentacle(commands, 80, 22, -6 - tentacleShift);
            AddTentacle(commands, 92, 20, -3 - tentacleShift);
            AddTentacle(commands, 104, 18, 0 - tentacleShift);

            if (rage)
            {
                commands.Add(Ellipse(64, 82, 44, 36, RageRed));
                commands.Add(Rect(44, 80, 10, 4, new Color32(255, 82, 82, 255)));
                commands.Add(Rect(74, 80, 10, 4, new Color32(255, 82, 82, 255)));
                commands.Add(Rect(52, 70, 24, 4, new Color32(255, 68, 68, 220)));
            }

            return new FrameData(128, 128, commands.ToArray());
        }

        private static void AddTentacle(List<DrawCommand> commands, int startX, int width, int waveBias)
        {
            for (int step = 0; step < 40; step++)
            {
                int y = 44 - step;
                int x = startX + Mathf.RoundToInt(Mathf.Sin((step + waveBias) * 0.35f) * 4f);
                commands.Add(Rect(x, y, width / 3, 2, BossBody));

                if (step % 6 == 0)
                {
                    commands.Add(Rect(x + 1, y, Mathf.Max(2, width / 4), 1, BossSucker));
                }
            }
        }

        private static FrameData CreateSlashFrame(Color32 slashColor, int offsetX, int offsetY)
        {
            List<DrawCommand> commands = new List<DrawCommand>(18);

            AddDiagonalStroke(commands, 12 + offsetX, 18 + offsetY, 7, 4, slashColor);
            AddDiagonalStroke(commands, 20 + offsetX, 26 + offsetY, 6, 3, slashColor);
            AddDiagonalStroke(commands, 28 + offsetX, 34 + offsetY, 5, 2, slashColor);
            commands.Add(Ellipse(20 + offsetX, 18 + offsetY, 8, 4, slashColor));

            return new FrameData(64, 64, commands.ToArray());
        }

        private static FrameData CreateCritBurstFrame(int radius, Color32 coreColor, Color32 outerColor)
        {
            List<DrawCommand> commands = new List<DrawCommand>(32);
            int cx = 32;
            int cy = 32;

            commands.Add(Ellipse(cx, cy, radius / 3, radius / 3, coreColor));
            commands.Add(Rect(cx - radius, cy - 2, radius * 2, 4, outerColor));
            commands.Add(Rect(cx - 2, cy - radius, 4, radius * 2, outerColor));
            AddDiagonalBurstArm(commands, cx, cy, radius, 1, 1, outerColor);
            AddDiagonalBurstArm(commands, cx, cy, radius, -1, 1, outerColor);
            AddDiagonalBurstArm(commands, cx, cy, radius - 4, 1, -1, outerColor);
            AddDiagonalBurstArm(commands, cx, cy, radius - 4, -1, -1, outerColor);

            return new FrameData(64, 64, commands.ToArray());
        }

        private static FrameData CreateBossDeathFrame(int radius, int thickness, Color32 innerColor, Color32 outerColor)
        {
            List<DrawCommand> commands = new List<DrawCommand>(24);

            commands.Add(Ellipse(64, 64, radius, radius, outerColor));
            commands.Add(Ellipse(64, 64, Mathf.Max(1, radius - thickness), Mathf.Max(1, radius - thickness), Clear));
            commands.Add(Ellipse(64, 64, Mathf.Max(2, radius / 3), Mathf.Max(2, radius / 3), innerColor));
            commands.Add(Rect(64 - radius, 63, radius * 2, 2, outerColor));
            commands.Add(Rect(63, 64 - radius, 2, radius * 2, outerColor));

            return new FrameData(128, 128, commands.ToArray());
        }

        private static void AddDiagonalStroke(List<DrawCommand> commands, int startX, int startY, int segments, int size, Color32 color)
        {
            for (int i = 0; i < segments; i++)
            {
                commands.Add(Rect(startX + (i * 4), startY + (i * 4), size, size, color));
            }
        }

        private static void AddDiagonalBurstArm(List<DrawCommand> commands, int cx, int cy, int radius, int dx, int dy, Color32 color)
        {
            for (int i = 4; i <= radius; i += 4)
            {
                commands.Add(Rect(cx + (dx * i) - 1, cy + (dy * i) - 1, 3, 3, color));
            }
        }

        private static DrawCommand Rect(int x, int y, int w, int h, Color32 color)
        {
            return new DrawCommand(DrawShape.Rect, x, y, w, h, color);
        }

        private static DrawCommand Ellipse(int cx, int cy, int rx, int ry, Color32 color)
        {
            return new DrawCommand(DrawShape.Ellipse, cx, cy, rx, ry, color);
        }

        private static void FillRect(Color32[] pixels, int width, int height, int x, int y, int w, int h, Color32 color)
        {
            int maxX = Mathf.Min(width, x + w);
            int maxY = Mathf.Min(height, y + h);

            for (int py = Mathf.Max(0, y); py < maxY; py++)
            {
                int rowOffset = py * width;
                for (int px = Mathf.Max(0, x); px < maxX; px++)
                {
                    pixels[rowOffset + px] = color;
                }
            }
        }

        private static void FillEllipse(Color32[] pixels, int width, int height, int cx, int cy, int rx, int ry, Color32 color)
        {
            for (int y = cy - ry; y <= cy + ry; y++)
            {
                if (y < 0 || y >= height)
                {
                    continue;
                }

                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    if (x < 0 || x >= width)
                    {
                        continue;
                    }

                    float dx = (x - cx) / (float)rx;
                    float dy = (y - cy) / (float)ry;
                    if ((dx * dx) + (dy * dy) <= 1f)
                    {
                        pixels[(y * width) + x] = color;
                    }
                }
            }
        }
    }
}
