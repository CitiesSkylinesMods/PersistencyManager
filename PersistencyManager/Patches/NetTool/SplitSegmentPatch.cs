namespace PersistencyManager.Patches._NetTool {
    using HarmonyLib;
    using static KianCommons.Helpers;
    using KianCommons;
    using System;
    using MoveItIntegration;
    using PersistencyManager.API;
    using PersistencyManager.Data;

    [HarmonyPatch(typeof(global::NetTool), "SplitSegment")]
    public class SplitSegmentPatch
    {
        static MoveItIntegrationBase man_ => PersistencyManager.Instance.Manager;
        internal static SegmentRecord? SegmentData3 { get; set; } // by move middle node
        internal static SegmentRecord? SegmentData2 { get; set; } // by move middle node
        internal static SegmentRecord? SegmentData { get; private set; }
        internal static bool CopyData => SegmentData != null || SegmentData2 !=null || SegmentData3 != null;

        public static void Prefix(ushort segment)
        {
            if (!InSimulationThread()) return;
            Log.Info($"SplitSegment.Prefix() segment:{segment}"/*\n" + Environment.StackTrace*/, true);
            SegmentData = SegmentRecord.Create(segment , man_);
        }

        public static void Postfix()
        {
            if (!InSimulationThread()) return;
            Log.Debug($"SplitSegment.Postfix()\n" + Environment.StackTrace, false);
            SegmentData = SegmentData2 = SegmentData3 = null;
        }
    }
}
