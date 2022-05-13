using System;

namespace GetJobProfiles
{
    public static class ColorConsole
    {
        // global kill switch for when you just want your temporary Console.WriteLine to output
        private const bool Enable = false;
        public static void WriteLine(string str, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            if (!Enable)
                return;

            ConsoleColor? currentBackgroundColor = null, currentForegroundColor = null;
            if (foreground != null)
                currentForegroundColor = Console.ForegroundColor;
            if (background != null)
                currentBackgroundColor = Console.BackgroundColor;
            Console.WriteLine(str);
            if (currentForegroundColor != null)
                Console.ForegroundColor = currentForegroundColor.Value;
            if (currentBackgroundColor != null)
                Console.BackgroundColor = currentBackgroundColor.Value;
        }
    }
}
