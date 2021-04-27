namespace PersistencyManager.Asset {
    using System;
    using KianCommons;
    using System.Collections.Generic;
    using PersistencyManager.API;
    using MoveItIntegration;

    [Serializable]
    internal class AssetData {
        private static MoveItIntegrationBase man_ => PersistencyManager.Instance.Manager;

        private string version_;
        /// <summary>Mod version at the time where data was serailized.</summary>
        internal Version Version {
            get => version_ != null ? new Version(version_) : default;
            set => version_ = value.ToString();
        }

        internal Record64[] Records;

        /// <summary>
        /// there is a 1:1 correspondence between returned array and BuildingInfo.m_paths.
        /// Does not store nodes that have no segments (they are at the end of BuildingInfo.m_paths so
        /// this does not interfere with array indeces).
        /// </summary>
        internal SegmentNetworkIDs[] PathNetworkIDs;

        public override string ToString() => $"AssetData(Version={Version} record={Records} ids={PathNetworkIDs})";

        /// <summary>
        /// gathers all data for the given asset.
        /// </summary>
        internal static AssetData GetAssetData(BuildingInfo prefab) {
            return new AssetData {
                Version = man_.DataVersion,
                Records = RecordAll(),
                PathNetworkIDs = GetPathsNetworkIDs(prefab),
            };
        }

        static InstanceID[] GetAllInstanceIDs() {
            var ret = new List<InstanceID>();
            for (ushort segmentId = 0; segmentId < NetManager.MAX_SEGMENT_COUNT; ++segmentId) {
                if (!NetUtil.IsSegmentValid(segmentId)) continue;
                var segment = new InstanceID { NetSegment = segmentId };
                var segmentStart = new InstanceID();
                segmentStart.SetSegmentEnd(segmentId, true);
                var segmentEnd = new InstanceID();
                segmentEnd.SetSegmentEnd(segmentId, false);
                ret.Add(segment);
                ret.Add(segmentStart);
                ret.Add(segmentEnd);
            }
            for (ushort nodeId = 0; nodeId < NetManager.MAX_NODE_COUNT; ++nodeId) {
                if (!NetUtil.IsNodeValid(nodeId)) continue;
                var node = new InstanceID { NetNode = nodeId };
                ret.Add(node);
            }
            return ret.ToArray();
        }

        internal static Record64[] RecordAll() {
            var instanceIDs = GetAllInstanceIDs();
            var records = new List<Record64>(instanceIDs.Length);
            for(int i=0;i<instanceIDs.Length; ++i) {
                Record64? record = Record64.Create(instanceIDs[i], man_);
                if(record != null)
                    records.Add((Record64)record);
            }
            return records.ToArray(); ;
        }

        /// <summary>
        /// creates an array of SegmentNetworkIDs.
        /// there is a 1:1 correspondence between returned array and BuildingInfo.m_paths.
        /// Does not store nodes that have no segments (they are at the end of BuildingInfo.m_paths so
        /// this does not interfere with array indeces).
        /// </summary>
        internal static SegmentNetworkIDs [] GetPathsNetworkIDs(BuildingInfo prefab) {
            // Code based on BuildingDecorations.SavePaths()
            Building[] buildingBuffer = BuildingManager.instance.m_buildings.m_buffer;
            List<ushort> assetSegmentIds = new List<ushort>();
            List<ushort> buildingIds = new List<ushort>(prefab.m_paths.Length);
            var ret = new List<SegmentNetworkIDs>();
            for (ushort buildingId = 1; buildingId < BuildingManager.MAX_BUILDING_COUNT; buildingId += 1) {
                if (buildingBuffer[buildingId].m_flags != Building.Flags.None) {
                    assetSegmentIds.AddRange(BuildingDecoration.GetBuildingSegments(ref buildingBuffer[buildingId]));
                    buildingIds.Add(buildingId);
                }
            }

            for (ushort segmentId = 0; segmentId < NetManager.MAX_SEGMENT_COUNT; segmentId++) {
                if (NetUtil.IsSegmentValid(segmentId)) {
                    if (!assetSegmentIds.Contains(segmentId)) {
                        ret.Add(new SegmentNetworkIDs(segmentId));
                    }
                }
            }

            return ret.ToArray();
        }
    }
}
