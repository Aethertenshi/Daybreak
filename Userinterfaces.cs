using SDL;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Drawing;

namespace Daybreak.UI
{
    public struct UDim
    {
        public float xScale;
        public float yScale;
        public float xOffset;
        public float yOffset;

        public static UDim UseOffset(float xOffset, float yOffset) => new(){xScale=0, yScale=0, xOffset=xOffset, yOffset=yOffset};
        public static UDim UseScale(float xScale, float yScale) => new(){xScale=xScale, yScale=yScale, xOffset=0, yOffset=0};
        public static UDim Zero() => new(){xScale=0, yScale=0, xOffset=0, yOffset=0};
    }
    public struct ColorRGBA
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public static ColorRGBA WHITE => new(){ R = 255, G = 255, B = 255, A = 255 };
        public static ColorRGBA BLACK => new(){ R = 0, G = 0, B = 0, A = 255 };
        public static ColorRGBA RED => new(){ R = 255, G = 0, B = 0, A = 255 };
        public static ColorRGBA GREEN => new(){ R = 0, G = 255, B = 0, A = 255 };
        public static ColorRGBA BLUE => new(){ R = 0, G = 0, B = 255, A = 255 };
        
        public static implicit operator SDL_FColor(ColorRGBA color) => new(){ r = color.R, g = color.G, b = color.B, a = color.A};
        public static implicit operator ColorRGBA(SDL_FColor fcolor) => new(){ R = fcolor.r, G = fcolor.g, B = fcolor.b, A = fcolor.a};
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint RgbaToUint(ColorRGBA color)
        {
            return ((uint)color.R << 24) | 
                ((uint)color.G << 16) | 
                ((uint)color.B << 8)  | 
                (uint)color.A;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ColorRGBA UintToRgba(uint color)
        {
            return new()
            {
                R = (byte)((color >> 24) & 0xFF),
                G = (byte)((color >> 16) & 0xFF),
                B = (byte)((color >> 8) & 0xFF),
                A = (byte)(color & 0xFF)
            };
        }
    }

    public abstract unsafe class Element
    {
        public static SDL_Renderer* renderer { get; set; }
        public UDim Position { get; set; }
        public UDim Size { get; set; }
        public ColorRGBA Color { get; set; }

        public abstract void Update(float dt);
    }
    public unsafe class DrawRectBatch
    {
        public static SDL_Renderer* Renderer { get; set; } 
        private static Dictionary<uint, List<SDL_FRect>> colorBatches = [];

        public void AddBatch(Element element)
        {
            UDim Position = element.Position;
            UDim Size = element.Size;
            ColorRGBA Color = element.Color;

            if (!colorBatches.TryGetValue(ColorRGBA.RgbaToUint(Color), out var custlist))
            {
                custlist = new();
                colorBatches[ColorRGBA.RgbaToUint(Color)] = custlist;
            }

            custlist.Add(new(){x=Position.xOffset, y=Position.yOffset, h=Size.yOffset, w=Size.xOffset});
        }
        public void DrawBatch()
        {
            foreach (var kvp in colorBatches)
            {
                List<SDL_FRect> rects = kvp.Value;
                
                if (rects.Count == 0) continue;

                ColorRGBA color = ColorRGBA.UintToRgba(kvp.Key);

                // Set state ONCE per color group
                SDL3.SDL_SetRenderDrawColor(Renderer, (byte)color.R, (byte)color.G, (byte)color.B, (byte)color.A);

                // Draw the whole batch zero-copy
                Span<SDL_FRect> rectSpan = CollectionsMarshal.AsSpan(rects);
                fixed (SDL_FRect* rectPtr = rectSpan)
                {
                    SDL3.SDL_RenderFillRects(Renderer, rectPtr, rects.Count);
                }
            }
        }
    }
    public class Button : Element
    {
        public override unsafe void Update(float dt)
        {
            float x, y;
            SDL_MouseButtonFlags buttons = SDL3.SDL_GetMouseState(&x, &y);

            if (x > Position.xOffset && y > Position.yOffset && x < (Position.xOffset + Size.xOffset) && y < (Position.yOffset + Size.yOffset))
            {
                Console.WriteLine("Hovered!");
            }
        }
    }
}