using Daybreak.UI;
using SDL;

public class Program
{
    const int ScreenHeight = 720;
    const int ScreenWidth = 1280;
    public unsafe static void Main()
    {
        if (!SDL3.SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO)) {Console.WriteLine("Cant initiate SDL"); return;}
        if (!SDL3_ttf.TTF_Init()) {Console.WriteLine("Cant initiate text rendering"); return;}

        SDL_Window* window = SDL3.SDL_CreateWindow("Daybreak", ScreenWidth, ScreenHeight, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);
        if (window == null) {Console.WriteLine("Cant create window"); SDL3.SDL_Quit(); return;}

        SDL_Renderer* renderer = SDL3.SDL_CreateRenderer(window, string.Empty);
        if (renderer == null) {Console.WriteLine("Cant create renderer"); SDL3.SDL_Quit(); return;}

        DrawRectBatch.Renderer = renderer;
        ulong last_counter = SDL3.SDL_GetPerformanceCounter();
        ulong frequency = SDL3.SDL_GetPerformanceFrequency();
        
        // Font testing
        // TTF_Font* font = SDL3_ttf.TTF_OpenFont(Path.Join(AppDomain.CurrentDomain.BaseDirectory, "GoogleSans-Regular.ttf"), 24);
        // SDL_Surface* textSurface = SDL3_ttf.TTF_RenderText_Blended(font, "Hello, this is a test", 0, new(){r=0, g=0, b=0, a=255});
        // SDL_Texture* textTexture = null;
        // SDL_FRect textRect = new();

        // if (textSurface is not null)
        // {
        //     textRect.w = textSurface->w;
        //     textRect.h = textSurface->h;
        //     textRect.x = 200;
        //     textRect.y = 200;

        //     textTexture = SDL3.SDL_CreateTextureFromSurface(renderer, textSurface);
        //     SDL3.SDL_DestroySurface(textSurface);
        // }

        DrawRectBatch batch1 = new();
        Button newButton = new(){ Position = UDim.UseOffset(100, 200), Size = UDim.UseOffset(200, 200), Color = ColorRGBA.GREEN };
        Button newButton2 = new(){ Position = UDim.UseOffset(200, 400), Size = UDim.UseOffset(200, 200), Color = ColorRGBA.RED };

        batch1.AddBatch(newButton);
        batch1.AddBatch(newButton2);

        bool isRunning = true;
        SDL_Event evnt;
        while (isRunning)
        {
            while (SDL3.SDL_PollEvent(&evnt))
            {
                switch (evnt.type)
                {
                    case (uint)SDL_EventType.SDL_EVENT_QUIT:
                        isRunning = false;
                        break;

                    case (uint)SDL_EventType.SDL_EVENT_KEY_DOWN:
                        switch (evnt.key.key)
                        {
                            case SDL_Keycode.SDLK_SPACE:
                                Console.WriteLine("Space is Pressed");
                                break;
                        }
                        break;
                }

            }

            ulong current_counter = SDL3.SDL_GetPerformanceCounter();
            double dt = current_counter - last_counter / frequency;
            last_counter = current_counter;

            newButton.Update((float)dt);
            // newButton2.Update((float)dt);

            SDL3.SDL_SetRenderDrawColor(renderer, 255, 255, 255, 255);
            SDL3.SDL_RenderClear(renderer);

            batch1.DrawBatch();

            // if (textTexture is not null)
            // {
            //     SDL3.SDL_RenderTexture(renderer, textTexture, null, &textRect);
            // }

            SDL3.SDL_RenderPresent(renderer);
        }

        // SDL3_ttf.TTF_CloseFont(font);
        // SDL3.SDL_DestroyTexture(textTexture);
        SDL3.SDL_DestroyRenderer(renderer);
        SDL3.SDL_DestroyWindow(window);
        SDL3.SDL_Quit();
    }
}