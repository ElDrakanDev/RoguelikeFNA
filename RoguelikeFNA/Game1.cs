using Nez;
using Nez.ImGuiTools;
using RoguelikeFNA.Items;

namespace RoguelikeFNA
{
    class Game1 : Core
    {
        public Game1() : base()
        {
			// uncomment this line for scaled pixel art games
			// System.Environment.SetEnvironmentVariable("FNA_OPENGL_BACKBUFFER_SCALE_NEAREST", "1");
        }

        void RegisterAllGlobalManagers()
        {
            Core.RegisterGlobalManager(new ConfigManager());
            Core.RegisterGlobalManager(new TranslationManager());
            Core.RegisterGlobalManager(new SoundEffectManager());
            Core.RegisterGlobalManager(new InputManager());
            Core.RegisterGlobalManager(new RNGManager());
            Core.RegisterGlobalManager(new ItemRepository());
#if DEBUG
            DebugRenderEnabled = true;
            System.Diagnostics.Debug.Listeners.Add(new System.Diagnostics.TextWriterTraceListener(System.Console.Out));

            // optionally render Nez in an ImGui window
			var imGuiManager = new ImGuiManager();
			Core.RegisterGlobalManager(imGuiManager);

			// optionally load up ImGui DLL if not using the above setup so that its command gets loaded in the DebugConsole
			//System.Reflection.Assembly.Load("Nez.ImGui")
#endif
        }

        override protected void Initialize()
        {
            base.Initialize();
            RegisterAllGlobalManagers();
            Scene = new MainMenuScene();
        }
    }
}

