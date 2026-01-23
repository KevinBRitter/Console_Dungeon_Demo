using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Console_Dungeon.UI
{
    public static class ScreenRenderer
    {
        // Allow tests or other callers to capture output by swapping this writer.
        // Default remains the real console output.
        public static TextWriter Output = Console.Out;

        private const int ScreenWidth = 80;  // Standard console width, a little wide but saves room for minimap if needed
        private const int ScreenHeight = 20; // 25 fits, but doesn't leave room for user input
        private const int PaddingLeft = 4; // Leave room for border plus some padding white space
        private const int PaddingRight = 4;
        private const int PaddingTop = 1; // Leave room for border no white space
        private const int PaddingBottom = 1;

        // Default header/footer (used when caller passes null).
        private const string DefaultHeader = "Console Dungeon";
        private const string DefaultFooter = "a demo project by Kevin Ritter";

        // Backwards-compatible single-string API
        public static void DrawScreen(string text)
            => DrawScreen(null, text, null, '+');

        // New API: header/body/footer + separator char
        // Behavior:
        //  - header == null => use DefaultHeader
        //  - header == ""   => no header
        //  - footer == null => use DefaultFooter
        //  - footer == ""   => no footer
        public static void DrawScreen(string? header, string? body, string? footer, char separator = '+')
        {
            int textWidth = ScreenWidth - 2 - PaddingLeft - PaddingRight;
            int textHeight = ScreenHeight - 2 - PaddingTop - PaddingBottom;

            // Interpret header/footer null as defaults, empty string as "no section"
            List<string> headerLines;
            if (header == string.Empty)
                headerLines = new List<string>();
            else
                headerLines = TextFormatter.WrapText(header ?? DefaultHeader, textWidth);

            List<string> bodyLines = string.IsNullOrEmpty(body) ? new List<string>() : TextFormatter.WrapText(body, textWidth);

            List<string> footerLines;
            if (footer == string.Empty)
                footerLines = new List<string>();
            else
                footerLines = TextFormatter.WrapText(footer ?? DefaultFooter, textWidth);

            bool hasHeader = headerLines.Count > 0;
            bool hasFooter = footerLines.Count > 0;

            // Contributions measured in content-area rows (no external top/bottom padding lines)
            // Header contribution: header lines + one margin line + one separator line
            int headerContribution = hasHeader ? (headerLines.Count + 1 + 1) : 0;
            // Footer contribution: one separator + one margin + footer lines
            int footerContribution = hasFooter ? (1 + 1 + footerLines.Count) : 0;
            int bodyContribution = bodyLines.Count;

            int contentCount = headerContribution + bodyContribution + footerContribution;

            // Truncate body first, then footer (from top), then header (from bottom) until content fits textHeight
            while (contentCount > textHeight)
            {
                if (bodyContribution > 0)
                {
                    // truncate from end of body (keep top of body)
                    bodyLines.RemoveAt(bodyLines.Count - 1);
                    bodyContribution--;
                }
                else if (footerContribution > 0 && footerLines.Count > 0)
                {
                    // remove from top of footer to move footer upward
                    footerLines.RemoveAt(0);
                    footerContribution = 1 + 1 + footerLines.Count;
                }
                else if (headerContribution > 0 && headerLines.Count > 0)
                {
                    // remove from bottom of header
                    headerLines.RemoveAt(headerLines.Count - 1);
                    headerContribution = headerLines.Count + 1 + 1;
                }
                else
                {
                    break;
                }

                contentCount = headerContribution + bodyContribution + footerContribution;
            }

            // Build content array of exact height, fill with empty strings
            var content = Enumerable.Repeat(string.Empty, textHeight).ToArray();

            int cursor = 0;

            // Header placement:
            // We want header text to appear on the 3rd overall line (top border, external top margin, header text).
            // Because external margin (PaddingTop) is printed before content, place headerLines starting at content index 0.
            if (hasHeader && headerLines.Count > 0)
            {
                // place header lines starting at content[0]
                foreach (var hl in headerLines)
                {
                    if (cursor >= textHeight) break;
                    content[cursor++] = hl;
                }

                // one margin line after header text
                if (cursor < textHeight)
                    content[cursor++] = string.Empty;

                // header separator
                if (cursor < textHeight)
                    content[cursor++] = separator.ToString();
            }

            // Body: fill starting at cursor, but stop before footer region
            int footerRegionStart = textHeight;
            if (hasFooter && footerLines.Count > 0)
            {
                // Reserve footerContribution rows at the bottom (separator + margin + footer lines)
                footerRegionStart = Math.Max(cursor, textHeight - footerContribution);
            }

            int bodyIdx = 0;
            while (cursor < footerRegionStart && bodyIdx < bodyLines.Count)
            {
                content[cursor++] = bodyLines[bodyIdx++];
            }

            // Footer: place separator and footer block such that separator is the fifth-from-bottom overall:
            // reserve content[textHeight - footerContribution] ... content[textHeight-1] for the footer block
            if (hasFooter && footerLines.Count > 0)
            {
                int sepIndex = Math.Max(cursor, textHeight - footerContribution);
                if (sepIndex < textHeight)
                    content[sepIndex] = separator.ToString();

                int marginAfterSep = sepIndex + 1;
                if (marginAfterSep < textHeight)
                    content[marginAfterSep] = string.Empty;

                int footerStart = sepIndex + 2;
                for (int i = 0; i < footerLines.Count && footerStart + i < textHeight; i++)
                {
                    content[footerStart + i] = footerLines[i];
                }

                // Do NOT add an extra bottom margin here; PaddingBottom supplies the final empty line before the outer border.
            }

            // Render
            try { Console.Clear(); } catch { /* ignore when no console is attached during tests */ }

            Output.WriteLine(new string('*', ScreenWidth));

            for (int i = 0; i < PaddingTop; i++)
                PrintEmptyLine();

            for (int i = 0; i < textHeight; i++)
            {
                string line = i < content.Length ? content[i] : "";

                if (line.Length == 1 && line[0] == separator)
                    PrintSeparatorLine(separator);
                else
                    PrintTextLine(line);
            }

            for (int i = 0; i < PaddingBottom; i++)
                PrintEmptyLine();

            Output.WriteLine(new string('*', ScreenWidth));
        }

        private static void PrintEmptyLine()
        {
            Output.WriteLine("*" + new string(' ', ScreenWidth - 2) + "*");
        }

        private static void PrintTextLine(string text)
        {
            int innerWidth = ScreenWidth - 2;

            string paddedText =
                new string(' ', PaddingLeft) +
                text.PadRight(innerWidth - PaddingLeft - PaddingRight) +
                new string(' ', PaddingRight);

            Output.WriteLine("*" + paddedText + "*");
        }

        private static void PrintSeparatorLine(char sep)
        {
            // Print a separator that spans the full screen width as: +-------------------------+
            // This replaces the usual outer '*' characters on that row.
            string line = sep.ToString() + new string('-', Math.Max(0, ScreenWidth - 2)) + sep.ToString();
            Output.WriteLine(line);
        }
    }
}
