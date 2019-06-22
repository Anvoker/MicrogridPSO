using UnityEngine;
using System.IO;
using System;
using System.Security;
using SFB;
using SSM.GridUI;
using System.Collections;
using UnityEngine.Networking;
using SSM.CSV;
using System.Runtime.InteropServices;
using System.Text;
using SSM.Grid;

namespace SSM.UI
{
    [System.Serializable]
    public class MicrogridFileInput : MonoBehaviour
    {
        public Microgrid microgrid;
        public ErrorMessenger errorMessenger;
        public string csvLoadFilePath;
        public string csvSaveFilePath;
        public string csvLoadDirectoryPath;
        public string csvSaveDirectoryPath;

        private readonly string openTitle = "Open File";
        private readonly string saveTitle = "Save File";
        private readonly string defaultFileName = "Microgrid.csv";

        private MicrogridVar[] mVarsToLoad;
        private bool downloadInProgress;
        private bool finishedDownload;
        private Tuple<string, string> downloadResult;

        private static readonly ExtensionFilter[] LoadExtension = new[]
        {
            new ExtensionFilter("Comma-Separated Values Files", "csv"),
            new ExtensionFilter("All Files", "*" ),
        };

        private static readonly ExtensionFilter[] SaveExtension = new[]
{
            new ExtensionFilter("Comma-Separated Values Files", "csv"),
            new ExtensionFilter("All Files", "*" ),
        };

        public void LoadDialog()
        {
            LoadDialog(MGMisc.GetInputVars());
        }

        public void SaveInputVars()
        {
            SaveDialog(MGMisc.GetInputVars());
        }

        public void SaveOutputVars()
        {
            SaveDialog(MGMisc.GetOutputVars());
        }

        public void SaveAll()
        {
            SaveDialog(null);
        }

        public void LoadDialog(MicrogridVar[] mVars)
        {
            mVarsToLoad = (MicrogridVar[])mVars.Clone();

#if !UNITY_WEBGL || UNITY_EDITOR
            var paths = StandaloneFileBrowser.OpenFilePanel(
                openTitle, 
                GetLoadInitialPath(),
                LoadExtension, 
                false);

            if (paths.Length > 0)
            {
                csvLoadFilePath = paths[0];
                csvLoadDirectoryPath = Path.GetDirectoryName(paths[0]);
                LoadFromFile(paths[0], mVars);
            }
#else
            UploadFile(gameObject.name, "OnFileUpload", ".csv, .txt", false);
#endif
        }

        public void SaveDialog(MicrogridVar[] mVars)
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            var path = StandaloneFileBrowser.SaveFilePanel(
                saveTitle, 
                GetSaveInitialPath(),
                defaultFileName, 
                SaveExtension);

            if (path.Length > 0)
            {
                csvSaveFilePath = path;
                csvSaveDirectoryPath = Path.GetDirectoryName(path);
                Save(mVars);
            }
#else
            string text = MGMisc.MicrogridVarsToString(microgrid, mVars);
            var bytes = Encoding.UTF8.GetBytes(text);
            DownloadFile(gameObject.name, "OnFileDownload", "outpso.csv", bytes, bytes.Length);
#endif
        }

        protected void Awake()
        {
            errorMessenger = errorMessenger ?? FindObjectOfType<ErrorMessenger>();
            csvLoadFilePath = String.Empty;
            csvSaveFilePath = String.Empty;
            csvLoadDirectoryPath = String.Empty;
            csvSaveDirectoryPath = String.Empty;
        }

        protected void Update()
        {
            if (downloadInProgress && finishedDownload)
            {
                if (downloadResult.Item1 == null)
                {
                    SendErrorMessage(downloadResult.Item2);
                }
                else
                {
                    LoadFromString(downloadResult.Item1, mVarsToLoad);
                }

                downloadInProgress = false;
            }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void UploadFile(
            string gameObjectName, 
            string methodName, 
            string filter, 
            bool multiple);

        [DllImport("__Internal")]
        private static extern void DownloadFile(
            string gameObjectName, string methodName,
            string filename, 
            byte[] byteArray, 
            int byteArraySize);
#endif

        public void OnFileUpload(string url) 
            => StartCoroutine(WebRequestDownloadFile(url));

        public void OnFileDownload() 
            => SendInfoMessage("File successfully downloaded.");

        private IEnumerator WebRequestDownloadFile(string url)
        {
            using (var web = UnityWebRequest.Get(url))
            {
                downloadInProgress = true;
                yield return web.SendWebRequest();

                if (web.isNetworkError || web.isHttpError)
                {
                    downloadResult = Tuple.Create<string, string>(null, web.error);
                }
                else
                {
                    downloadResult = Tuple.Create(web.downloadHandler.text, "");
                    finishedDownload = true;
                }
            }
        }

        private void LoadFromString(string content, MicrogridVar[] mVars = null)
        {
            try
            {
                CSVHelper.ReadInputFromString(content, microgrid, mVars);
            }
            catch (Exception e)
            {
                if (IsHandleableFileException(e))
                {
                    SendErrorMessage(e.Message);
                }
                else
                {
                    throw;
                }
            }

            Notify();
        }

        private void LoadFromFile(string path, MicrogridVar[] mVars = null)
        {
            try
            {
                string content = File.ReadAllText(path);
                CSVHelper.ReadInputFromString(content, microgrid, mVars);
            }
            catch (Exception e)
            {
                if (IsHandleableFileException(e))
                {
                    SendErrorMessage(e.Message);
                }
                else
                {
                    throw;
                }
            }

            Notify();
        }

        private void Notify()
        {
            microgrid.NotifyLoaded();
            var uiInputs = FindObjectsOfType<MicrogridInputFields>();
            for (int i = 0; i < uiInputs.Length; i++)
            {
                uiInputs[i].MicrogridToAllUI();
            }
        }

        private void Save(MicrogridVar[] mVars = null)
        {
            string text = MGMisc.MicrogridVarsToString(microgrid, mVars);
            try
            {
                File.WriteAllText(csvSaveFilePath, text);
            }
            catch (Exception e)
            {
                if (IsHandleableFileException(e))
                {
                    SendErrorMessage(e.Message);
                }
                else
                {
                    throw;
                }
            }
        }

        private string GetLoadInitialPath()
        {
            if (!String.IsNullOrEmpty(csvLoadDirectoryPath))
            {
                return csvLoadDirectoryPath;
            }
            else
            {
                return GetExePath();
            }
        }

        private string GetSaveInitialPath()
        {
            if (!String.IsNullOrEmpty(csvSaveDirectoryPath))
            {
                return csvSaveDirectoryPath;
            }
            else
            {
                return GetExePath();
            }
        }

        private static string GetExePath()
        {
            var path = Application.dataPath;
            if (Application.platform == RuntimePlatform.OSXPlayer)
            {
                path += "/../../";
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer)
            {
                path += "/../";
            }
            return path;
        }

        private static bool IsHandleableFileException(Exception e)
        {
            return     e is PathTooLongException
                    || e is DirectoryNotFoundException
                    || e is DirectoryNotFoundException
                    || e is IOException
                    || e is UnauthorizedAccessException
                    || e is NotSupportedException
                    || e is SecurityException;
        }

        private void SendErrorMessage(string message) 
            => errorMessenger?.SendErrorMessage(message);

        private void SendInfoMessage(string message) 
            => errorMessenger?.SendInfoMessage(message);

    }
}
