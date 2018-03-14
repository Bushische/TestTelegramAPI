using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace TelegramBushTestBotBG
{
    /// <summary>
    /// abstract class for next possible response messages
    /// </summary>
    abstract class ResponseMessage
    {
        protected long ChatId { get; private set; }
        protected int ReplMessageId { get; set; }

        public ResponseMessage(Message origMes)
        {
            ChatId = origMes.Chat.Id;
            ReplMessageId = 0;// origMes.MessageId;
        }

        /// <summary>
        /// apply message to Bot
        /// </summary>
        /// <param name="bot"></param>
        public virtual async void ApplyToBot(TelegramBotClient bot) { }
    }

    class TextResponseMessage: ResponseMessage
    {
        protected string ResponseMessage;
        public TextResponseMessage(Message origMes, string inResponse = "") : base(origMes)
        {
            ResponseMessage = inResponse;
        }

        public override async void ApplyToBot(TelegramBotClient bot)
        {
            await bot.SendTextMessageAsync(ChatId, ResponseMessage, replyToMessageId:ReplMessageId);
        }
    }

    /// <summary>
    /// version of message which will send respond with quotation of original message
    /// </summary>
    class QuoteResponseMessage: TextResponseMessage
    {
        public QuoteResponseMessage(Message origMes, string inResponse) : base(origMes, inResponse)
        {
            ReplMessageId = origMes.MessageId;
        }
        
    }
}
