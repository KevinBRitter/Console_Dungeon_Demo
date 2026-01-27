using Console_Dungeon.UI;

namespace Console_Dungeon.Tests
{
    public class ScreenRendererTests
    {
        [Fact]
        public void DrawScreen_IncludesHeaderAndFooterAndSeparator()
        {
            var sw = new StringWriter();
            var originalOut = ScreenRenderer.Output;
            try
            {
                ScreenRenderer.Output = sw;
                // call the simplified API that relies on defaults
                ScreenRenderer.DrawScreen("Main Menu\n\n  1) Play\n  2) Options\n  3) Exit\n");
                string output = sw.ToString();

                Assert.Contains("Console Dungeon", output); // default header
                Assert.Contains("a demo project by Kevin Ritter", output); // default footer
                // separator is a full-width line beginning and ending with '+'
                Assert.Contains("+", output);
            }
            finally
            {
                ScreenRenderer.Output = originalOut;
            }
        }

        [Fact]
        public void TextFormatter_WrapText_DoesNotProduceLinesLongerThanMaxWidth()
        {
            string text = "This is a long line that should be wrapped into multiple lines to fit the width.";
            int maxWidth = 20;
            var lines = TextFormatter.WrapText(text, maxWidth);

            Assert.NotEmpty(lines);
            foreach (var line in lines)
            {
                Assert.True(line.Length <= maxWidth, $"Line was too long: '{line}' ({line.Length} > {maxWidth})");
            }
        }
    }
}