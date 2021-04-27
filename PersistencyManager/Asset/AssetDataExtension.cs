namespace PersistencyManager.Asset {
    using ICities;
    using System.Collections.Generic;
    using KianCommons;
    using static KianCommons.Assertion;
    using KianCommons.Serialization;
    using System;
    using PersistencyManager.API;

    public class AssetDataExtension : AssetDataExtensionBase {
        internal static string ID => PersistencyManager.Instance.ID;

        static Dictionary<BuildingInfo, AssetData> asset2Data_ => PersistencyManager.Instance.Asset2Data;

        public override void OnAssetLoaded(string name, object asset, Dictionary<string, byte[]> userData) =>
            OnAssetLoadedImpl(name, asset, userData);

        public override void OnAssetSaved(string name, object asset, out Dictionary<string, byte[]> userData) =>
            OnAssetSavedImpl(name, asset, out userData);

        internal static void OnAssetLoadedImpl(string name, object asset, Dictionary<string, byte[]> userData) {
            Log.Info($"AssetDataExtension.OnAssetLoaded({name}, {asset}, userData) called");
            if (asset is BuildingInfo prefab) {
                Log.Debug("AssetDataExtension.OnAssetLoaded():  prefab is " + prefab);
                if (userData.TryGetValue(ID, out byte[] data)) {
                    Log.Info("AssetDataExtension.OnAssetLoaded(): extracted data for " + ID);
                    string data64 = Convert.ToBase64String(data);
                    var assetData = XMLSerializerUtil.Deserialize<AssetData>(data64);
                    AssertNotNull(assetData, "assetData");
                    asset2Data_[prefab] = assetData;
                    Log.Debug("AssetDataExtension.OnAssetLoaded(): Asset Data=" + assetData);
                }
            }
        }

        // asset should be the same as ToolsModifierControl.toolController.m_editPrefabInfo
        internal static void OnAssetSavedImpl(string name, object asset, out Dictionary<string, byte[]> userData) {
            Log.Info($"AssetDataExtension.OnAssetSaved({name}, {asset}, userData) called");
            userData = null;
            if (asset is BuildingInfo prefab) {
                Log.Info("AssetDataExtension.OnAssetSaved():  prefab is " + prefab);
                var assetData = AssetData.GetAssetData(prefab);
                Log.Debug("AssetDataExtension.OnAssetSaved(): assetData=" + assetData);

                string data64 = XMLSerializerUtil.Serialize(assetData);
                byte[] data = Convert.FromBase64String(data64);
                userData = new Dictionary<string, byte[]>();
                userData[ID] = data;
            }
        }
    }
}
