namespace Helpmebot.Model.Interfaces
{
    public interface ICommandParserEntity
    {
        string CommandKeyword { get; }
        string CommandChannel { get; }
    }
}