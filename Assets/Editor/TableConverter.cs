using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

public sealed class TableConverter
{
    private const string RES_DIR = "./Assets/StreamingAssets/LocalResource.bundle/ResTable/";
    private const string RES_ROOT_NAME = "resRoot";
    private const string RES_SUFFIX = ".res";

    [MenuItem("MyTools/Convert Resource excel(.xlsx) to pb")]
    private static void ConvertCSVToPB()
	{
		ConvertTables();
		AssetDatabase.Refresh();
    }

    /// <summary>
    /// 转表
    /// </summary>
    private static void ConvertTables()
    {
        Debug.Log("converting xlsx to json...");
        string dir = Application.dataPath + "/Doc/ResTable/";
        string tmpDir = dir + "CSV/";
        //string param = string.Format("-jar {0}excel2json.jar {1} {2}", dir, dir, tmpDir); //java转表
        //var ans = RunProcessSync("java", param);
        string param = string.Format(" {0}excel2json.py {1} {2}", dir, dir, tmpDir); //Python转表
        var ans = RunProcessSync(@"C:\Users\jyhd\AppData\Local\Programs\Python\Python36\python", param);
        Debug.Log(param);
        Debug.Log(ans);
        dir = tmpDir;

        Debug.Log("converting json to pb...");
        var dirInfo = new DirectoryInfo(dir);
        FileInfo[] jsonFiles = dirInfo.GetFiles();
        var root = new ResRoot();
        foreach (var jsonFile in jsonFiles)
        {
            if (jsonFile.FullName.EndsWith(".json"))
            {
                var resFile = ParseJsonToResFile(jsonFile.FullName);
                if (resFile != null)
                {
                    var key = Path.GetFileNameWithoutExtension(jsonFile.Name);
                    root.fileNames.Add(key);
                    root.files.Add(resFile);
                }
                else
                {
                    Debug.LogError(string.Format("failed to parse json file {0}.", jsonFile));
                }
                jsonFile.Delete();
            }
        }

        FileHelper.WritePbToFile(root, string.Format("{0}/{1}{2}", RES_DIR, RES_ROOT_NAME, RES_SUFFIX), false);
        Debug.Log("done.");
    }

    /// <summary>
    /// excel转json文件
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    private static string RunProcessSync(string cmd, string arguments)
    {
        ProcessStartInfo processStartInfo = new ProcessStartInfo(cmd, arguments);
        processStartInfo.UseShellExecute = false;
        processStartInfo.RedirectStandardOutput = true;
        processStartInfo.RedirectStandardError = true;

        string text;
        string text2;
        using (Process process = Process.Start(processStartInfo))
        {
            using (StreamReader standardOutput = process.StandardOutput)
            {
                text = standardOutput.ReadToEnd();
            }
            using (StreamReader standardError = process.StandardError)
            {
                text2 = standardError.ReadToEnd();
            }
            process.WaitForExit();
        }
        return string.Format("stdout: {0}\n stderr: {1}", text, text2);
    }

    /// <summary>
    /// 存储json文件
    /// </summary>
    /// <param name="jsonFileName"></param>
    /// <returns></returns>
    private static ResFile ParseJsonToResFile(string jsonFileName)
    {
        List<string> colName = new List<string>();
        List<List<string>> colItem = new List<List<string>>();
        if (!ParseJson(jsonFileName, colName, colItem))
        {
            Debug.LogError("can't parse json " + jsonFileName);
            return null;
        }

        int rowCnt = colItem[0].Count;
        int colCnt = colName.Count;
        if (colCnt == 0 || rowCnt == 0)
        {
            Debug.LogError("empty file " + jsonFileName);
            return null;
        }
        for (int i = 0; i < colItem.Count; ++i)
        {
            if (colItem[i].Count != rowCnt)
            {
                Debug.LogError("row isn't matched. of file " + jsonFileName);
                return null;
            }
        }


        var res = new ResFile();
        for (int j = 0; j < colCnt; ++j)
        {
            var col = new ResItem();
            col.key = colName[j];
            col.value = colItem[j];
            res.items.Add(col);
        }

        return res;
    }

    private static bool ParseJson(string jsonFile, List<string> colName, List<List<string>> colItem)
    {
        //		Debug.Log("jsonFile " + jsonFile);

        //1、读取json文件的json字符串
        string jsonStr;
        using (var reader = new StreamReader(jsonFile))
        {
            jsonStr = reader.ReadToEnd();
        }

        if (String.IsNullOrEmpty(jsonStr))
        {
            Debug.LogError("No Such File. " + jsonFile);
            return false;
        }

        //2、得到的是一个字典数组
        object[] dicts = Util.FromJson<object[]>(jsonStr);
        if (dicts.Length == 0)
        {
            Debug.LogWarning("json obj is null. " + jsonFile);
            Debug.Log(jsonStr);
            return false;
        }

        //3、得到每一列的名字和行号
        colName.Clear();
        colItem.Clear();
        Dictionary<string, int> colNameDic = new Dictionary<string, int>();//每一列的名字和行号的字典
        var firstRowsDic = dicts[0] as Dictionary<string, object>;
        if (firstRowsDic == null)
        {
            Debug.LogWarning("first row json obj is error.");
            return false;
        }
        foreach (KeyValuePair<string, object> pair in firstRowsDic)
        {
            colNameDic.Add(pair.Key, colName.Count);
            colName.Add(pair.Key);
            colItem.Add(new List<string>());
        }
        //4、遍历第2步得到的字典数组，
        for (int i = 0; i < dicts.Length; ++i)
        {
            var rowDic = dicts[i] as Dictionary<string, object>;
            if (rowDic == null)
            {
                Debug.LogWarning(string.Format("jsonFile : {0} row : {1} json obj is null. ", jsonFile, i));
                continue;
            }
            foreach (KeyValuePair<string, object> pair in rowDic)
            {
                int j = colNameDic[pair.Key];
                colItem[j].Add(pair.Value.ToString().Trim());
            }
        }

        return true;
    }
}
