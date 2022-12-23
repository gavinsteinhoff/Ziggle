using Discord;
using Discord.Interactions;

namespace Ziggle.Bot;

public abstract class InteractionModuleHelper<T> : InteractionModuleBase<T> where T : class, IInteractionContext
{
    public virtual Task RespondDefaultAsync()
    {
        return RespondAsync("There you go bruh.", ephemeral: true);
    }

    public virtual Task RespondDefaultErrorAsync(string message)
    {
        return RespondAsync($"Bruh, {message}.", ephemeral: true);
    }
}
