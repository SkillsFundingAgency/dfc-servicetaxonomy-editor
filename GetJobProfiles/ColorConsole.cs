using System;

namespace GetJobProfiles
{
    public static class ColorConsole
    {
        public static void WriteLine(string str, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
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
