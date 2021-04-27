namespace PersistencyManager.Patches._NetManager {
    using HarmonyLib;
    using KianCommons;
    using static KianCommons.HelpersExtensions;
    using PersistencyManager.Patches._NetTool;
    using System;
    using PersistencyManager.Data;
    using MoveItIntegration;
    using PersistencyManager.API;
    using System.Collections.Generic;

    // TODO check compat with ParallelRoadTool
    [HarmonyPatch(typeof(global::NetManager), nameof(NetManager.CreateSegment))]
    public static class CreateSegmentPatch {
        internal static MoveItIntegrationBase man_ => PersistencyManager.Instance.Manager;

        // pastes segment ends that:
        // 1- not nullnot null and
        // 2- its nodeID matches input start/end nodeID.
        static void PasteSegment(
            SegmentRecord segmentData, ushort nodeID1, ushort nodeID2, ushort targetSegmentID) {
            if (segmentData == null) return;
            PasteSegmentEnd(segmentData.SegmenStart, segmentData.StartNodeID, nodeID1, nodeID2, targetSegmentID);
            PasteSegmentEnd(segmentData.SegmenEnd, segmentData.EndNodeID, nodeID1, nodeID2, targetSegmentID);
        }

        static void PasteSegmentEnd(
            object data, ushort nodeID1, ushort nodeID, ushort nodeID2, ushort targetSegmentID) {
            if (data != null) {
                if (nodeID == nodeID1 || nodeID == nodeID2) {
                    //man_.PasteSegmentEnd(data, targetNodeID: nodeID, targetSegmentID: targetSegmentID);
                    bool startNode = targetSegmentID.ToSegment().IsStartNode(nodeID);
                    var targetInstanceID = InstanceIDExtension.SegmentEnd(targetSegmentID, startNode);
                    man_.Paste(targetInstanceID, data, null);
                }
            }
        }

        static void Map(
            this Dictionary<InstanceID, InstanceID> map,
            SegmentRecord segmentData,
            ushort nodeID,
            ushort targetSegmentID) {
            if (segmentData.StartNodeID == nodeID) {
                map.Map(segmentData, nodeID, true, targetSegmentID);
            } else if (segmentData.EndNodeID == nodeID) {
                map.Map(segmentData, nodeID, false, targetSegmentID);
            }
        }

        static void Map(
            this Dictionary<InstanceID, InstanceID> map,
            SegmentRecord segmentData,
            ushort nodeID,
            bool startNode,
            ushort targetSegmentID) {
            // node maps to it self.
            InstanceID instanceID = new InstanceID { NetNode = nodeID };
            map[instanceID] = instanceID;

            // map segment.
            InstanceID sourceSegment = new InstanceID { NetSegment = segmentData.SegmentID };
            InstanceID targetSegment = new InstanceID { NetSegment = targetSegmentID };
            map[sourceSegment] = targetSegment;

            // map segment end.
            InstanceID sourceInstanceID = InstanceIDExtension.SegmentEnd(segmentData.SegmentID, startNode);
            bool targetStartNode = targetSegmentID.ToSegment().IsStartNode(nodeID);
            InstanceID targetInstanceID = InstanceIDExtension.SegmentEnd(targetSegmentID, targetStartNode);
            map[sourceInstanceID] = targetInstanceID;
        }

        public static void Postfix(ref ushort segment, ushort startNode, ushort endNode, bool __result) {
            if (!__result || !InSimulationThread()) return;
            Log.Debug($"CreateSegment.Postfix( {startNode}.-{segment}-.{endNode} )\n" + Environment.StackTrace, false);

            if (MoveMiddleNodePatch.CopyData) {
                var segmentData = MoveMiddleNodePatch.SegmentData;
                Log.Debug("Moving middle node: copying data to newly created segment. " +
                    $"newSegmentID={segment} data={segmentData}\n", false);
                PasteSegment(segmentData, startNode, endNode, targetSegmentID: segment);
            } else if (SplitSegmentPatch.CopyData) {
                var segmentData = SplitSegmentPatch.SegmentData;
                var segmentData2 = SplitSegmentPatch.SegmentData2;
                var segmentData3 = SplitSegmentPatch.SegmentData3;
                Log.Debug("Spliting segment: copying data to newly created segment. " +
                    $"newSegmentID={segment} data={segmentData} dat2={segmentData2} dat3={segmentData3}\n", false);

                var map = new Dictionary<InstanceID, InstanceID>();
                map.Map(segmentData, startNode, segment);
                map.Map(segmentData, endNode, segment);
                map.Map(segmentData2, startNode, segment);
                map.Map(segmentData2, endNode, segment);
                map.Map(segmentData3, startNode, segment);
                map.Map(segmentData3, endNode, segment);

                PasteSegment(segmentData, startNode, endNode, targetSegmentID: segment);
                PasteSegment(segmentData2, startNode, endNode, targetSegmentID: segment);
                PasteSegment(segmentData3, startNode, endNode, targetSegmentID: segment);
            } else if (ReleaseSegmentImplementationPatch.UpgradingSegmentData != null) {
                if (!ReleaseSegmentImplementationPatch.m_upgrading) {
                    Log.Error("Unexpected UpgradingSegmentData != null but m_upgrading == false ");
                } else {
                    var segmentData = ReleaseSegmentImplementationPatch.UpgradingSegmentData;
                    PasteSegment(segmentData, startNode, endNode, targetSegmentID: segment);
                }
                ReleaseSegmentImplementationPatch.UpgradingSegmentData = null; // consume
            }
        }
    }
}
