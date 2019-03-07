using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReviewGrabberBot.Handlers;
using ReviewGrabberBot.Models;
using ReviewGrabberBot.Options;
using ReviewGrabberBot.Services;
using Telegram.Bot;

namespace ReviewGrabberBot
{
    public class Program
    {
        internal static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.Configure<ContextOptions>(options =>
                    {
                        options.ConnectionString = "mongodb://localhost";
                        options.DatabaseName = "reviewbot";
                        options.ReviewCollectionName = "reviews";
                    });
                    services.Configure<BotOptions>(options =>
                    {
                        options.AdminId = "336510341";
                    });
                    services.Configure<NotifierOptions>(options =>
                    {
                        options.Restaurants = new List<Restaurant>
                        {
                            new Restaurant
                            {
                                Name = "Brisket Eat & Fun",
                                YellUrl = "https://www.yell.ru/spb/com/brisket-eat-fun_11911251/",
                                GoogleUrl = "https://www.google.com/maps/place/Brisket+Eat%26Fun/@59.9390097,30.3232879,17z/data=!3m1!4b1!4m5!3m4!1s0x4696310e51b441c5:0x9e1d9de1fc875954!8m2!3d59.939007!4d30.325482",
                                RestoclubUrl = "https://www.restoclub.ru/spb/place/brisket-eat-fun/opinions",
                                FoursquareUrl = "https://ru.foursquare.com/v/brisket-eat--fun/5a1161ef60d11b3755321549",
                                RestoratingUrl = "https://www.restorating.ru/opinions/brisket-eat-fun/",
                                TripadvisorUrl = "https://www.tripadvisor.ru/Restaurant_Review-g298507-d13189819-Reviews-Brisket_Eat_Fun-St_Petersburg_Northwestern_District.html"
                            },
                            new Restaurant
                            {
                                Name = "BLOODY MARY Bar & Grill",
                                YellUrl = "https://www.yell.ru/spb/com/bloody-mary-bar_11862123/",
                                GoogleUrl = "https://www.google.com/maps/place/Bloody+Mary+Bar+%26+Grill/@59.9296286,30.321197,17z/data=!4m7!3m6!1s0x469631014a8f1bb5:0xf8f13ca063d99203!8m2!3d59.9296259!4d30.323391!9m1!1b1",
                                RestoclubUrl = "https://www.restoclub.ru/spb/place/bloody-mary/opinions",
                                FoursquareUrl = "https://ru.foursquare.com/v/bloody-mary-bar--grill/5762cd0c498e1dbf2f6f0c83?tipsSort=recent",
                                RestoratingUrl = "https://www.restorating.ru/spb/catalogue/bloody-mary-bar/",
                                TripadvisorUrl = "https://www.tripadvisor.ru/Restaurant_Review-g298507-d10437838-Reviews-BLOODY_MARY_Bar_Grill-St_Petersburg_Northwestern_District.html"
                            }
                        };
                    });

                    services.AddSingleton<Context>();
                    services.AddSingleton(new TelegramBotClient(Constants.BotToken));
                    services.AddSingleton<UpdateHandler>();

                    services.AddHostedService<BotHandlerService>();
                    services.AddHostedService<ScriptRunnerService>();
                })
                .RunConsoleAsync();
        }
    }
}