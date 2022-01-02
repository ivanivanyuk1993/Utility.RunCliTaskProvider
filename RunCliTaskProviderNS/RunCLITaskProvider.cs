using System.CommandLine;
using System.CommandLine.IO;
using CliExitCodeProviderNS;
using GetJsonDateProviderNS;
using RunCLITaskProviderNS;

namespace RunCliTaskProviderNS;

public static class RunCliTaskProvider
{
    public static Task<int> RunCLITask(
        IConsole console,
        string taskName,
        Func<CancellationToken, Task> runCliTaskFunc,
        CancellationToken cancellationToken
    )
    {
        return RunCLITask(
            console: console,
            taskName: taskName,
            runCliTaskFunc: async cancellationToken2 =>
            {
                await runCliTaskFunc(arg: cancellationToken2);
                return CliExitCodeProvider.Success;
            },
            cancellationToken: cancellationToken
        );
    }

    public static async Task<int> RunCLITask(
        IConsole console,
        string taskName,
        Func<CancellationToken, Task<int>> runCliTaskFunc,
        CancellationToken cancellationToken
    )
    {
        var startTime = DateTimeOffset.UtcNow;
        PrintTaskStart(
            console: console,
            startTime: startTime,
            taskName: taskName
        );
        DateTimeOffset? finishTime = null;
        try
        {
            var exitCode = await runCliTaskFunc(arg: cancellationToken);
            finishTime = DateTimeOffset.UtcNow;
            if (exitCode != CliExitCodeProvider.Success)
            {
                throw new CliTaskExitedWithErrorExitCodeException(
                    exitCode: exitCode,
                    taskName: taskName
                );
            }
            PrintTaskFinish(
                console: console,
                startTime: startTime,
                finishTime: finishTime.Value,
                taskName: taskName
            );
            return exitCode;
        }
        catch (Exception exception)
        {
            finishTime ??= DateTimeOffset.UtcNow;
            PrintTaskFinishWithException(
                console: console,
                exception: exception,
                startTime: startTime,
                finishTime: finishTime.Value,
                taskName: taskName
            );
            switch (exception)
            {
                case CliTaskExitedWithErrorExitCodeException cliTaskExitedWithErrorExitCodeException:
                    return cliTaskExitedWithErrorExitCodeException.ExitCode;
                case OperationCanceledException:
                    return CliExitCodeProvider.ScriptTerminatedByControlC;
                default:
                    return CliExitCodeProvider.CatchallForGeneralErrors;
            }
        }
    }

    private static void PrintTaskFinish(
        IStandardOut console,
        DateTimeOffset startTime,
        DateTimeOffset finishTime,
        string taskName
    )
    {
        console.Out.Write(value: $@"

""{taskName}"" successfully finished at:
{finishTime.GetJsonDate()}
""{taskName}"" took:
{finishTime - startTime}

"
        );
    }

    private static void PrintTaskFinishWithException(
        IStandardError console,
        Exception exception,
        DateTimeOffset startTime,
        DateTimeOffset finishTime,
        string taskName
    )
    {
        console.Error.Write(value: $@"

{exception}
""{taskName}"" finished with failure at:
{finishTime.GetJsonDate()}
""{taskName}"" took:
{finishTime - startTime}

"
        );
    }

    private static void PrintTaskStart(
        IStandardOut console,
        DateTimeOffset startTime,
        string taskName
    )
    {
        console.Out.Write(value: $@"

""{taskName}"" started at:
{startTime.GetJsonDate()}

"
        );
    }
}