﻿namespace AzureBot.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Autofac;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Builder.Dialogs.Internals;
    using Microsoft.Bot.Connector;

    public static class DialogExtensions
    {
        public static void NotifyLongRunningOperation<T>(this Task<T> operation, IDialogContext context, Func<T, string> handler)
        {
            operation.ContinueWith(
                async (t, ctx) =>
                {
                    var messageText = handler(t.Result);
                    await NotifyUser((IDialogContext)ctx, messageText);
                },
                context);
        }

        public static void NotifyLongRunningOperation<T>(this Task<T> operation, IDialogContext context, Func<T, IDialogContext, string> handler)
        {
            operation.ContinueWith(
                async (t, ctx) =>
                {
                    var messageText = handler(t.Result, (IDialogContext)ctx);
                    await NotifyUser((IDialogContext)ctx, messageText);
                }, 
                context);
        }

        private static async Task NotifyUser(IDialogContext context, string messageText)
        {
            var reply = context.MakeMessage();
            reply.Text = messageText;

            using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, reply))
            {
                var client = scope.Resolve<IConnectorClient>();
                await client.Messages.SendMessageAsync(reply);
            }
        }
    }
}