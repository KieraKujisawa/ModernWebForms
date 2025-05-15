using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

#pragma warning disable CS0219
#pragma warning disable CS0649
#pragma warning disable CS8600
#pragma warning disable CS8601
#pragma warning disable CS8602
#pragma warning disable CS8603
#pragma warning disable CS8604
#pragma warning disable CS8605
#pragma warning disable CS8618
#pragma warning disable CS8625

namespace WebForms.Helper.Util;

public class Prop
{
    public const string SESSIONKEY_USER = "PROP_SESSIONKEY_USER";
    public const string SESSIONKEY_USER_JSON = "PROP_SESSIONKEY_USER_JSON";
    public const string SESSIONKEY_USERID = "PROP_SESSIONKEY_USERID";
    private static object padlock = new object();
    private static Hashtable? props = null;

    private const string RESOURCES = "NDE.Lib.resources.";

    [DebuggerStepThrough]
    public static string GetEmbeddedString(string fileName)
    {
        byte[] fileData = GetEmbeddedFile("", fileName);

        string ret = System.Text.Encoding.UTF8.GetString(fileData);
        ret = Regex.Replace(ret, @"\p{C}+", string.Empty);

        return ret;
    }

    [DebuggerStepThrough]
    public static string GetEmbeddedFile(ref HttpResponse response, string fileName, bool isDefault = false)
    {

        string appName = !isDefault ? System.Configuration.ConfigurationManager.AppSettings["AppName"] : string.Empty;
        byte[] fileData = GetEmbeddedFile(appName, fileName);
        string contentType = MimeMapping.GetMimeMapping(fileName);

        if (fileData != null)
        {
            response.ContentType = contentType;
            response.BinaryWrite(fileData);
        }

        return contentType;
    }

    [DebuggerStepThrough]
    private static string GetResourceFolder(string appName)
    {
        string resFolder = "";

        switch (appName.ToLower())
        {
            case "mmrs":
            case "nmp":
            case "fmp":
                resFolder = "fmp_net";
                break;
            case "nm":
                resFolder = "nm_net";
                break;
            default:
                resFolder = "Documents";
                break;
        }

        return resFolder;
    }

    [DebuggerStepThrough]
    private static string GetResourceName(string[] files, string path, string fileName)
    {
        string ret = "";

        foreach (string f in files)
        {
            string temp = f.Replace(path, "");
            if (temp.ToLower() == fileName.ToLower())
            {
                ret = f;
                break;
            }
        }

        return ret;
    }

    [DebuggerStepThrough]
    private static byte[] GetEmbeddedFile(string appName, string fileName)
    {
        string resName;
        string resFolder = GetResourceFolder(appName);

        var a = Assembly.GetExecutingAssembly();

        if (appName == string.Empty)
        {
            resName = string.Concat(RESOURCES, fileName);
        }
        else
        {
            string resPath = string.Concat(RESOURCES, "files", ".", resFolder, ".");
            string[] files = a.GetManifestResourceNames();
            resName = GetResourceName(a.GetManifestResourceNames(), resPath, fileName);
        }

        byte[] fileData = null;
        Stream resFilestream = a.GetManifestResourceStream(resName);

        if (resFilestream != null)
        {
            using (var br = new BinaryReader(resFilestream))
            {
                fileData = new byte[resFilestream.Length];
                resFilestream.Read(fileData, 0, fileData.Length);
                br.Close();
            }
        }
        return fileData;
    }

    [DebuggerStepThrough]
    public static string GetProperty(string inProp)
    {

        if (props == null) lock (padlock) { loadAllProps(); } // "padlock" mutex
        if (!props.ContainsKey(inProp.ToLower())) throw new Exception("APPLICATION ERROR: Property " + inProp + " could not be found.");
        return (string)props[inProp.ToLower()];
    }

    [DebuggerStepThrough]
    public static void loadAllProps()
    {
        props = new Hashtable();
        //string baseDir = HttpContext.Current.Server.MapPath( "~/resources" );
        string baseDir = HttpRuntime.AppDomainAppPath + "resources";
        loadBaseDirProps(baseDir);
        loadSubDirProps(baseDir);
    }

    [DebuggerStepThrough]
    private static void loadBaseDirProps(string inBaseRootDir)
    {
        string[] files = Directory.GetFiles(inBaseRootDir);
        foreach (string file in files)
        {
            if (file.EndsWith(".scc")) continue; // project-generated file
            if (props.ContainsKey(file)) continue; // only one key per prop (1st wins)
            storeFileContents(file);
        }
    }

    [DebuggerStepThrough]
    private static void loadSubDirProps(string inDir)
    {
        string[] directories = Directory.GetDirectories(inDir);
        foreach (string directory in directories)
        {
            string[] files = Directory.GetFiles(directory);
            foreach (string file in files)
            {
                if (file.EndsWith(".scc")) continue; // project-generated file
                if (props.ContainsKey(file)) continue; // only one key per prop (1st wins)
                storeFileContents(file);
            }
            loadSubDirProps(directory); // recurse.
        }
    }

    /*private static string storeFileContents(string inFilePath)
		{
			string       fileName     = inFilePath;
			StreamReader reader       = File.OpenText( inFilePath );
			string       fileContents = reader.ReadToEnd();
			reader.Close();
			fileName = fileName.Substring( fileName.IndexOf( @"resources\" ) + 10 );
			fileContents = processPropertyContent( fileContents );
			props.Add( fileName, fileContents );
			return fileContents;
		}*/

    [DebuggerStepThrough]
    private static string storeFileContents(string inFilePath)
    {
        string fileContents = "";
        string fileName = inFilePath;
        StreamReader sr = new StreamReader(inFilePath);
        int fileSize = Convert.ToInt32(sr.BaseStream.Length);
        sr.Dispose();
        fileName = fileName.Substring(fileName.IndexOf(@"resources\") + 10);
        if (fileSize < 2000000) // Size of largest file that is read by this function 
        {
            fileContents = File.ReadAllText(inFilePath);
        }
        else
        {
            throw new Exception("File too long");
        }
        props.Add(fileName.ToLower(), fileContents);
        return fileContents;
    }
        
    [DebuggerStepThrough]
    public static string[,] GetPropNameArray()
    {
        string[,] retArr = new string[props.Count, 2];
        ArrayList al = new ArrayList();

        foreach (DictionaryEntry pName in props)
        {
            al.Add(pName.Key.ToString());
        }
        al.Sort();
        for (int index = 0; index < al.Count; index++)
        {
            retArr[index, 0] = al[index].ToString();
            retArr[index, 1] = al[index].ToString();
        }
        return retArr;
    }

}
