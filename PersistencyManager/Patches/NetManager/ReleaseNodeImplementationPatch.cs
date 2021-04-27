using System;
using System.Reflection;
using HarmonyLib;
using ColossalFramework;
using PersistencyManager.Data;
using MoveItIntegration;
using PersistencyManager.API;

namespace PersistencyManager.Patches._NetManager
{
    [HarmonyPatch]
    public static class ReleaseNodeImplementationPatch
    {
        internal static MoveItIntegrationBase man_ => PersistencyManager.Instance.Manager;

        public static MethodBase TargetMethod()
        {
            // ReleaseNodeImplementation(ushort node, ref NetNode data)
            return typeof(global::NetManager).GetMethod(
                "ReleaseNodeImplementation",
                BindingFlags.NonPublic | BindingFlags.Instance,
                Type.DefaultBinder,
                new[] {
                typeof(ushort), typeof(global::NetNode).MakeByRefType(),
                }, null);
        }

        public static void Prefix(ushort node)
        {
            if (man_ is IReleaser releaser) {
                releaser.Release(new InstanceID { NetNode = node });
        }
    }
}