using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using OpenWeatherMap;

namespace TelegramBushTestBotBG
{
    enum CommandEnum
    {
        None = 0,
        Repeat = 1,
        Weather = 2
    }

    class MessageProcessor
    {
        /// <summary>
        /// Map of user which work in command
        /// </summary>
        SortedDictionary<int, CommandEnum> userInCommandProcessing = new SortedDictionary<int, CommandEnum>();

        public ResponseMessage ProcessUpdate(Update inUpd)
        {
            ResponseMessage resObj = null;

            if (inUpd.Type == Telegram.Bot.Types.Enums.UpdateType.EditedMessage)
            {
                resObj = new QuoteResponseMessage(inUpd.EditedMessage, "Нехорошо редактировать сообщения");
                //await bot.SendTextMessageAsync(inUpd.EditedMessage.Chat.Id, "Нехорошо редактировать сообщения", replyToMessageId: inUpd.EditedMessage.MessageId);
            }
            else
            {
                if (inUpd.Message.Entities?.FirstOrDefault()?.Type == Telegram.Bot.Types.Enums.MessageEntityType.BotCommand)
                {
                    string commandName = inUpd.Message.Text;
                    CommandEnum command = CommandEnum.None;
                    switch (commandName)
                    {
                        case "/weather": command = CommandEnum.Weather; break;
                        case "/repeat": command = CommandEnum.Repeat; break;
                    }
                    userInCommandProcessing[inUpd.Message.From.Id] = command;
                    //resObj = new TextResponseMessage(inUpd.Message, $"Ой… я пока плохо с коммандами");
                }

                if (userInCommandProcessing.ContainsKey(inUpd.Message.From.Id))
                {
                    switch (userInCommandProcessing[inUpd.Message.From.Id])
                    {
                        case CommandEnum.Weather: resObj = CommandWeather(inUpd); break;
                        case CommandEnum.Repeat: resObj = CommandRepeat(inUpd); break;
                    }
                }
            }
            if (resObj == null)
                resObj = new TextResponseMessage(inUpd.Message, $"Что-то я даже не знаю, как реагировать");
            return resObj;
        }

        #region Repeat command
        /// <summary>
        /// don't move to next state, staying in REPEAT mode
        /// </summary>
        /// <param name="inUpd"></param>
        /// <returns></returns>
        ResponseMessage CommandRepeat(Update inUpd)
        {
            return new TextResponseMessage(inUpd.Message, $"Echo: {inUpd.Message.Text}");
        }
        #endregion

        #region Weather command
        string APIkey = "9e65c8a3c6962a1cc0a5ec4d7e2c7aa7";
        OpenWeatherMapClient weatherClient = null;

        OpenWeatherMapClient GetWeatherServiceClient()
        {
            if (weatherClient == null)
            {
                weatherClient = new OpenWeatherMapClient(APIkey);
            }
            return weatherClient;
        }

        ResponseMessage CommandWeather(Update inUpd)
        {
            CurrentWeatherResponse weather = null;
            if (inUpd.Message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
            {
                if (inUpd.Message.Text == "/weather")
                    return new TextResponseMessage(inUpd.Message, $"Жду вашу локацию");
                // Пытаемся поискать по локации
                weather = GetWeatherByName(inUpd.Message.Text);
            }
            if (inUpd.Message.Type == Telegram.Bot.Types.Enums.MessageType.LocationMessage)
            {
                weather = GetWeatherByLocation(inUpd.Message.Location.Longitude, inUpd.Message.Location.Latitude);
            }

            // Подготовка результата
            userInCommandProcessing[inUpd.Message.From.Id] = CommandEnum.None;
            if (weather == null)
                return new TextResponseMessage(inUpd.Message, "Не удалось определить погоду");
            return new TextResponseMessage(inUpd.Message, $"Погода в {weather.City.Name}: t°={weather.Temperature.Value}°C (min: {weather.Temperature.Min}°C, max: {weather.Temperature.Max}°C)" +
                $", w={weather.Wind.Speed.Value}м/с ({weather.Wind.Direction.Name})");
        }

        CurrentWeatherResponse GetWeatherByName(string place)
        {
            try
            {
                var client = GetWeatherServiceClient();
                var resTask = client.CurrentWeather.GetByName(place, MetricSystem.Metric);
                resTask.Wait();
                var res = resTask.Result;
                return res;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        CurrentWeatherResponse GetWeatherByLocation(float longitude, float latitude)
        {
            try
            {
                var client = GetWeatherServiceClient();
                var resTask = client.CurrentWeather.GetByCoordinates(new Coordinates() { Longitude = longitude, Latitude = latitude }, MetricSystem.Metric);
                resTask.Wait();
                var res = resTask.Result;
                return res;
            }
            catch (Exception ex)
            {
                return null;
            }
}
        #endregion
    }//
}
