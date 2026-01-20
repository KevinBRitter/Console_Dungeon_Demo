namespace Console_Dungeon.UI
{
    public static class TextFormatter
    {
        public static List<string> WrapText(string text, int maxWidth)
        {
            var result = new List<string>();

            var rawLines = text.Split('\n', StringSplitOptions.None);

            foreach (var rawLine in rawLines)
            {
                if (string.IsNullOrWhiteSpace(rawLine))
                {
                    result.Add("");
                    continue;
                }

                var words = rawLine.Split(' ');
                var currentLine = "";

                foreach (var word in words)
                {
                    if ((currentLine + word).Length > maxWidth)
                    {
                        result.Add(currentLine.TrimEnd());
                        currentLine = "";
                    }

                    currentLine += word + " ";
                }

                if (currentLine.Length > 0)
                    result.Add(currentLine.TrimEnd());
            }

            return result;
        }
    }

}
