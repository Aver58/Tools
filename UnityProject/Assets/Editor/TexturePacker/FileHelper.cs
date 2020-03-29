using System.IO;

class FileHelper
{
    public static void CreateDirectory(string path)
    {
        if(!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static string ReadTextFromFile(string path, string defaultValue = "")
    {
        string ret = defaultValue;

        FileInfo fi = new FileInfo(path);
        if(fi.Exists)
        {
            StreamReader reader = fi.OpenText();
            ret = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
        }

        return ret;
    }
}
