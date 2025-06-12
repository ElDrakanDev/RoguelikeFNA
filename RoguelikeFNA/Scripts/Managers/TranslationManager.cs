using Nez;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

namespace RoguelikeFNA
{
    public class TranslationManager : GlobalManager
    {
        ConfigManager _configManager;
        FileInfo _translationsPath = new FileInfo(ContentPath.Translations_xlsx);
        public static Action OnLanguageChanged;
        public string ActiveLanguage { get => _configManager.Config.Language; private set => _configManager.Config.Language = value; }
        Dictionary<string, string> _translations = new Dictionary<string, string>();
        List<string> _headers = new List<string>();
        public List<string> AvailableLanguages => _headers.Except(new string[] { "id" }).ToList();

        static TranslationManager _instance;

        public void ChangeLanguage(string to, bool reapplyTranslations = true)
        {
            if (_headers.Contains(to))
            {
                ActiveLanguage = to;
                _configManager.ApplyChanges();
                if (reapplyTranslations)
                    ReadTranslations();
            }
            else
            {
                Debug.Warn("Trying to change to language {0} but it doesnt exist", to);
            }
        }

        public override void OnEnabled()
        {
            _instance = this;
            _configManager = Core.GetGlobalManager<ConfigManager>();
            ReadTranslations();
        }

        void ReadTranslations()
        {
            _headers.Clear();
            _translations.Clear();

            using (FastExcel.FastExcel excel = new FastExcel.FastExcel(_translationsPath, true))
            {
                var worksheet = excel.Read(1);
                var rows = worksheet.Rows.ToArray();

                foreach (var cell in rows[0].Cells)
                    _headers.Add((string)cell.Value);

                int idIndex = _headers.IndexOf("id");
                if (!_headers.Contains(ActiveLanguage))
                {
                    // Language doesn't exist in sheet, return to default_language
                    ChangeLanguage(ConfigManager.DEFAULT_LANG, false);
                }

                int languageIndex = _headers.IndexOf(ActiveLanguage);

                foreach (var row in rows.Skip(1))
                {
                    var cells = row.Cells.ToArray();
                    // If row doesnt have at least ID and translation it is an empty / invalid row
                    if (cells.Length < 2) continue;
                    _translations.Add((string)cells[idIndex].Value, (string)cells[languageIndex].Value);
                }
            }
        }

        public static string GetTranslation(string textId)
        {
            if (_instance._translations.TryGetValue(textId, out string value))
                return value;
            throw new Exception($"Dialogue unset for ID {textId}");
        }

        public static string TryGetTranslation(string textId)
        {
            if (_instance._translations.TryGetValue(textId, out string value))
                return value;
            return string.Empty;
        }
    }
}
