using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Drawing;

namespace TelegramBot
{
    class TelegramBot
    {
        #region Variable Definitions

        public TelegramBotClient Bot;

        Random rand = new Random();

        string apiKey = System.IO.File.ReadAllText("telegram_api_key.txt");

        string[] freshestMemes;

        string[] quotes;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new TelegramBot
        /// </summary>
        public TelegramBot()
        {
            // instanciates the bot object
            Bot = new TelegramBotClient(apiKey);

            // event handlers
            Bot.OnMessage += BotOnMessageReceived;
            Bot.OnReceiveError += BotOnReceiveError;

            // Gets the bot information from telegram
            var me = Bot.GetMeAsync().Result;

            // Sets the console title to bot username
            Console.Title = me.Username;

            Console.WriteLine(Console.Title + " is running.");

            // get quotes from file
            loadQuotes();

            Bot.StartReceiving();
            Console.ReadLine();
            Bot.StopReceiving();

        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// This event takes place when an error occurs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BotOnReceiveError(object sender, ReceiveErrorEventArgs e)
        {
            Debugger.Break();
        }

        /// <summary>
        /// This event takes place whenever a message is received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BotOnMessageReceived(object sender, MessageEventArgs e)
        {
            // gets the message that was sent
            var message = e.Message;

            // returns if it was null or if it was not a text message
            if (message == null || message.Type != MessageType.TextMessage) return;

            /***********************
             * COMMANDS BEGIN HERE *
             ***********************/
            if (message.Text.StartsWith("/photo")) // send a photo
            {
                photoCommand(e);
            }
            else if (message.Text.StartsWith("/addquote@TelegramBot")) // add a quote
            {
                addQuoteRnnACommand(e);
            }
            else if (message.Text.StartsWith("/addquote")) // add a quote 
            {
                addQuoteCommand(e);
            }
            else if (message.Text.StartsWith("/quote")) // send a quote
            {
                displayQuoteCommand(e);
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Controls what occurs when the photo command is entered
        /// </summary>
        /// <param name="e"></param>
        async void photoCommand(MessageEventArgs e)
        {
            var message = e.Message;

            loadImages();

            int randomPhotoIndex = rand.Next(freshestMemes.Length);
            string imageToPost = freshestMemes[randomPhotoIndex];


            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);

            string file = imageToPost;

            var fileName = file.Split('\\').Last();

            using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var fts = new FileToSend(fileName, fileStream);

                await Bot.SendPhotoAsync(message.Chat.Id, fts);
            }
        }

        /// <summary>
        /// Controls what occurs when the addquote command is entered
        /// </summary>
        /// <param name="e"></param>
        async void addQuoteCommand(MessageEventArgs e)
        {
            var message = e.Message;

            if (message.Text.Length > 10)
            {
                string messageContents = message.Text.Substring(10);

                using (System.IO.StreamWriter file =
                new System.IO.StreamWriter("quotes.txt", true))
                {
                    file.WriteLine(messageContents + ",");
                }

                loadQuotes();

                await Bot.SendTextMessageAsync(message.Chat.Id, "Quote Saved: " + messageContents);
            }
            else
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, "Syntax is /addquote \"Your Quote\" - Author");
            }
        }

        /// <summary>
        /// Controls what occurs when the addquote@TelegramBot command is entered
        /// </summary>
        /// <param name="e"></param>
        async void addQuoteRnnACommand(MessageEventArgs e)
        {
            var message = e.Message;

            if (message.Text.Length > 17)
            {
                string messageContents = message.Text.Substring(17);

                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter("quotes.txt", true))
                {
                    file.WriteLine(messageContents + ",");
                }

                loadQuotes();

                await Bot.SendTextMessageAsync(message.Chat.Id, "Quote Saved: " + messageContents);
            }
            else
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, "Syntax is /addquote@TelegramBot \"Your Quote\" - Author");
            }
        }

        /// <summary>
        /// Controls what occurs when the quote command is entered
        /// </summary>
        /// <param name="e"></param>
        async void displayQuoteCommand(MessageEventArgs e)
        {
            var message = e.Message;

            await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.Typing);

            int randomQuoteIndex = rand.Next(quotes.Length);
            string quoteToPost = quotes[randomQuoteIndex];

            await Bot.SendTextMessageAsync(message.Chat.Id, quoteToPost.TrimEnd(quoteToPost[quoteToPost.Length - 1]));

        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads quotes from a text file
        /// </summary>
        void loadQuotes()
        {
            quotes = System.IO.File.ReadAllLines("quotes.txt");
        }

        /// <summary>
        /// Loads image paths from directory
        /// </summary>
        void loadImages()
        {
            string[] files = Directory.GetFiles("images");

            freshestMemes = files;

        }

        #endregion
    }
}
