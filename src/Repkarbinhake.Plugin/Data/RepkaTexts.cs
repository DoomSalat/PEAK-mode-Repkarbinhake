namespace Repkarbinhake.Plugin.Data
{
    public static class RepkaTexts // Static: shared UI strings of the mod, no state
    {
        // Tables follow LocalizedText.Language order, English first
        private static readonly string[] RopeNames = { "Rope", "Corde", "Corda", "Seil", "Cuerda", "Cuerda", "Corda", "Верёвка", "Мотузка", "绳子", "繩子", "ロープ", "밧줄", "Lina", "Halat" }; // Static: fixed translation table
        private static readonly string[] Grabs = { "Grab rope", "Saisir la corde", "Afferra la corda", "Seil greifen", "Agarrar la cuerda", "Agarrar la cuerda", "Pegar a corda", "Взять верёвку", "Взяти мотузку", "抓住绳子", "抓住繩子", "ロープをつかむ", "밧줄 잡기", "Chwyć linę", "Halatı tut" }; // Static: fixed translation table
        private static readonly string[] Releases = { "Let go of rope", "Lâcher la corde", "Lascia la corda", "Seil loslassen", "Soltar la cuerda", "Soltar la cuerda", "Soltar a corda", "Отпустить верёвку", "Відпустити мотузку", "松开绳子", "鬆開繩子", "ロープを放す", "밧줄 놓기", "Puść linę", "Halatı bırak" }; // Static: fixed translation table
        private static readonly string[] Ties = { "Tie rope", "Attacher la corde", "Lega la corda", "Seil anbinden", "Atar la cuerda", "Amarrar la cuerda", "Amarrar a corda", "Привязать верёвку", "Прив'язати мотузку", "系上绳子", "繫上繩子", "ロープを結ぶ", "밧줄 묶기", "Przywiąż linę", "Halatı bağla" }; // Static: fixed translation table
        private static readonly string[] Unties = { "Untie rope", "Détacher la corde", "Slega la corda", "Seil lösen", "Desatar la cuerda", "Desamarrar la cuerda", "Desamarrar a corda", "Отцепить верёвку", "Відчепити мотузку", "解开绳子", "解開繩子", "ロープをほどく", "밧줄 풀기", "Odwiąż linę", "Halatı çöz" }; // Static: fixed translation table

        public static string RopeName => Pick(RopeNames);
        public static string Grab => Pick(Grabs);
        public static string Release => Pick(Releases);
        public static string Tie => Pick(Ties);
        public static string Untie => Pick(Unties);

        private static string Pick(string[] translations) // Static: reads the game's current language
        {
            int language = (int)LocalizedText.CURRENT_LANGUAGE;
            return language >= 0 && language < translations.Length ? translations[language] : translations[0];
        }
    }
}
