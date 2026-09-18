using SDL;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Drawing;

namespace Daybreak.UI
{
    public struct Origin
    {
        public float x;
        public float y;
    }
    public struct UDim
    {
        public float xScale;
        public float yScale;
        public float xOffset;
        public float yOffset;

        public static UDim UseOffset(float xOffset, float yOffset) => new(){xScale=0, yScale=0, xOffset=xOffset, yOffset=yOffset};
        public static UDim UseScale(float xScale, float yScale) => new(){xScale=xScale, yScale=yScale, xOffset=0, yOffset=0};
        public static UDim Zero() => new(){xScale=0, yScale=0, xOffset=0, yOffset=0};

        public static UDim CalcAbs(UDim udim, int sh, int sw) => new()
            {
                xScale = 0,
                yScale = 0,
                xOffset = udim.xOffset + (Math.Clamp(udim.xScale, 0, 1) * sw),
                yOffset = udim.yOffset + (Math.Clamp(udim.yScale, 0, 1) * sh),
            };
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
        public Origin Origin { get; set; }
        public ColorRGBA Color { get; set; }

        public abstract void Update(float dt);
        public abstract void HandleEvent(SDL_Event* evnt);
    }

    public unsafe class DrawRectBatch
    {
        public static SDL_Renderer* Renderer { get; set; } 
        public static SDL_Window* Window { get; set; }
        public static int WindowWidth { get; private set; }
        public static int WindowHeight { get; private set; }

        private SDL_Vertex[] _vertices;
        private int[] _indices;
        private int _vertexCount;
        private int _indexCount;

        public DrawRectBatch(SDL_Renderer* renderer, SDL_Window* window, int initialRectCapacity = 512)
        {
            Renderer = renderer;
            Window = window;

            _vertices = new SDL_Vertex[initialRectCapacity * 4];
            _indices = new int[initialRectCapacity * 6];
        }
        public static void WindowResizedEvent()
        {
            int w = 0, h = 0;
            SDL3.SDL_GetWindowSize(Window, &w, &h);
            WindowWidth = w;
            WindowHeight = h;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Begin()
        {
            _vertexCount = 0;
            _indexCount = 0;
        }
        public void Push(Element element)
        {
            EnsureCapacity(1);

            UDim pos = element.Position;
            UDim sz = element.Size;

            if (pos.xScale != 0 || pos.yScale != 0) 
                pos = UDim.CalcAbs(pos, WindowHeight, WindowWidth);
            if (sz.xScale != 0 || sz.yScale != 0) 
                sz = UDim.CalcAbs(sz, WindowHeight, WindowWidth);

            float originX = Math.Clamp(element.Origin.x, 0, 1) * sz.xOffset;
            float originY = Math.Clamp(element.Origin.y, 0, 1) * sz.yOffset;

            float x0 = pos.xOffset - originX;
            float y0 = pos.yOffset - originY;
            float x1 = x0 + sz.xOffset;
            float y1 = y0 + sz.yOffset;

            ColorRGBA c = element.Color;
            SDL_FColor color = new()
            {
                r = c.R / 255f,
                g = c.G / 255f,
                b = c.B / 255f,
                a = c.A / 255f
            };

            int baseVertex = _vertexCount;

            _vertices[baseVertex + 0] = new SDL_Vertex { position = new SDL_FPoint { x = x0, y = y0 }, color = color };
            _vertices[baseVertex + 1] = new SDL_Vertex { position = new SDL_FPoint { x = x1, y = y0 }, color = color };
            _vertices[baseVertex + 2] = new SDL_Vertex { position = new SDL_FPoint { x = x1, y = y1 }, color = color };
            _vertices[baseVertex + 3] = new SDL_Vertex { position = new SDL_FPoint { x = x0, y = y1 }, color = color };

            int baseIndex = _indexCount;
            _indices[baseIndex + 0] = baseVertex + 0;
            _indices[baseIndex + 1] = baseVertex + 1;
            _indices[baseIndex + 2] = baseVertex + 2;
            _indices[baseIndex + 3] = baseVertex + 2;
            _indices[baseIndex + 4] = baseVertex + 3;
            _indices[baseIndex + 5] = baseVertex + 0;

            _vertexCount += 4;
            _indexCount += 6;
        }
        public void End()
        {
            if (_vertexCount == 0) return;

            fixed (SDL_Vertex* pVertices = _vertices)
            fixed (int* pIndices = _indices)
            {
                SDL3.SDL_RenderGeometry(
                    Renderer,
                    null,
                    pVertices,
                    _vertexCount,
                    pIndices,
                    _indexCount
                );
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int rectCountToAdd)
        {
            int neededVertices = _vertexCount + (rectCountToAdd * 4);
            int neededIndices = _indexCount + (rectCountToAdd * 6);

            if (neededVertices > _vertices.Length)
            {
                Array.Resize(ref _vertices, Math.Max(_vertices.Length * 2, neededVertices));
            }

            if (neededIndices > _indices.Length)
            {
                Array.Resize(ref _indices, Math.Max(_indices.Length * 2, neededIndices));
            }
        }
    }

    public class Button : Element
    {
        public override unsafe void HandleEvent(SDL_Event* evnt)
        {
            float x, y;
            SDL_MouseButtonFlags buttons = SDL3.SDL_GetMouseState(&x, &y);

            UDim pos = Position;
            UDim sz = Size;

            if (pos.xScale != 0 || pos.yScale != 0) 
                pos = UDim.CalcAbs(pos, DrawRectBatch.WindowHeight, DrawRectBatch.WindowWidth);
            if (sz.xScale != 0 || sz.yScale != 0) 
                sz = UDim.CalcAbs(sz, DrawRectBatch.WindowHeight, DrawRectBatch.WindowWidth);

            float originX = Math.Clamp(Origin.x, 0, 1) * sz.xOffset;
            float originY = Math.Clamp(Origin.y, 0, 1) * sz.yOffset;

            float x0 = pos.xOffset - originX;
            float y0 = pos.yOffset - originY;
            float x1 = x0 + sz.xOffset;
            float y1 = y0 + sz.yOffset;

            if (x > x0 && y > y0 && x < x1 && y < y1)
            {
                if (evnt->type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN){
                    Console.WriteLine("Button Down");
                    if (evnt->button.button == SDL3.SDL_BUTTON_LEFT)
                    {
                        Console.WriteLine("Clicked");
                    }
                }
            }
        }
        public override void Update(float dt)
        {

        }
    }
}