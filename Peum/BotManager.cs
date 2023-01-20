using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using DSharpPlus.VoiceNext;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Peum
{
    public class BotManager
    {
        internal static EventId TestBotEventId { get; } = new EventId(1000, "NYZX");
        public DiscordClient Discord { get; }
        private SlashCommandsExtension SlashCommandService { get; }

        public BotManager()
        {

            DiscordConfiguration dcfg = new()
            {
                AutoReconnect = true,
                LargeThreshold = 250,
                MinimumLogLevel = LogLevel.Trace,
                Token = "Your Token Here",
                TokenType = TokenType.Bot,
                MessageCacheSize = 2048,
                LogTimestampFormat = "dd-MM-yyyy HH:mm:ss zzz",
                Intents = DiscordIntents.All
            };
            Discord = new DiscordClient(dcfg);

            Discord.Ready += Discord_Ready;
            Discord.GuildStickersUpdated += Discord_StickersUpdated;
            Discord.GuildAvailable += Discord_GuildAvailable;
            Discord.SocketErrored += Discord_SocketError;
            Discord.GuildCreated += Discord_GuildCreated;
            Discord.VoiceStateUpdated += Discord_VoiceStateUpdated;
            Discord.GuildDownloadCompleted += Discord_GuildDownloadCompleted;
            Discord.GuildUpdated += Discord_GuildUpdated;
            Discord.ChannelDeleted += Discord_ChannelDeleted;

            Discord.InteractionCreated += Discord_InteractionCreated;
            Discord.ComponentInteractionCreated += Discord_ModalCheck;
            Discord.ModalSubmitted += Discord_ModalSubmitted;
            Discord.ThreadCreated += Discord_ThreadCreated;
            Discord.ThreadUpdated += Discord_ThreadUpdated;
            Discord.ThreadDeleted += Discord_ThreadDeleted;
            Discord.ThreadListSynced += Discord_ThreadListSynced;
            Discord.ThreadMemberUpdated += Discord_ThreadMemberUpdated;
            Discord.ThreadMembersUpdated += Discord_ThreadMembersUpdated;

            VoiceNextConfiguration vcfg = new()
            {
                AudioFormat = AudioFormat.Default,
                EnableIncoming = true
            };

            ServiceCollection depco = new();

            InteractivityConfiguration icfg = new()
            {
                Timeout = TimeSpan.FromSeconds(10),
                AckPaginationButtons = true,
                ResponseBehavior = InteractionResponseBehavior.Respond,
                PaginationBehaviour = PaginationBehaviour.Ignore,
                ResponseMessage = "Sorry, but this wasn't a valid option, or does not belong to you!",
                PaginationButtons = new PaginationButtons()
                {
                    Stop = new DiscordButtonComponent(ButtonStyle.Danger, "stop", null, false, new DiscordComponentEmoji(862259725785497620)),
                    Left = new DiscordButtonComponent(ButtonStyle.Secondary, "left", null, false, new DiscordComponentEmoji(862259522478800916)),
                    Right = new DiscordButtonComponent(ButtonStyle.Secondary, "right", null, false, new DiscordComponentEmoji(862259691212242974)),
                    SkipLeft = new DiscordButtonComponent(ButtonStyle.Primary, "skipl", null, false, new DiscordComponentEmoji(862259605464023060)),
                    SkipRight = new DiscordButtonComponent(ButtonStyle.Primary, "skipr", null, false, new DiscordComponentEmoji(862259654403031050))
                }
            };

            SlashCommandService = Discord.UseSlashCommands();
            SlashCommandService.SlashCommandErrored += SlashCommandService_CommandErrored;
            SlashCommandService.SlashCommandInvoked += SlashCommandService_CommandReceived;
            SlashCommandService.SlashCommandExecuted += SlashCommandService_CommandExecuted;

            SlashCommandService.RegisterCommands<Chaopc>(551461216598622236);
        }


        private async Task Discord_ModalSubmitted(DiscordClient sender, ModalSubmitEventArgs e)
        {
            bool testWaitForModal = true;
            if (!testWaitForModal)
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder().WithContent("Thank you!"));
            }

            Discord.Logger.LogInformation("Got callback from user {User}, {Modal}", e.Interaction.User, e.Values);
        }

        private async Task Discord_ModalCheck(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            if (e.Id == "modal")
            {
                await e.Interaction.CreateResponseAsync(InteractionResponseType.Modal, new DiscordInteractionResponseBuilder()
                    .WithTitle("Test!")
                    .WithCustomId("owo")
                    .AddComponents(new TextInputComponent("Short, optional", "short_opt", "Placeholder!"))
                    .AddComponents(new TextInputComponent("Long, optional", "long_opt", "Placeholder 2!", style: TextInputStyle.Paragraph))
                    .AddComponents(new TextInputComponent("Short, required", "short_req", "Placeholder 3!", style: TextInputStyle.Short, min_length: 10, max_length: 20))
                    .AddComponents(new TextInputComponent("Long, required", "long_req", "Placeholder 4!", "Lorem Ipsum", true, TextInputStyle.Paragraph, 100, 300))
                );
            }
        }

        private async Task Discord_InteractionCreated(DiscordClient sender, InteractionCreateEventArgs e)
        {
            if (e.Interaction.Type != InteractionType.AutoComplete)
            {
                return;
            }

            Discord.Logger.LogInformation("AutoComplete: Focused: {Focused}, Data: {Data}", e.Interaction.Data.Options.First().Focused, e.Interaction.Data.Options.First().Value);

            DiscordInteractionDataOption option = e.Interaction.Data.Options.First();

            if (string.IsNullOrEmpty(option.Value as string))
            {
                return;
            }

            DiscordInteractionResponseBuilder builder = new DiscordInteractionResponseBuilder()
                .AddAutoCompleteChoice(new DiscordAutoCompleteChoice(option.Value as string, "pog ig"));

            await e.Interaction.CreateResponseAsync(InteractionResponseType.AutoCompleteResult, builder);

            return;
        }

        private Task Discord_StickersUpdated(DiscordClient sender, GuildStickersUpdateEventArgs e)
        {
            Discord.Logger.LogInformation("{GuildId}'s stickers updated: {StickerBeforeCount} -> {StickerAfterCount}", e.Guild.Id, e.StickersBefore.Count, e.StickersAfter.Count);
            return Task.CompletedTask;
        }

        public async Task RunAsync()
        {
            DiscordActivity act = new("Explotador x NYZX", ActivityType.ListeningTo);
            await Discord.ConnectAsync(act, UserStatus.DoNotDisturb).ConfigureAwait(false);
        }

        public async Task StopAsync()
        {
            await Discord.DisconnectAsync().ConfigureAwait(false);
        }

        private Task Discord_Ready(DiscordClient client, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

        private Task Discord_GuildAvailable(DiscordClient client, GuildCreateEventArgs e)
        {
            client.Logger.LogInformation(TestBotEventId, "Guild available: '{Guild}'", e.Guild.Name);
            return Task.CompletedTask;
        }

        private Task Discord_GuildCreated(DiscordClient client, GuildCreateEventArgs e)
        {
            client.Logger.LogInformation(TestBotEventId, "Guild created: '{Guild}'", e.Guild.Name);
            return Task.CompletedTask;
        }

        private Task Discord_SocketError(DiscordClient client, SocketErrorEventArgs e)
        {
            Exception? ex = e.Exception is AggregateException ae ? ae.InnerException : e.Exception;
            client.Logger.LogError(TestBotEventId, ex, "WebSocket threw an exception");
            return Task.CompletedTask;
        }

        private Task Discord_VoiceStateUpdated(DiscordClient client, VoiceStateUpdateEventArgs e)
        {
            client.Logger.LogDebug(TestBotEventId, "Voice state changed for '{User}' (mute: {MutedBefore} -> {MutedAfter}; deaf: {DeafBefore} -> {DeafAfter})", e.User, e.Before?.IsServerMuted, e.After.IsServerMuted, e.Before?.IsServerDeafened, e.After.IsServerDeafened);
            return Task.CompletedTask;
        }

        private Task Discord_GuildDownloadCompleted(DiscordClient client, GuildDownloadCompletedEventArgs e)
        {
            client.Logger.LogDebug(TestBotEventId, "Guild download completed");
            return Task.CompletedTask;
        }

        private async Task SlashCommandService_CommandErrored(SlashCommandsExtension sc, SlashCommandErrorEventArgs e)
        {
            e.Context.Client.Logger.LogError(TestBotEventId, e.Exception, "Exception occurred during {User}'s invocation of '{Command}'", e.Context.User.Username, e.Context.CommandName);

            DiscordEmoji emoji = DiscordEmoji.FromName(e.Context.Client, ":no_entry:");

            DiscordEmbedBuilder embed = new()
            {
                Title = "Error",
                Description = $"{emoji} Error!",
                Color = new DiscordColor(0xFF0000)
            };
            await e.Context.CreateResponseAsync(embed);
        }

        private Task SlashCommandService_CommandReceived(SlashCommandsExtension sc, SlashCommandInvokedEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(TestBotEventId, "User {User} tries to execute '{Command}' in {Channel}", e.Context.User.Username, e.Context.CommandName, e.Context.Channel.Name);
            return Task.CompletedTask;
        }

        private Task SlashCommandService_CommandExecuted(SlashCommandsExtension sc, SlashCommandExecutedEventArgs e)
        {
            e.Context.Client.Logger.LogInformation(TestBotEventId, "User {User} executed '{Command}' in {Channel}", e.Context.User.Username, e.Context.CommandName, e.Context.Channel.Name);
            return Task.CompletedTask;
        }

        private Task Discord_GuildUpdated(DiscordClient client, GuildUpdateEventArgs e)
        {
            StringBuilder str = new();

            _ = str.AppendLine($"The guild {e.GuildBefore.Name} has been updated.");

            foreach (System.Reflection.PropertyInfo prop in typeof(DiscordGuild).GetProperties())
            {
                try
                {
                    object? bfr = prop.GetValue(e.GuildBefore);
                    object? aft = prop.GetValue(e.GuildAfter);

                    if (bfr is null)
                    {
                        client.Logger.LogDebug(TestBotEventId, "Guild update: property {Property} in before was null", prop.Name);
                    }

                    if (aft is null)
                    {
                        client.Logger.LogDebug(TestBotEventId, "Guild update: property {Property} in after was null", prop.Name);
                    }

                    if (bfr is null || aft is null)
                    {
                        continue;
                    }

                    if (bfr.ToString() == aft.ToString())
                    {
                        continue;
                    }

                    _ = str.AppendLine($" - {prop.Name}: `{bfr}` to `{aft}`");
                }
                catch (Exception ex)
                {
                    client.Logger.LogError(TestBotEventId, ex, "Exception occurred during guild update");
                }
            }

            _ = str.AppendLine($" - VoiceRegion: `{e.GuildBefore.VoiceRegion?.Name}` to `{e.GuildAfter.VoiceRegion?.Name}`");

            Console.WriteLine(str);

            return Task.CompletedTask;
        }

        private async Task Discord_ChannelDeleted(DiscordClient client, ChannelDeleteEventArgs e)
        {
            IEnumerable<DiscordAuditLogChannelEntry> logs = (await e.Guild.GetAuditLogsAsync(5, null, AuditLogActionType.ChannelDelete).ConfigureAwait(false)).Cast<DiscordAuditLogChannelEntry>();
            foreach (DiscordAuditLogChannelEntry entry in logs)
            {
                Console.WriteLine("TargetId: " + entry.Target.Id);
            }
        }

        private Task Discord_ThreadCreated(DiscordClient client, ThreadCreateEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, "Thread created in {GuildName}. Thread Name: {ThreadName}", e.Guild.Name, e.Thread.Name);
            return Task.CompletedTask;
        }

        private Task Discord_ThreadUpdated(DiscordClient client, ThreadUpdateEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, "Thread updated in {GuildName}. New Thread Name: {ThreadName}", e.Guild.Name, e.ThreadAfter.Name);
            return Task.CompletedTask;
        }

        private Task Discord_ThreadDeleted(DiscordClient client, ThreadDeleteEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, "Thread deleted in {GuildName}. Thread Name: {ThreadName}", e.Guild.Name, e.Thread.Name ?? "Unknown");
            return Task.CompletedTask;
        }

        private Task Discord_ThreadListSynced(DiscordClient client, ThreadListSyncEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, "Threads synced in {GuildName}.", e.Guild.Name);
            return Task.CompletedTask;
        }

        private Task Discord_ThreadMemberUpdated(DiscordClient client, ThreadMemberUpdateEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, $"Thread member updated.");
            Console.WriteLine($"Discord_ThreadMemberUpdated fired for thread {e.ThreadMember.ThreadId}. User ID {e.ThreadMember.Id}.");
            return Task.CompletedTask;
        }

        private Task Discord_ThreadMembersUpdated(DiscordClient client, ThreadMembersUpdateEventArgs e)
        {
            client.Logger.LogDebug(eventId: TestBotEventId, "Thread members updated in {GuildName}.", e.Guild.Name);
            return Task.CompletedTask;
        }
    }
}