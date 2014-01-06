// Guids.cs
// MUST match guids.h
using System;

namespace DevlinLiles.VSConsole_PowerPack
{
    static class GuidList
    {
        public const string guidVSConsole_PowerPackPkgString = "c3497f74-316a-4d2c-a90c-695a9d5782d7";
        public const string guidVSConsole_PowerPackCmdSetString = "30c01dfe-b3df-4c6b-a5e0-9746573b8228";
        public const string guidToolWindowPersistanceString = "fbb4f1c7-9a8f-4832-bf0d-8741d38ba81b";

        public static readonly Guid guidVSConsole_PowerPackCmdSet = new Guid(guidVSConsole_PowerPackCmdSetString);
    };
}