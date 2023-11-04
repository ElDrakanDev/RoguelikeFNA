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
        public string ActiveLanguage { get => _configManager.Config.Language; set => _configManager.Config.Language = value; }
        Dictionary<string, string> _translations = new Dictionary<string, string>();
        List<string> _headers = new List<string>();
        public List<string> AvailableLanguages => _headers.Except(new string[] { "id" }).ToList();

        static TranslationManager _instance;

        public override void OnEnabled()
        {
            _instance = this;
            _configManager = Core.GetGlobalManager<ConfigManager>();
            ReadTranslations();
        }

        void ReadTranslations()
        {
            _headers.Clear();

            using (FastExcel.FastExcel excel = new FastExcel.FastExcel(_translationsPath, true))
            {
                var worksheet = excel.Read(1);
                var rows = worksheet.Rows.ToArray();

                foreach(var cell in rows[0].Cells)
                    _headers.Add((string)cell.Value);

                int idIndex = _headers.IndexOf("id");
                int languageIndex = _headers.IndexOf(ActiveLanguage);

                foreach ( var row in rows.Skip(1))
                {
                    var cells = row.Cells.ToArray();
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
    }
}
