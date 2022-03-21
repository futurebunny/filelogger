#define IRL_INCLUDE_EXTENSIONS 
#define IRL_SPRINT_3_OR_LATER
namespace Karambolo.Extensions.Logging.File
{
    public interface ILogFilePathFormatContext
    {
        FileLogEntry LogEntry { get; }

        string DateFormat { get; }
        string CounterFormat { get; }
        int Counter { get; }

        string FormatDate(string inlineFormat);
        string FormatCounter(string inlineFormat);

#if IRL_INCLUDE_EXTENSIONS && IRL_SPRINT_3_OR_LATER
        string FormatTickCount(string inlineFormat);
#endif
    }


    public delegate string LogFilePathPlaceholderResolver(string placeholderName, string inlineFormat, ILogFilePathFormatContext context);
}
