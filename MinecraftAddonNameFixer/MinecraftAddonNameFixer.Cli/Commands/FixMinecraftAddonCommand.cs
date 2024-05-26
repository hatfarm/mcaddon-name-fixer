using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecraftAddonNameFixer.Cli.Commands;

internal class FixMinecraftAddonCommand : AsyncCommand<FixMinecraftAddonSettings>
{
    public async override Task<int> ExecuteAsync(CommandContext context, FixMinecraftAddonSettings settings)
    {
        Stream cleanedContents = MinecraftFilenameFixer.CleanFile(new FileStream(settings.FilePath, FileMode.Open));

        // Ensure the source stream is at the start
        cleanedContents.Position = 0;

        // Create a new FileStream for the destination path
        using FileStream destinationStream = new FileStream(settings.DestinationPath, FileMode.Create);

        // Use CopyToAsync to copy the stream data to the file
        await cleanedContents.CopyToAsync(destinationStream);

        return 0;
    }
}
