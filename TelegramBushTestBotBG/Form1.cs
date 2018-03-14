using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Telegram.Bot.Types;

namespace TelegramBushTestBotBG
{
    /* small background process to support telegram bot BushTestBot
     * Token: in mail
     
        based on : http://aftamat4ik.ru/pishem-bota-telegram-na-c/
         */
    public partial class Form1 : Form
    {

        BackgroundWorker bw;

        public Form1()
        {
            InitializeComponent();
            bw = new BackgroundWorker();
            bw.DoWork += BW_TelegramLoop;
        }

        private void bStart_Click(object sender, EventArgs e)
        {
            var TokenKey = this.tbToken.Text;
            if (!string.IsNullOrEmpty(TokenKey) && !bw.IsBusy)
            {
                this.bStart.Enabled = false;
                bw.RunWorkerAsync(TokenKey);
            }
        }

        private async void BW_TelegramLoop(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker tbw = sender as BackgroundWorker;
            var key = e.Argument as string;
            try
            {
                var bot = new Telegram.Bot.TelegramBotClient(key);
                await bot.SetWebhookAsync("");
                int offset = 0;

                while (true)
                {
                    var updates = await bot.GetUpdatesAsync(offset);

                    foreach (var update in updates)
                    {
                        bool res = await ProcessTelegramUpdate(bot, update);
                        offset = update.Id + 1;
                    }

                    System.Threading.Thread.Sleep(1000);    // to avoid CPU load
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error {ex.Message}");
                SetStatus("Работа остановлена по ошибке");

            }
        }

        MessageProcessor msproc = new MessageProcessor();
        /// <summary>
        /// Process all telegram messages
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="inUpd"></param>
        /// <returns></returns>
        async Task<bool> ProcessTelegramUpdate(Telegram.Bot.TelegramBotClient bot, Update inUpd)
        {
            SetStatus((inUpd.EditedMessage??inUpd.Message)?.From?.Id
                +":"+ inUpd.Message?.Text);
            try
            {
                #region //
                //if (inUpd.Type == Telegram.Bot.Types.Enums.UpdateType.EditedMessage)
                //{
                //    await bot.SendTextMessageAsync(inUpd.EditedMessage.Chat.Id, "Нехорошо редактировать сообщения", replyToMessageId: inUpd.EditedMessage.MessageId);
                //}
                //else if (inUpd.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                //{
                //    await bot.SendTextMessageAsync(inUpd.Message.Chat.Id, $"Echo: {inUpd.Message.Text}", replyToMessageId: inUpd.Message.MessageId);
                //}
                #endregion
                var res = msproc.ProcessUpdate(inUpd);
                res?.ApplyToBot(bot);
            }
            catch (Exception ex)
            {
                SetStatus($"Message process error: {inUpd?.Id}");
            }
            return true;
        }


        /// <summary>
        /// Set status line
        /// </summary>
        /// <param name="inContr"></param>
        /// <param name="inAct"></param>
        void SetStatus(string inMessage)
        {
            Action act = () => { lState.Text = inMessage; };
            if (lState.InvokeRequired)
                lState.Invoke(act);
            else
                act();
        }
    }//
}
