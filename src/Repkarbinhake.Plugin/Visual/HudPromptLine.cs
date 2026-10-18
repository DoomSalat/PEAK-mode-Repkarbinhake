namespace Repkarbinhake.Plugin.Visual
{
    public readonly struct HudPromptLine
    {
        public HudPromptLine(string text, string keyPath)
        {
            Text = text;
            KeyPath = keyPath;
        }

        public string Text { get; }
        public string KeyPath { get; }
    }
}
