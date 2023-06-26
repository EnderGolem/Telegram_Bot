using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Segment.Model;

namespace Telegram_bot
{
    public class Bot
    {
        static ITelegramBotClient bot = null;

        private static Dictionary<string, Func<Update, bool>> commands = new Dictionary<string, Func<Update, bool>>() {
            {"/start", ProcessStart },
            {"/command1", ProcessCommand1 },
            {"/help", ProcessHelp },
            {"/shark", ProcessShark },
            {"/screenshot",ProcessMakeScreenShot }
        };



        public Bot(string token)
        {
            bot = new TelegramBotClient(token);
            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { },
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
        }


        protected async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                case UpdateType.Message:
                    BotOnMessageReceived(update);
                    break;
                default:
                    throw new NotImplementedException();
            }

        }

        protected void BotOnMessageReceived(Update update)
        {
            if (bot == null || update == null)
                return;

            var message = update.Message;
            var chatID = message.Chat.Id.ToString();
            var text = message.Text;
            if (commands.ContainsKey(text))
            {
                if (!commands[text](update))
                {
                    Console.WriteLine($"ChatID: {chatID} text: {text}  была вызвана ошибка ");
                }
            }
            else
            {
                Console.WriteLine($"Введена неизвестная комманда ChatID:{chatID}");
            }

        }

        protected async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        private static async Task<bool> SendFile(string chatID, string path, string fileName)
        {
                using (var stream = File.OpenRead(path + fileName))
                {
                    var input = InputFile.FromStream(stream, fileName);
                    await bot.SendPhotoAsync(chatID, input);
                    return true;
                }
            return false;
            
        }

        private static async Task<bool> SendTextMessage(string chatID, string text)
        {
            try
            {
                await bot.SendTextMessageAsync(chatID, text, replyMarkup: GetButtons());
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static IReplyMarkup? GetButtons()
        {
            var list = new List<KeyboardButton>();
            foreach (var element in commands)
            {
                list.Add(new KeyboardButton(element.Key));
            }

            return new ReplyKeyboardMarkup(list);
        }

        #region commands
        private static bool ProcessStart(Update update)
        {
            return SendTextMessage(update.Message.Chat.Id.ToString(), "Это начальное сообщение бота!").Result;
        }
        private static bool ProcessCommand1(Update update)
        {
            return SendTextMessage(update.Message.Chat.Id.ToString(), "Введена комманда номер 1").Result;
        }

        private static bool ProcessHelp(Update update)
        {
            try
            {
                string allCommands = "Доступные комманды:\n";
                foreach (var element in commands)
                {
                    allCommands += element.Key + '\n';
                }
                return SendTextMessage(update.Message.Chat.Id.ToString(), allCommands).Result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool ProcessShark(Update update)
        {
            try
            {
                return SendFile(update.Message.Chat.Id.ToString(), "d:\\Users\\Григорий\\Desktop\\мусор\\", "акула.jpg").Result;
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }


        private static bool ProcessMakeScreenShot(Update update)
        {
            try
            {
                Rectangle bounds = new Rectangle(0,0,1600,900);
                Bitmap screenshot = new Bitmap(bounds.Width, bounds.Height, PixelFormat.Format32bppArgb);
                using (Graphics graphics = Graphics.FromImage(screenshot))
                {
                    graphics.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                screenshot.Save("screenshot.png", ImageFormat.Png);

                return SendFile(update.Message.Chat.Id.ToString(), ".\\", "screenshot.png").Result;
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
        #endregion

    }
}
