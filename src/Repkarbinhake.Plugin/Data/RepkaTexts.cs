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

        private static readonly string[] Unwinds = { "Let out", "Dérouler", "Srotola", "Abwickeln", "Desenrollar", "Desenrollar", "Desenrolar", "Размотать", "Розмотати", "放绳", "放繩", "繰り出す", "풀어주기", "Popuść", "Sal" }; // Static: fixed translation table
        private static readonly string[] Pulls = { "Pull in", "Tirer", "Tira", "Heranziehen", "Tirar", "Jalar", "Puxar", "Подтянуть", "Підтягнути", "拉近", "拉近", "引き寄せる", "당기기", "Przyciągnij", "Çek" }; // Static: fixed translation table

        private static readonly string[] UntiedFromOnes = { "{0} untied from player: {1}", "{0} s'est détaché du joueur : {1}", "{0} si è slegato dal giocatore: {1}", "{0} hat sich von Spieler gelöst: {1}", "{0} se desató del jugador: {1}", "{0} se desamarró del jugador: {1}", "{0} se desamarrou do jogador: {1}", "{0} отцепился от игрока: {1}", "{0} відчепився від гравця: {1}", "{0} 解开了与玩家的绳子：{1}", "{0} 解開了與玩家的繩子：{1}", "{0} がプレイヤーとのロープを外した：{1}", "{0} 님이 플레이어와 밧줄을 풀었습니다: {1}", "{0} odczepił się od gracza: {1}", "{0} oyuncudan ayrıldı: {1}" }; // Static: fixed translation table
        private static readonly string[] UntiedFromManys = { "{0} untied from players: {1}", "{0} s'est détaché des joueurs : {1}", "{0} si è slegato dai giocatori: {1}", "{0} hat sich von Spielern gelöst: {1}", "{0} se desató de los jugadores: {1}", "{0} se desamarró de los jugadores: {1}", "{0} se desamarrou dos jogadores: {1}", "{0} отцепился от игроков: {1}", "{0} відчепився від гравців: {1}", "{0} 解开了与玩家的绳子：{1}", "{0} 解開了與玩家的繩子：{1}", "{0} がプレイヤーとのロープを外した：{1}", "{0} 님이 플레이어들과 밧줄을 풀었습니다: {1}", "{0} odczepił się od graczy: {1}", "{0} oyunculardan ayrıldı: {1}" }; // Static: fixed translation table

        private static readonly string[] TiesAlloweds = { "The host allowed tying and untying", "L'hôte a autorisé à s'attacher et se détacher", "L'host ha consentito di legarsi e slegarsi", "Der Host hat Anbinden und Lösen erlaubt", "El anfitrión permitió atarse y desatarse", "El anfitrión permitió amarrarse y desamarrarse", "O anfitrião permitiu amarrar e desamarrar", "Хост разрешил сцепляться и расцепляться", "Хост дозволив зчіплюватися й розчіплюватися", "房主允许系上和解开绳子", "房主允許繫上和解開繩子", "ホストがロープの連結と解除を許可した", "호스트가 밧줄 연결과 해제를 허용했습니다", "Host zezwolił na łączenie i odczepianie", "Ev sahibi bağlanmaya ve ayrılmaya izin verdi" }; // Static: fixed translation table
        private static readonly string[] TiesForbiddens = { "The host disabled tying and untying", "L'hôte a interdit de s'attacher et se détacher", "L'host ha vietato di legarsi e slegarsi", "Der Host hat Anbinden und Lösen verboten", "El anfitrión prohibió atarse y desatarse", "El anfitrión prohibió amarrarse y desamarrarse", "O anfitrião proibiu amarrar e desamarrar", "Хост запретил сцепляться и расцепляться", "Хост заборонив зчіплюватися й розчіплюватися", "房主禁止系上和解开绳子", "房主禁止繫上和解開繩子", "ホストがロープの連結と解除を禁止した", "호스트가 밧줄 연결과 해제를 금지했습니다", "Host zabronił łączenia i odczepiania", "Ev sahibi bağlanmayı ve ayrılmayı yasakladı" }; // Static: fixed translation table
        private static readonly string[] HooksAlloweds = { "The host allowed safety hooks (T)", "L'hôte a autorisé les crochets d'assurage (T)", "L'host ha consentito i ganci di sicura (T)", "Der Host hat Sicherungshaken (T) erlaubt", "El anfitrión permitió los ganchos de seguro (T)", "El anfitrión permitió los ganchos de seguro (T)", "O anfitrião permitiu os ganchos de segurança (T)", "Хост разрешил ставить страховку (T)", "Хост дозволив ставити страховку (T)", "房主允许放置保护钩 (T)", "房主允許放置保護鉤 (T)", "ホストが確保フック(T)を許可した", "호스트가 확보 갈고리(T)를 허용했습니다", "Host zezwolił na haki asekuracyjne (T)", "Ev sahibi emniyet kancalarına (T) izin verdi" }; // Static: fixed translation table
        private static readonly string[] HooksForbiddens = { "The host disabled safety hooks (T)", "L'hôte a interdit les crochets d'assurage (T)", "L'host ha vietato i ganci di sicura (T)", "Der Host hat Sicherungshaken (T) verboten", "El anfitrión prohibió los ganchos de seguro (T)", "El anfitrión prohibió los ganchos de seguro (T)", "O anfitrião proibiu os ganchos de segurança (T)", "Хост запретил ставить страховку (T)", "Хост заборонив ставити страховку (T)", "房主禁止放置保护钩 (T)", "房主禁止放置保護鉤 (T)", "ホストが確保フック(T)を禁止した", "호스트가 확보 갈고리(T)를 금지했습니다", "Host zabronił haków asekuracyjnych (T)", "Ev sahibi emniyet kancalarını (T) yasakladı" }; // Static: fixed translation table

        public static string RopeName => Pick(RopeNames);
        public static string Pull => Pick(Pulls);
        public static string Unwind => Pick(Unwinds);
        public static string Grab => Pick(Grabs);
        public static string Release => Pick(Releases);
        public static string Tie => Pick(Ties);
        public static string Untie => Pick(Unties);
        public static string TiesAllowed => Pick(TiesAlloweds);
        public static string TiesForbidden => Pick(TiesForbiddens);
        public static string HooksAllowed => Pick(HooksAlloweds);
        public static string HooksForbidden => Pick(HooksForbiddens);

        public static string UntiedFrom(int partners) // Static: reads the game's current language
        {
            return Pick(partners == 1 ? UntiedFromOnes : UntiedFromManys);
        }

        private static string Pick(string[] translations) // Static: reads the game's current language
        {
            int language = (int)LocalizedText.CURRENT_LANGUAGE;
            return language >= 0 && language < translations.Length ? translations[language] : translations[0];
        }
    }
}
