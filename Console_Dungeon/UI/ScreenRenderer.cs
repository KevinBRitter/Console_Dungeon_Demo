namespace Console_Dungeon.UI
{
    public static class ScreenRenderer
    {
        private const int ScreenWidth = 80;  // Standard console width, a little wide but saves room for minimap if needed
        private const int ScreenHeight = 20; // 25 fits, but doesn't leave room for user input
        private const int PaddingLeft = 4; // Leave room for border plus some padding white space
        private const int PaddingRight = 4;
        private const int PaddingTop = 1; // Leave room for border no white space
        private const int PaddingBottom = 1;

        public static void DrawScreen(string text)
        {
            // TODO: add a way to customize line endings to section off different parts of the screen
            // I'm thinking a replaceable character like + that is replaced with +----- etc ----+ as a line
            // Perhaps each DrawScreen call should take in header, body, and footer strings separately?
            int textWidth = ScreenWidth - 2 - PaddingLeft - PaddingRight;
            int textHeight = ScreenHeight - 2 - PaddingTop - PaddingBottom;

            var wrappedLines = TextFormatter.WrapText(text, textWidth);

            Console.Clear();
            Console.WriteLine(new string('*', ScreenWidth));

            for (int i = 0; i < PaddingTop; i++)
                PrintEmptyLine();

            for (int i = 0; i < textHeight; i++)
            {
                string line = i < wrappedLines.Count ? wrappedLines[i] : "";
                PrintTextLine(line);
            }

            for (int i = 0; i < PaddingBottom; i++)
                PrintEmptyLine();

            Console.WriteLine(new string('*', ScreenWidth));
        }

        private static void PrintEmptyLine()
        {
            Console.WriteLine("*" + new string(' ', ScreenWidth - 2) + "*");
        }

        private static void PrintTextLine(string text)
        {
            int innerWidth = ScreenWidth - 2;

            string paddedText =
                new string(' ', PaddingLeft) +
                text.PadRight(innerWidth - PaddingLeft - PaddingRight) +
                new string(' ', PaddingRight);

            Console.WriteLine("*" + paddedText + "*");
        }
    }
}
