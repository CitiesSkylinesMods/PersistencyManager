namespace PersistencyManager.API {
    using MoveItIntegration;
    using global::PersistencyManager.Asset;
    using System.Collections.Generic;

    public class PersistencyManager {
        internal static PersistencyManager Instance { get; private set; }
        internal string ID;

        internal readonly MoveItIntegrationBase Manager;

        internal Dictionary<BuildingInfo, AssetData> Asset2Data = new Dictionary<BuildingInfo, AssetData>();

        private PersistencyManager(MoveItIntegrationBase manager, string id) {
            Manager = manager;
            ID = "PersistencyManager_" + id;
        }

        /// <summary>
        /// Starts persistency manager if it is not started already
        /// </summary>
        public static PersistencyManager Start(MoveItIntegrationBase manager, string id) =>
            Instance ??= new PersistencyManager(manager, id);

        /// <summary>
        /// stops persistency manager and releases its memory.
        /// </summary>
        public void End() {
            Instance = null;
        }
    }
}