namespace Console_Dungeon.Input
{
    public static class InputHandler
    {
        public static string GetMenuChoice()
        {
            Console.Write("\n> ");
            return Console.ReadLine()?.Trim() ?? "";
        }

        public static void WaitForKey()
        {
            Console.ReadKey(true);
        }
    }
}
