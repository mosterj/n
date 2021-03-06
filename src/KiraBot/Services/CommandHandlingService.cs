﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace KiraBot.Services
{
    public class CommandHandlingService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private IServiceProvider _provider;

        public CommandHandlingService(IServiceProvider provider, DiscordSocketClient discord, CommandService commands)
        {
            _discord = discord;
            _commands = commands;
            _provider = provider;

            _discord.MessageReceived += MessageReceived;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _provider = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            // Add additional initialization code here...
        }

        private async Task MessageReceived(SocketMessage rawMessage)
        {
            // Ignore system messages and messages from bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

			// Mark where the prefix ends and the command begins
			int argPos = 0;
			// Determine if the message has a valid prefix, adjust argPos 
			if (!message.HasCharPrefix('~', ref argPos)) return;

			var context = new SocketCommandContext(_discord, message);
            var result = await _commands.ExecuteAsync(context, argPos, _provider);

			// If the command failed, notify the user
			if (!result.IsSuccess)
			{
				var eb = new Discord.EmbedBuilder()
				{
					Title = "Error!",
					Description = result.ErrorReason,
					Color = new Discord.Color(255, 0, 0)
				};
				await message.Channel.SendMessageAsync("", embed: eb);
			}
		}
    }
}
