using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public interface IBotService
    {
        Task InitializeAsync();
        Task GenerateImagesAsync();
    }
}
