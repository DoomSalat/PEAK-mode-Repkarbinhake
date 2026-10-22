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

        private static readonly string[] SettingsTitles = { "settings", "paramètres", "impostazioni", "Einstellungen", "ajustes", "ajustes", "configurações", "настройки", "налаштування", "设置", "設定", "設定", "설정", "ustawienia", "ayarlar" }; // Static: fixed translation table
        private static readonly string[] Applies = { "Apply", "Appliquer", "Applica", "Übernehmen", "Aplicar", "Aplicar", "Aplicar", "Применить", "Застосувати", "应用", "套用", "適用", "적용", "Zastosuj", "Uygula" }; // Static: fixed translation table
        private static readonly string[] Resets = { "Reset", "Réinitialiser", "Ripristina", "Zurücksetzen", "Restablecer", "Restablecer", "Redefinir", "Сбросить", "Скинути", "重置", "重設", "リセット", "초기화", "Resetuj", "Sıfırla" }; // Static: fixed translation table
        private static readonly string[] Closes = { "Close", "Fermer", "Chiudi", "Schließen", "Cerrar", "Cerrar", "Fechar", "Закрыть", "Закрити", "关闭", "關閉", "閉じる", "닫기", "Zamknij", "Kapat" }; // Static: fixed translation table
        private static readonly string[] SettingsNotes = { "Thickness and color apply to new ropes. \"{0}\": your game computes these forces for your character. Hover a name for a hint.", "L'épaisseur et la couleur s'appliquent aux nouvelles cordes. « {0} » : ton jeu calcule ces forces pour ton personnage. Survole un nom pour une aide.", "Spessore e colore valgono per le nuove corde. «{0}»: il tuo gioco calcola queste forze per il tuo personaggio. Passa sul nome per un suggerimento.", "Dicke und Farbe gelten für neue Seile. „{0}“: dein Spiel berechnet diese Kräfte für deine Figur. Fahre über einen Namen für einen Hinweis.", "El grosor y el color se aplican a cuerdas nuevas. «{0}»: tu juego calcula estas fuerzas para tu personaje. Pasa el cursor por un nombre para ver una pista.", "El grosor y el color se aplican a cuerdas nuevas. «{0}»: tu juego calcula estas fuerzas para tu personaje. Pasa el cursor sobre un nombre para ver una pista.", "Espessura e cor valem para cordas novas. \"{0}\": seu jogo calcula essas forças para o seu personagem. Passe o mouse sobre um nome para ver uma dica.", "Толщина и цвет — для новых верёвок. «{0}» — силы считает твоя игра для твоего персонажа. Наведи на имя — подсказка.", "Товщина й колір — для нових мотузок. «{0}» — сили рахує твоя гра для твого персонажа. Наведи на назву — підказка.", "粗细和颜色只对新绳子生效。“{0}”：这些力由你的游戏为你的角色计算。悬停在名称上查看提示。", "粗細和顏色只對新繩子生效。「{0}」：這些力由你的遊戲為你的角色計算。滑鼠移到名稱上查看提示。", "太さと色は新しいロープに適用されます。「{0}」：これらの力はあなたのゲームが自分のキャラクター用に計算します。名前にカーソルを合わせるとヒントが出ます。", "두께와 색은 새 밧줄에 적용됩니다. \"{0}\": 이 힘은 당신의 게임이 당신 캐릭터에 대해 계산합니다. 이름에 마우스를 올리면 도움말이 나옵니다.", "Grubość i kolor dotyczą nowych lin. „{0}”: te siły liczy twoja gra dla twojej postaci. Najedź na nazwę, aby zobaczyć podpowiedź.", "Kalınlık ve renk yeni halatlara uygulanır. \"{0}\": bu kuvvetleri senin oyunun kendi karakterin için hesaplar. İpucu için bir adın üzerine gel." }; // Static: fixed translation table
        private static readonly string[] ScopeOnYous = { "on you", "sur toi", "su di te", "auf dich", "sobre ti", "sobre ti", "em você", "на тебя", "на тебе", "作用于你", "作用於你", "自分に", "나에게", "na ciebie", "sana" }; // Static: fixed translation table
        private static readonly string[] ScopeWhenPullings = { "when pulling", "quand tu tires", "quando tiri", "beim Ziehen", "al tirar", "al jalar", "ao puxar", "когда тянешь", "коли тягнеш", "拉绳时", "拉繩時", "引くとき", "당길 때", "gdy ciągniesz", "çekerken" }; // Static: fixed translation table
        private static readonly string[] ScopeOnYouWhenPullings = { "on you, when pulling", "sur toi, quand tu tires", "su di te, quando tiri", "auf dich, beim Ziehen", "sobre ti, al tirar", "sobre ti, al jalar", "em você, ao puxar", "на тебя и когда тянешь", "на тебе й коли тягнеш", "作用于你和拉绳时", "作用於你和拉繩時", "自分に・引くとき", "나에게, 당길 때", "na ciebie i gdy ciągniesz", "sana ve çekerken" }; // Static: fixed translation table
        private static readonly string[] ScopeHostToAlls = { "host → all", "hôte → tous", "host → tutti", "Host → alle", "anfitrión → todos", "anfitrión → todos", "anfitrião → todos", "хост → всем", "хост → усім", "房主 → 所有人", "房主 → 所有人", "ホスト → 全員", "호스트 → 모두", "host → wszyscy", "ev sahibi → herkes" }; // Static: fixed translation table
        private static readonly string[] ScopeLocalOnlys = { "only you", "seulement toi", "solo tu", "nur du", "solo tú", "solo tú", "só você", "только у тебя", "лише в тебе", "仅你", "僅你", "自分のみ", "나만", "tylko ty", "sadece sen" }; // Static: fixed translation table

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
        public static string SettingsTitle => Pick(SettingsTitles);
        public static string Apply => Pick(Applies);
        public static string Reset => Pick(Resets);
        public static string Close => Pick(Closes);
        public static string SettingsNote => string.Format(Pick(SettingsNotes), ScopeName(SettingScope.OnYou));

        public static string ScopeName(SettingScope scope) // Static: reads the game's current language
        {
            switch (scope)
            {
                case SettingScope.OnYou:
                    return Pick(ScopeOnYous);

                case SettingScope.WhenPulling:
                    return Pick(ScopeWhenPullings);

                case SettingScope.OnYouWhenPulling:
                    return Pick(ScopeOnYouWhenPullings);

                case SettingScope.HostToAll:
                    return Pick(ScopeHostToAlls);

                default:
                    return Pick(ScopeLocalOnlys);
            }
        }

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
