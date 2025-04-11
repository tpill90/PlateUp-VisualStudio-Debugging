namespace AutoRestaurantLoader
{
    //TODO document what all of this does, maybe put it in my readme under development
    [UsedImplicitly]
    [UpdateAfter(typeof(CreateLocationsRoom))]
    public class AutoLoadRestaurant : FranchiseFirstFrameSystem, IModSystem, IModInitializer
    {
        // Preference manager must be static otherwise it will be null after the mod is initially activated.
        private static PreferenceSystemManager _prefManager;
        private const string SaveSlotKey = "SaveSlotKey";

        // This is run first, after the mod has been loaded from disk
        public void PostActivate(Mod mod)
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            _prefManager = new PreferenceSystemManager("AutoRestaurantLoader", "Auto Restaurant Loader");
            _prefManager.AddLabel("Save slot to load")
                       .AddOption<int>(SaveSlotKey, initialValue: 0, values: new int[] { 0, 1, 2, 3, 4, 5 }, new string[] { "Disabled", "1", "2", "3", "4", "5" })
                       .AddSpacer()
                       .AddSpacer();
            _prefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
            _prefManager.RegisterMenu(PreferenceSystemManager.MenuType.MainMenu);

            LogInfo(" loaded !");
        }

        protected override void OnUpdate()
        {
            var saveSlot = _prefManager.Get<int>(SaveSlotKey);
            if (saveSlot == 0)
            {
                LogInfo("Auto Load disabled.  No restaurant will be loaded.");
                return;
            }

            EntityQuery query = GetEntityQuery(new QueryHelper().All(typeof(CLocationChoice)));
            using var entities = query.ToEntityArray(Allocator.TempJob);
            foreach (var entity in entities)
            {
                if (!Require(entity, out CLocationChoice location))
                {
                    continue;
                }

                if (location.Slot != saveSlot || location.State != SaveState.Loaded)
                {
                    continue;
                }

                Set(new SSelectedLocation
                {
                    Valid = true,
                    Selected = location
                });

                Entity e = EntityManager.CreateEntity(typeof(SPerformSceneTransition), typeof(CDoNotPersist));
                EntityManager.SetComponentData(e, new SPerformSceneTransition
                {
                    NextScene = SceneType.LoadFullAutosave
                });
            }
        }

        #region Logging

        public static string MOD_NAME = "AutoLoadRestaurant";
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        #endregion

        // These are empty on purpose
        public void PreInject() { }
        public void PostInject() { }
    }

    [HarmonyPatch]
    public static class Patches
    {
        public static void LogInfo(string _log) { Debug.Log($"[{"AutoLoadRestaurant"}] " + _log); }

        public static int playerId = 0;
        public static bool ranOnce = false;

        // TODO this might need to be moved into a bepinex plugin
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuView), nameof(MainMenuView.BeginSplash))]
        public static bool SkipSplashScreen(MainMenuView __instance)
        {
            LogInfo("BeginSplash");
            __instance.SplashScreen.SetActive(false);
            return false;
        }

        // This will load us into the world on the first keypress we make
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainMenuView), nameof(MainMenuView.TakeInput))]
        public static void MainMenuView_TakeInput(int player_id, InputState state)
        {
            //TODO comment + rename
            if (ranOnce)
            {
                return;
            }
            playerId = player_id;
            LogInfo($"MainMenuView - TakeInput - PlayerId {player_id}");

            PlayerProfile playerProfile = ProfileAccessor.EnsureProfile(InputSourceIdentifier.Default.GetPlatformUser(playerId));
            Session.RetainedPlayers.Clear();
            Session.RetainedPlayers.Add(new RetainedPlayer()
            {
                PlayerProfile = playerProfile.Identifier,
                InputPlayerID = playerId
            });
            LogInfo($"SetupMenus - PlayerProfile {playerProfile.Identifier._Value}  InputPlayerId {playerId}");

            NetworkHelpers.CurrentNetworkPermissions = NetworkPermissions.Private;
            Session.StartLocalGame(false);
            ranOnce = true;
        }
    }
}