namespace PersistencyManager.Patches._NetTool {
    using HarmonyLib;
    using KianCommons;
    using System;
    using static KianCommons.HelpersExtensions;
    using static KianCommons.Assertion;
    using PersistencyManager.Data;
    using MoveItIntegration;
    using PersistencyManager.API;

    [HarmonyPatch(typeof(global::NetTool), "MoveMiddleNode")]
    public static class MoveMiddleNodePatch {
        internal static MoveItIntegrationBase man_ => PersistencyManager.Instance.Manager;
        internal static SegmentRecord SegmentData { get; private set; }
        internal static bool CopyData => SegmentData != null;
        internal static ushort NodeID, NodeID2;


        /// <summary>
        /// scenario 1: no change - returns the input node.
        /// scenario 2: move node : segment is released and a smaller segment is created - returns the moved node.
        /// scenario 3: merge node: segment is released and the other node is returned.
        ///
        /// How to handle:
        /// 1: skip (DONE)
        /// 2: copy segment end for the node that didn't move (moved node cannot have customisations) (DONE)
        /// 3: when split-segment creates a new segment, that copy segment end to it.
        /// </summary>
        /// <param name="node">input/output node</param>
        public static void Prefix(ref ushort node) // TODO remove ref when in lates harmony.
        {
            if (!InSimulationThread()) return;
            NodeID = node;
            AssertEqual(NodeID.ToNode().CountSegments(), 1, "CountSegments");
            ushort segmentID = NetUtil.GetFirstSegment(NodeID);
            Log.Info($"MoveMiddleNode.Prefix() node:{NodeID} segment:{segmentID}"/*+"\n" + Environment.StackTrace*/, true);
            SegmentData = SegmentRecord.Create(segmentID, man_);
            NodeID2 = segmentID.ToSegment().GetOtherNode(NodeID);
        }

        /// <param name="node">output node</param>
        public static void Postfix(ref ushort node) {
            if (!InSimulationThread()) return;
            if (SegmentData == null) {
                Log.Debug($"MoveMiddleNode.Postfix()\n" + Environment.StackTrace,false);

                if (node == NodeID) {
                    // scenario 1: node did not move
                    // do nothing.
                } else if (node == NodeID2) {
                    // scenario 3: node merged
                    if (SplitSegmentPatch.SegmentData2 == null) {
                        SplitSegmentPatch.SegmentData2 = SegmentData;
                    } else {
                        SplitSegmentPatch.SegmentData3 = SegmentData;
                    }
                } else {
                    // scenario 2: node moved.
                }

            }
            SegmentData = null;
        }
    }
}
