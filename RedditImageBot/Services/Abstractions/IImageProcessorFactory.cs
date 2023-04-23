namespace RedditImageBot.Services.Abstractions
{
    public interface IImageProcessorFactory
    {
        IImageProcessor CreateImageProcessor(string imageFormat);
    }
}