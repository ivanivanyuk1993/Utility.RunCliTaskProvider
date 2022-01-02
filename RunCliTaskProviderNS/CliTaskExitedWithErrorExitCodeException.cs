namespace RunCLITaskProviderNS;

public class CliTaskExitedWithErrorExitCodeException : Exception
{
    public readonly int ExitCode;

    public CliTaskExitedWithErrorExitCodeException(
        int exitCode,
        string taskName
    ) : base(
        message: $"\"{taskName}\" exited with error exit code: {exitCode}"
    )
    {
        ExitCode = exitCode;
    }
}