namespace PersistencyManager.Util {
    using System.Collections.Generic;
    using System.Linq;
    using KianCommons;
    using PersistencyManager.Asset;
    using MoveItIntegration;
    using PersistencyManager.API;

    public static class PlaceIntersectionUtil {
        static MoveItIntegrationBase man_ => PersistencyManager.Instance.Manager;
        static Dictionary<BuildingInfo, AssetData> Asset2Data => PersistencyManager.Instance.Asset2Data;


        ///<summary>maps old netowkr ids to new network ids</summary>
        /// <param name="newSegmentIds">segment list provided by LoadPaths.</param>
        public static void MapSegments(
            SegmentNetworkIDs[] oldSegments,
            ushort[] newSegmentIds,
            Dictionary<InstanceID, InstanceID> map) {
            Assertion.Assert(oldSegments.Length == newSegmentIds.Length);
            for (int i = 0; i < newSegmentIds.Length; ++i) {
                // load paths load segments in the same order as they were stored.
                oldSegments[i].MapInstanceIDs(newSegmentId: newSegmentIds[i], map: map);
            }
        }

        public delegate void Handler(BuildingInfo info, Dictionary<InstanceID, InstanceID> map);

        /// <summary>
        /// invoked before copying data to networks.
        /// </summary>
        public static event Handler OnIntersectionPlacing;

        /// <summary>
        /// invoked after copying data to networks.
        /// </summary>
        public static event Handler OnIntersectionPlaced;

        /// <summary>
        /// start mapping for <paramref name="intersectionInfo"/>
        /// </summary>
        /// <param name="newSegmentIds">segment list provided by LoadPaths.</param>
        public static void ApplyData(BuildingInfo intersectionInfo, ushort[] newSegmentIds) {
            /*************************
             * Prepration: */

            Log.Debug($"PlaceIntersectionUtil.ApplyData({intersectionInfo?.ToString() ?? "null"})");

            if (!Helpers.InSimulationThread()) {
                Log.Error("must be called from simulation thread");
                return;
            }
            if (intersectionInfo == null) {
                Log.Error("intersectionInfo is null");
                return;
            }

            var map = new Dictionary<InstanceID, InstanceID>();

            Log.Debug("PlaceIntersectionUtil.ApplyData(): Asset2Data.keys=" +
                Asset2Data.Select(item => item.Key).ToSTR());

            if (Asset2Data.TryGetValue(intersectionInfo, out var assetData)) {
                Log.Info("PlaceIntersectionUtil.ApplyData(): assetData =" + assetData);
            } else {
                Log.Info("PlaceIntersectionUtil.ApplyData(): assetData not found (the asset does not have TMPE data)");
                return;
            }

            var pathNetworkIDs = assetData.PathNetworkIDs;
            if (pathNetworkIDs == null) return;

            /*************************
             * Apply traffic rules: */

            MapSegments(oldSegments: pathNetworkIDs, newSegmentIds: newSegmentIds, map: map);

            foreach (var item in map)
                CalculateNetwork(item.Value);

            OnIntersectionPlacing?.Invoke(intersectionInfo, map);

            var version = assetData.Version;
            foreach (var record in assetData.Records) {
                record.Paste(man_, version, map);
            }

            OnIntersectionPlaced?.Invoke(intersectionInfo, map);
        }

        /// <summary>
        /// early calculate networks so that we are able to set traffic rules without delay.
        /// </summary>
        public static void CalculateNetwork(InstanceID instanceId) {
            switch (instanceId.Type) {
                case InstanceType.NetNode:
                    ushort nodeId = instanceId.NetNode;
                    nodeId.ToNode().CalculateNode(nodeId);
                    break;
                case InstanceType.NetSegment:
                    ushort segmentId = instanceId.NetSegment;
                    segmentId.ToSegment().CalculateSegment(segmentId);
                    break;
            }
        }

    }
}
