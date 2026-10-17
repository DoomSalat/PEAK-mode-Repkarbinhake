using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class ConfigEditorWindow : MonoBehaviour
    {
        private const int WindowId = 0x5245504B;
        private const float WindowWidth = 520f;
        private const float WindowHeight = 640f;
        private const float NameWidth = 210f;
        private const float HintHeight = 54f;
        private const string Title = "Repkarbinhake — настройки";
        private const string ApplyText = "Применить";
        private const string ResetText = "Сбросить";
        private const string CloseText = "Закрыть";
        private const string Note = "Толщина и цвет — для новых верёвок. Наведи на имя — подсказка.";
        private const string TrueText = "true";

        private readonly List<ConfigRow> _rows = new List<ConfigRow>();
        private ConfigFile _file;
        private Rect _window;
        private Vector2 _scroll;

        public bool IsOpen { get; private set; }

        private void OnGUI()
        {
            if (!IsOpen)
                return;

            _window = GUI.Window(WindowId, _window, DrawWindow, Title);
        }

        public void Initialize(ConfigFile file)
        {
            _file = file;
            _window = new Rect((Screen.width - WindowWidth) / 2f, (Screen.height - WindowHeight) / 2f, WindowWidth, WindowHeight);
        }

        public void Toggle()
        {
            if (IsOpen)
                Close();
            else
                Open();
        }

        public void Close()
        {
            IsOpen = false;
        }

        private void Open()
        {
            _rows.Clear();

            foreach (ConfigEntryBase entry in _file.Keys.OrderBy(definition => definition.Section).Select(definition => _file[definition]))
                _rows.Add(new ConfigRow(entry));

            IsOpen = true;
        }

        private void DrawWindow(int id)
        {
            _scroll = GUILayout.BeginScrollView(_scroll);
            string section = null;

            foreach (ConfigRow row in _rows)
            {
                if (row.Entry.Definition.Section != section)
                {
                    section = row.Entry.Definition.Section;
                    GUILayout.Label($"[{section}]");
                }

                DrawRow(row);
            }

            GUILayout.EndScrollView();
            GUILayout.Label(string.IsNullOrEmpty(GUI.tooltip) ? Note : GUI.tooltip, GUILayout.Height(HintHeight));
            GUILayout.BeginHorizontal();

            if (GUILayout.Button(ApplyText))
                Apply();

            if (GUILayout.Button(ResetText))
                Revert();

            if (GUILayout.Button(CloseText))
                Close();

            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }

        private void DrawRow(ConfigRow row)
        {
            ConfigEntryBase entry = row.Entry;
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(entry.Definition.Key, entry.Description.Description), GUILayout.Width(NameWidth));

            if (entry.SettingType == typeof(bool))
                row.Draft = GUILayout.Toggle(row.Draft == TrueText, string.Empty) ? TrueText : bool.FalseString.ToLowerInvariant();
            else if (entry.SettingType.IsEnum && GUILayout.Button(row.Draft))
                row.Draft = NextEnum(entry.SettingType, row.Draft);
            else if (!entry.SettingType.IsEnum)
                row.Draft = GUILayout.TextField(row.Draft);

            GUILayout.EndHorizontal();
        }

        private void Apply()
        {
            foreach (ConfigRow row in _rows)
            {
                if (row.Draft != row.Entry.GetSerializedValue())
                    row.Entry.SetSerializedValue(row.Draft);
            }

            _file.Save();
            Revert();
        }

        private void Revert()
        {
            foreach (ConfigRow row in _rows)
                row.Draft = row.Entry.GetSerializedValue();
        }

        private string NextEnum(Type type, string current)
        {
            string[] names = Enum.GetNames(type);
            int index = Array.IndexOf(names, current);
            return names[(index + 1) % names.Length];
        }

        private sealed class ConfigRow
        {
            public ConfigRow(ConfigEntryBase entry)
            {
                Entry = entry;
                Draft = entry.GetSerializedValue();
            }

            public ConfigEntryBase Entry { get; }
            public string Draft { get; set; } // Set: edited by the window until applied
        }
    }
}
