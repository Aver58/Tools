using System.Diagnostics;

class ShellHelper
{
    public static void ProcessCommand(string command, string argument, string workingdir = null)
    {
        ProcessStartInfo start = new ProcessStartInfo(command);
        start.Arguments = argument;
        start.CreateNoWindow = false;
        start.ErrorDialog = true;
        start.UseShellExecute = true;
        if(!string.IsNullOrEmpty(workingdir))
        {
            start.WorkingDirectory = workingdir;
        }

        if(start.UseShellExecute)
        {
            start.RedirectStandardOutput = false;
            start.RedirectStandardError = false;
            start.RedirectStandardInput = false;
        }
        else
        {
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
            start.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
        }

        Process p = Process.Start(start);

        p.WaitForExit();
        p.Close();
    }
}
