using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.IO;
using UnityEngine;

public class EnablePassThrough_cmd : MonoBehaviour
{
    private Thread enablePassThrough_t;
    private Thread disablePassThrough_t;
    string path;
    // Start is called before the first frame update
    private void Awake() {
        #if UNITY_EDITOR
        path = System.IO.Path.Combine(Directory.GetCurrentDirectory() + "\\Assets\\Samples\\VIVE\\OpenXR\\Samples\\SceneUnderstanding\\PassThrough Plugin\\OfflineTool.exe");
        #elif UNITY_STANDALONE
        path = System.IO.Path.Combine(Directory.GetCurrentDirectory() + "/OfflineTool.exe");
        #endif
        UnityEngine.Debug.Log(path);
        
        enablePassThrough_t = new Thread(new ThreadStart(EnablePassThrough));
        enablePassThrough_t.Start();
    }
    void Start()
    {

    }

    public void EnablePassThrough()
    {
        string cmd = "PassThrough 1 0";
        RunCMD(cmd);
        UnityEngine.Debug.Log("enable pass through");
    }

    public void DisablePassThrough()
    {
        string cmd = "PassThrough 0 4";
        RunCMD(cmd);
    }

    public void RunCMD(string command)
    {
        Process process = new Process();
        if(System.IO.File.Exists(path))
        {
            process.StartInfo.FileName = path;
            process.StartInfo.Arguments = command;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            UnityEngine.Debug.Log(process.StandardOutput.ReadToEnd());
        } else
        {
            UnityEngine.Debug.Log("OfflineTool.exe isn't exists.");
        }
    }
    private void OnApplicationQuit()
    {
        disablePassThrough_t = new Thread(new ThreadStart(DisablePassThrough));
        disablePassThrough_t.Start();
    }

}
