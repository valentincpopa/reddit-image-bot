﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RedditImageBot.Services
{
    public interface IImgurService
    {
        Task<string> UploadImageAsync(MemoryStream fileStream);
    }
}
