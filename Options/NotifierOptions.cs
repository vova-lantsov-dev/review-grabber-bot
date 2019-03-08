using System.Collections.Generic;
using ReviewGrabberBot.Models;

namespace ReviewGrabberBot.Options
{
    internal sealed class NotifierOptions
    {
        public List<Restaurant> Restaurants;
        public string WorkingDirectory;
        public string ScrapyPath;
    }
}