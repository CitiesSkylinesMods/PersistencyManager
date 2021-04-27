namespace PersistencyManager.Patches._NetManager
{
    using System;
    using System.Reflection;
    using HarmonyLib;
    using ColossalFramework;
    using KianCommons;
    using static KianCommons.ReflectionHelpers;
    using PersistencyManager.Data;
    using MoveItIntegration;
    using PersistencyManager.API;

    [HarmonyPatch]
    public static class ReleaseSegmentImplementationPatch
    {
        internal static MoveItIntegrationBase man_ => PersistencyManager.Instance.Manager;

        //private void ReleaseSegmentImplementation(ushort segment, ref NetSegment data, bool keepNodes)
        public static MethodBase TargetMethod()
        {
            return AccessTools.DeclaredMethod(
                typeof(NetManager),
                "ReleaseSegmentImplementation",
                new[] {typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(bool) },
                null);
        }

        internal static SegmentRecord UpgradingSegmentData;
        public static bool m_upgrading =>
            (bool)GetFieldValue(Singleton<NetTool>.instance, "m_upgrading");

        public static void Prefix(ushort segment)
        {
            if (UpgradingSegmentData != null) {
                KianCommons.Log.Error("Unexpected UpgradingSegmentData != null");
                UpgradingSegmentData = null;
            }
            if (m_upgrading) {
                UpgradingSegmentData = SegmentRecord.Create(segment, man_);
            }
            Log.Debug($"ReleaseSegment.Prefix({segment})\n"+Environment.StackTrace);
            if(man_ is IReleaser releaser) {
                releaser.Release(new InstanceID { NetSegment = segment });
                releaser.Release(InstanceIDExtension.SegmentEnd(segment, true));
                releaser.Release(InstanceIDExtension.SegmentEnd(segment, false));
            }

        }
    }
}