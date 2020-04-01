using System.Diagnostics;
using System.Text;

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

    static StringBuilder ms_log;
    static StringBuilder ms_error;
    public static void ProcessCommandEx(string command, string argument, string workingdir = null, bool outputLog = false)
    {
        ms_log = new StringBuilder();
        ms_error = new StringBuilder();

        Process process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = argument;
        if(!string.IsNullOrEmpty(workingdir))
        {
            process.StartInfo.WorkingDirectory = workingdir;
        }
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.OutputDataReceived += OnOutputDataReceived;
        process.ErrorDataReceived += OnErrorDataReceived;

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();


        if(outputLog && ms_log.Length > 0)
        {
            UnityEngine.Debug.Log(ms_log);
        }


        if(ms_error.Length > 0)
        {
            throw new System.Exception(string.Format("Process Failed:\n{0}", ms_error.ToString()));
        }


        ms_log = null;
        ms_error = null;
    }

    static void OnOutputDataReceived(object sender, DataReceivedEventArgs args)
    {
        if(!string.IsNullOrEmpty(args.Data))
        {
            // UnityEngine.Debug.Log (args.Data);
            ms_log.AppendLine(args.Data);
        }
    }

    static void OnErrorDataReceived(object sender, DataReceivedEventArgs args)
    {
        if(!string.IsNullOrEmpty(args.Data))
        {
            // UnityEngine.Debug.LogError (args.Data);
            ms_error.AppendLine(args.Data);
        }
    }
}
