using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Peum
{
    public class Chaopc : ApplicationCommandModule
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(
        IntPtr hWnd,
        uint Msg,
        IntPtr wParam,
        IntPtr lParam
        );

        public static void PowerOff()
        {
            _ = SendMessage(
               (IntPtr)0xffff,
               0x0112,
               (IntPtr)0xf170,
               (IntPtr)0x0002
            );
        }

        [SlashCommand("ping", "Checks my ping.")]
        public async Task Ping(InteractionContext ctx)
        {
            DiscordMessage message;
            try
            {
                message = await ctx.Channel.SendMessageAsync("Pong! This is a temporary message used to check ping and should be deleted shortly.");
            }
            catch
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().WithContent($"Pong! Client ping is `{ctx.Client.Ping}ms`.\n\nI tried to send a message to check round-trip ping, but I don't have permission to send messages in this channel! Try again in another channel where I have permission to send messages."));
                return;
            }

            ulong msSinceEpoch = message.Id >> 22;
            ulong messageTimestamp = msSinceEpoch + 1420070400000;
            DateTimeOffset messageTimestampOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)messageTimestamp);
            DateTime messageTimestampDateTime = messageTimestampOffset.UtcDateTime;


            string responseTime = (messageTimestampDateTime - ctx.Interaction.CreationTimestamp.UtcDateTime).ToString()
                .Replace("0", "")
                .Replace(":", "")
                .Replace(".", "");

            await message.DeleteAsync();

            await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().WithContent($"Pong! Client ping is `{ctx.Client.Ping}ms`.\n\nIt took me `{responseTime}ms` to send a message after you used this command."));
        }

        [SlashCommand("cancel", "Cancelar Apagado de la PC.")]
        public async Task cancelshutdown(InteractionContext ctx)
        {
            if (!SharedData.OwnerID.Contains(ctx.Member.Id) && !ctx.Client.CurrentApplication.Owners.Any(x => x.Id == ctx.User.Id))
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().WithContent("You do not have permission to use this command!"));
                return;
            }
            else
            {
                try
                {
                    ProcessStartInfo commandPromptSettings = new()
                    {
                        FileName = @"C:\Windows\System32\cmd.exe",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    Process commandPrompt = new()
                    {
                        StartInfo = commandPromptSettings,
                        EnableRaisingEvents = true
                    };
                    _ = commandPrompt.Start();
                    commandPrompt.BeginOutputReadLine();
                    commandPrompt.BeginErrorReadLine();
                    commandPrompt.StandardInput.WriteLine("shutdown -a");
                    commandPrompt.StandardInput.WriteLine("exit");
                    await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().WithContent("Shutdown cancelled"));
                }
                catch (Exception) { }
            }
        }

        [SlashCommand("shutdown", "Apagar PC.")]
        public async Task Shutdown(InteractionContext ctx, [Option("minutos", "En cuantos minutos apagar la PC")] string? tr = null)
        {
            if (!SharedData.OwnerID.Contains(ctx.Member.Id) && !ctx.Client.CurrentApplication.Owners.Any(x => x.Id == ctx.User.Id))
            {
                await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().WithContent("You do not have permission to use this command!"));
                return;
            }
            try
            {
                DSharpPlus.Interactivity.InteractivityExtension interactivity = ctx.Client.GetInteractivity();
                if (tr == null)
                {
                    ProcessStartInfo psi = new("shutdown", "/s /t 0")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    PowerOff();
                    _ = Process.Start(psi);
                }
                else
                {
                    int time = Convert.ToInt32(tr.Replace("h", "").Replace(" ", "").Replace("H", ""));
                    int minutesInt = int.Parse("0");
                    int hoursInt = int.Parse(time.ToString());
                    int daysInt = int.Parse("0");

                    if (minutesInt > 0 || hoursInt > 0 || daysInt > 0)
                    {
                        ProcessStartInfo commandPromptSettings = new()
                        {
                            FileName = @"C:\Windows\System32\cmd.exe",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            RedirectStandardInput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            WindowStyle = ProcessWindowStyle.Hidden
                        };
                        Process commandPrompt = new()
                        {
                            StartInfo = commandPromptSettings,
                            EnableRaisingEvents = true
                        };
                        _ = commandPrompt.Start();
                        commandPrompt.BeginOutputReadLine();
                        commandPrompt.BeginErrorReadLine();
                        int milliseconds = (daysInt * 86400) + (hoursInt * 3600) + (minutesInt * 60);
                        PowerOff();
                        string argument = "shutdown -s -t " + milliseconds.ToString();
                        commandPrompt.StandardInput.WriteLine(argument);
                        commandPrompt.StandardInput.WriteLine("exit");
                        _ = await ctx.Channel.SendMessageAsync("Shutdown started").ConfigureAwait(false);
                    }
                    else
                    {
                        await ctx.CreateResponseAsync(new DiscordInteractionResponseBuilder().WithContent("Please select a time at least 1 minute or higher."));
                    }
                }
            }
            catch (Exception) { }
        }
    }
}