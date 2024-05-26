using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftAddonNameFixer.Cli.Commands;

internal class FixMinecraftAddonSettings : CommandSettings
{
    [Description("mcaddon file to be fixed.")]
    [CommandArgument(0, "[filePath]")]
    public string FilePath { get; init; }

    [Description("the name of the file being cleaned.")]
    [CommandArgument(1, "[destPath]")]
    public string DestinationPath { get; init; }
}
