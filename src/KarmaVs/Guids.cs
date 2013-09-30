// Guids.cs
// MUST match guids.h

using System;

namespace devcoach.Tools
{
    static class GuidList
    {
        public const string guidKarmaVsPkgString = "7ca5e40e-4946-4da8-be1f-b3bab3e8adfc";
        public const string guidToggleKarmaVsUnitCmdSetString = "a91817b7-83bc-4d3b-bbac-67c87be2b5b5";
        public const string guidToggleKarmaVsE2eCmdSetString = "a91817b7-83bc-4d3b-bbac-67c87be2b5b6";

        public static readonly Guid guidKarmaVsPkg = new Guid(guidKarmaVsPkgString);
        public static readonly Guid guidToggleKarmaVsUnitCmdSet = new Guid(guidToggleKarmaVsUnitCmdSetString);
        public static readonly Guid guidToggleKarmaVsE2eCmdSet = new Guid(guidToggleKarmaVsE2eCmdSetString);
    };
}