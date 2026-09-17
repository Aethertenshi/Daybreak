# Daybreak
Daybreak is an ad-free sandbox game centered on creative base building, farming diverse crops, and trading harvests to expand your settlement.

## Technical Architecture
Commercial engines often force awkward workarounds and heavy abstractions for systems that should be simple and predictable. To avoid fighting someone else’s architecture, Daybreak is built from the ground up using C# and [SDL3-CS](https://github.com/ppy/SDL3-CS). This setup pairs the low-overhead performance of SDL3 with the productivity and fast iteration cycles of modern C#.

## Building from Source

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/download) (or later)
* Git

### Quick Start
1. Clone the repository:
   ```bash
   git clone [https://github.com/your-username/daybreak.git](https://github.com/your-username/daybreak.git)
   cd daybreak
   ```
2. Restore dependencies and run:
    ```bash
    dotnet run
    ```

(Native SDL3 runtimes are pulled automatically via the SDL3-CS package during restore, so no manual library linking is required.)