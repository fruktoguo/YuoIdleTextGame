using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace YuoTools.Extend.Helper
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
    public class WindowsHelper
    {
        /// <summary>
        /// 打开文件选择对话框，支持单选文件。
        /// </summary>
        /// <param name="formatName">文件格式名称（如 "txt"）。</param>
        /// <param name="fileSize">文件路径缓冲区大小，默认256。</param>
        /// <param name="title">对话框标题，默认 "选择文件"。</param>
        /// <param name="initialDir">默认打开的目录，默认是 StreamingAssets 目录。</param>
        /// <returns>返回选中的文件信息，如果取消则返回默认值。</returns>
        public static OpenFileName OpenSelectFile(string formatName, int fileSize = 256, string title = "选择文件",
            string initialDir = null)
        {
            return OpenSelectFileInternal(formatName, fileSize, false, title, initialDir);
        }

        /// <summary>
        /// 打开文件选择对话框，支持多选文件。
        /// </summary>
        /// <param name="formatName">文件格式名称（如 "txt"）。</param>
        /// <param name="fileSize">文件路径缓冲区大小，默认256。</param>
        /// <param name="title">对话框标题，默认 "选择文件"。</param>
        /// <param name="initialDir">默认打开的目录，默认是 StreamingAssets 目录。</param>
        /// <returns>返回选中的文件信息列表，如果取消则返回空列表。</returns>
        public static List<string> OpenSelectFiles(string formatName, int fileSize = 256, string title = "选择文件",
            string initialDir = null)
        {
            OpenFileName openFileName = OpenSelectFileInternal(formatName, fileSize, true, title, initialDir);
            if (openFileName.file != null)
            {
                // 处理多选结果
                return new List<string>(openFileName.file.Split(new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries));
            }

            return new List<string>();
        }

        /// <summary>
        /// 内部方法，用于打开文件选择对话框。
        /// </summary>
        private static OpenFileName OpenSelectFileInternal(string formatName, int fileSize, bool allowMultiSelect,
            string title, string initialDir)
        {
            OpenFileName openFileName = new OpenFileName();
            openFileName.structSize = Marshal.SizeOf(openFileName);
            openFileName.filter = $"{formatName}文件(*.{formatName})\0*.{formatName}";
            openFileName.file = new string(new char[fileSize]);
            openFileName.maxFile = openFileName.file.Length;
            openFileName.fileTitle = new string(new char[64]);
            openFileName.maxFileTitle = openFileName.fileTitle.Length;
            openFileName.initialDir = string.IsNullOrEmpty(initialDir)
                ? Application.streamingAssetsPath.Replace('/', '\\')
                : initialDir.Replace('/', '\\');
            openFileName.title = title;
            openFileName.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;

            if (allowMultiSelect)
            {
                openFileName.flags |= 0x00000200; // 允许多选
            }

            if (LocalDialog.GetSaveFileName(openFileName))
            {
                openFileName.file.Log();
                openFileName.fileTitle.Log();
                return openFileName;
            }

            return default;
        }

        /// <summary>
        /// 打开文件选择对话框，支持选择文件夹。
        /// </summary>
        /// <param name="title">对话框标题，默认 "选择目录"。</param>
        /// <param name="initialDir">默认打开的目录，默认是 StreamingAssets 目录。</param>
        /// <returns>返回选中的文件夹路径，如果取消则返回空字符串。</returns>
        public static string OpenSelectFolder(string title = "选择目录", string initialDir = null)
        {
            var dialog = new FileOpenDialog() as IFileOpenDialog;
            try
            {
                // 设置选项
                uint options;
                dialog.GetOptions(out options);
                options |= (uint)(FILEOPENDIALOGOPTIONS.FOS_PICKFOLDERS | FILEOPENDIALOGOPTIONS.FOS_FORCEFILESYSTEM);
                dialog.SetOptions(options);

                // 设置标题
                dialog.SetTitle(title);

                // 设置初始目录
                if (!string.IsNullOrEmpty(initialDir))
                {
                    IShellItem item;
                    SHCreateItemFromParsingName(initialDir, IntPtr.Zero, typeof(IShellItem).GUID, out item);
                    if (item != null)
                    {
                        dialog.SetFolder(item);
                    }
                }

                // 显示对话框
                uint hr = dialog.Show(IntPtr.Zero);
                if (hr == 0) // 用户点击"选择文件夹"
                {
                    IShellItem result;
                    dialog.GetResult(out result);
                    string path;
                    result.GetDisplayName(SIGDN.SIGDN_FILESYSPATH, out path);
                    return path;
                }
            }
            catch (Exception ex)
            {
                // Debug.LogError($"选择文件夹出错: {ex.Message}");
            }
            finally
            {
                Marshal.ReleaseComObject(dialog);
            }

            return "";
        }

        /// <summary>
        /// 屏幕截图,记得隐藏不需要截取的部分
        /// </summary>
        /// <param name="name"></param>
        private static void CaptureScreen(string name)
        {
            ScreenCapture.CaptureScreenshot($"{name}.png", 0);
        }

        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="path"></param>
        public static void OpenFolder(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            //判断是否是文件
            path = FileHelper.GetFilePath(path);

            path = path.Replace("/", "\\");

            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        public static void CopyToClipboard(string str)
        {
            UnityEngine.GUIUtility.systemCopyBuffer = str;
        }

        public static MemoryInfo LogMemory()
        {
            MemoryInfo MemInfo = new MemoryInfo();
            GlobalMemoryStatus(ref MemInfo);

            double totalMb = MemInfo.TotalPhysical / 1024 / 1024;
            double avaliableMb = MemInfo.AvailablePhysical / 1024 / 1024;

            Debug.Log($"物理内存共有:{totalMb}MB");
            Debug.Log($"可使用的物理内存:{avaliableMb}MB");
            Debug.Log($"剩余内存百分比：{Math.Round((avaliableMb / totalMb) * 100, 2)}");
            return MemInfo;
        }

        [DllImport("kernel32")]
        public static extern void GlobalMemoryStatus(ref MemoryInfo meminfo);

        /// <summary>
        /// 打开CMD并执行命令
        /// </summary>
        /// <param name="cmd"></param>
        public static void Command(string cmd)
        {
            var process = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    Arguments = "/k" + cmd,
                    CreateNoWindow = false,
                }
            };
            try
            {
                process.Start();
                process.WaitForExit();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.Message);
            }
            finally
            {
                process.Close();
            }
        }

        [ComImport]
        [Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
        private class FileOpenDialog
        {
        }

        [ComImport]
        [Guid("42f85136-db7e-439c-85f1-e4075d135fc8")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileDialog
        {
            [PreserveSig]
            uint Show([In] IntPtr parent);

            void SetFileTypes([In] uint cFileTypes,
                [In] [MarshalAs(UnmanagedType.LPArray)] COMDLG_FILTERSPEC[] rgFilterSpec);

            void SetFileTypeIndex([In] uint iFileType);
            void GetFileTypeIndex(out uint piFileType);
            void Advise([In, MarshalAs(UnmanagedType.Interface)] IFileDialogEvents pfde, out uint pdwCookie);
            void Unadvise([In] uint dwCookie);
            void SetOptions([In] uint fos);
            void GetOptions(out uint fos);
            void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);
            void SetFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);
            void GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);
            void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);
            void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);
            void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            void GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);
            void AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, uint fdap);
            void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            void Close([MarshalAs(UnmanagedType.Error)] uint hr);
            void SetClientGuid([In] ref Guid guid);
            void ClearClientData();
            void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
        }

        [ComImport]
        [Guid("d57c7288-d4ad-4768-be02-9d969532d960")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileOpenDialog : IFileDialog
        {
            // IFileDialog methods
            new uint Show([In] IntPtr parent);

            new void SetFileTypes([In] uint cFileTypes,
                [In] [MarshalAs(UnmanagedType.LPArray)] COMDLG_FILTERSPEC[] rgFilterSpec);

            new void SetFileTypeIndex([In] uint iFileType);
            new void GetFileTypeIndex(out uint piFileType);
            new void Advise([In, MarshalAs(UnmanagedType.Interface)] IFileDialogEvents pfde, out uint pdwCookie);
            new void Unadvise([In] uint dwCookie);
            new void SetOptions([In] uint fos);
            new void GetOptions(out uint fos);
            new void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);
            new void SetFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);
            new void GetFolder([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);
            new void GetCurrentSelection([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);
            new void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);
            new void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);
            new void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
            new void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);
            new void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
            new void GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);
            new void AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, uint fdap);
            new void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);
            new void Close([MarshalAs(UnmanagedType.Error)] uint hr);
            new void SetClientGuid([In] ref Guid guid);
            new void ClearClientData();
            new void SetFilter([MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);

            // IFileOpenDialog-specific methods
            void GetResults([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppenum);
            void GetSelectedItems([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppsai);
        }

        [ComImport]
        [Guid("b63ea76d-1f85-456f-a19c-48159efa858b")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItemArray
        {
            void BindToHandler([In, MarshalAs(UnmanagedType.Interface)] IntPtr pbc, [In] ref Guid rbhid,
                [In] ref Guid riid, out IntPtr ppvOut);

            void GetPropertyStore([In] int Flags, [In] ref Guid riid, out IntPtr ppv);
            void GetPropertyDescriptionList([In] ref PROPERTYKEY keyType, [In] ref Guid riid, out IntPtr ppv);
            void GetAttributes([In] uint dwAttribFlags, [In] uint sfgaoMask, out uint psfgaoAttribs);
            void GetCount(out uint pdwNumItems);
            void GetItemAt([In] uint dwIndex, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);
            void EnumItems([MarshalAs(UnmanagedType.Interface)] out IntPtr ppenumShellItems);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct PROPERTYKEY
        {
            private Guid fmtid;
            private uint pid;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct COMDLG_FILTERSPEC
        {
            [MarshalAs(UnmanagedType.LPWStr)] public string pszName;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszSpec;
        }

        [ComImport]
        [Guid("973510DB-7D7F-452B-8975-74A85828D354")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileDialogEvents
        {
            [PreserveSig]
            uint OnFileOk([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

            [PreserveSig]
            uint OnFolderChanging([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd,
                [In, MarshalAs(UnmanagedType.Interface)] IShellItem psiFolder);

            void OnFolderChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);
            void OnSelectionChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

            void OnShareViolation([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd,
                [In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, out uint pResponse);

            void OnTypeChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

            void OnOverwrite([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd,
                [In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, out uint pResponse);
        }

        [ComImport]
        [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            void BindToHandler([In] IntPtr pbc, [In] ref Guid bhid, [In] ref Guid riid,
                [Out, MarshalAs(UnmanagedType.Interface)] out IntPtr ppv);

            void GetParent([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);
            void GetDisplayName([In] SIGDN sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
            void GetAttributes([In] uint sfgaoMask, out uint psfgaoAttribs);
            void Compare([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, [In] uint hint, out int piOrder);
        }

        private enum SIGDN : uint
        {
            SIGDN_NORMALDISPLAY = 0x0,
            SIGDN_PARENTRELATIVEPARSING = 0x80018001,
            SIGDN_DESKTOPABSOLUTEPARSING = 0x80028000,
            SIGDN_PARENTRELATIVEEDITING = 0x80031001,
            SIGDN_DESKTOPABSOLUTEEDITING = 0x8004c000,
            SIGDN_FILESYSPATH = 0x80058000,
            SIGDN_URL = 0x80068000,
            SIGDN_PARENTRELATIVEFORADDRESSBAR = 0x8007c001,
            SIGDN_PARENTRELATIVE = 0x80080001,
            SIGDN_PARENTRELATIVEFORUI = 0x80094001
        }

        private enum FILEOPENDIALOGOPTIONS : uint
        {
            FOS_OVERWRITEPROMPT = 0x2,
            FOS_STRICTFILETYPES = 0x4,
            FOS_NOCHANGEDIR = 0x8,
            FOS_PICKFOLDERS = 0x20,
            FOS_FORCEFILESYSTEM = 0x40,
            FOS_ALLNONSTORAGEITEMS = 0x80,
            FOS_NOVALIDATE = 0x100,
            FOS_ALLOWMULTISELECT = 0x200,
            FOS_PATHMUSTEXIST = 0x800,
            FOS_FILEMUSTEXIST = 0x1000,
            FOS_CREATEPROMPT = 0x2000,
            FOS_SHAREAWARE = 0x4000,
            FOS_NOREADONLYRETURN = 0x8000,
            FOS_NOTESTFILECREATE = 0x10000,
            FOS_HIDEMRUPLACES = 0x20000,
            FOS_HIDEPINNEDPLACES = 0x40000,
            FOS_NODEREFERENCELINKS = 0x100000,
            FOS_OKBUTTONNEEDSINTERACTION = 0x200000,
            FOS_DONTADDTORECENT = 0x2000000,
            FOS_FORCESHOWHIDDEN = 0x10000000,
            FOS_DEFAULTNOMINIMODE = 0x20000000,
            FOS_FORCEPREVIEWPANEON = 0x40000000,
            FOS_SUPPORTSTREAMABLEITEMS = 0x80000000
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        private static extern void SHCreateItemFromParsingName(
            [In] [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            [In] IntPtr pbc,
            [In] [MarshalAs(UnmanagedType.LPStruct)]
            Guid riid,
            [Out] [MarshalAs(UnmanagedType.Interface, IidParameterIndex = 2)]
            out IShellItem ppv);
    }

    #region 类型

    public struct MemoryInfo
    {
        public uint Length;
        public uint MemoryLoad;
        public ulong TotalPhysical; //总内存
        public ulong AvailablePhysical; //可用物理内存
        public ulong TotalPageFile;
        public ulong AvailablePageFile;
        public ulong TotalVirtual;
        public ulong AvailableVirtual;
    }

    public struct OpenFileName
    {
        public int structSize;
        public IntPtr dlgOwner;
        public IntPtr instance;

        [MarshalAs(UnmanagedType.LPTStr)] public string filter;

        [MarshalAs(UnmanagedType.LPTStr)] public string customFilter;

        public int maxCustFilter;
        public int filterIndex;

        [MarshalAs(UnmanagedType.LPTStr)] public string file;

        public int maxFile;

        [MarshalAs(UnmanagedType.LPTStr)] public string fileTitle;

        public int maxFileTitle;

        [MarshalAs(UnmanagedType.LPTStr)] public string initialDir;

        [MarshalAs(UnmanagedType.LPTStr)] public string title;

        public int flags;
        public short fileOffset;
        public short fileExtension;

        [MarshalAs(UnmanagedType.LPTStr)] public string defExt;

        public IntPtr custData;
        public IntPtr hook;

        [MarshalAs(UnmanagedType.LPTStr)] public string templateName;

        public IntPtr reservedPtr;
        public int reservedInt;
        public int flagsEx;
    }

    public enum BrowseFlag
    {
        BIF_RETURNONLYFSDIRS = 0x0001, // For finding a folder to start document searching
        BIF_DONTGOBELOWDOMAIN = 0x0002, // For starting the Find Computer
        BIF_STATUSTEXT = 0x0004,
        BIF_RETURNFSANCESTORS = 0x0008,
        BIF_EDITBOX = 0x0010,
        BIF_VALIDATE = 0x0020, // insist on valid result (or CANCEL)

        BIF_BROWSEFORCOMPUTER = 0x1000, // Browsing for Computers.
        BIF_BROWSEFORPRINTER = 0x2000, // Browsing for Printers
        BIF_BROWSEINCLUDEFILES = 0x4000 // Browsing for Everything
    }

    //目录对话框所需数据类型
    public struct BrowseInfo
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;

        [MarshalAs(UnmanagedType.LPTStr)] public string displayName;

        [MarshalAs(UnmanagedType.LPTStr)] public string title;

        public int flags;
        public IntPtr callback;
        public IntPtr lparam;
    }

    public class LocalDialog
    {
        //链接指定系统函数       打开文件对话框
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

        public static bool GetOFN([In, Out] OpenFileName ofn)
        {
            return GetOpenFileName(ofn);
        }

        //链接指定系统函数        另存为对话框
        [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
        public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);

        public static bool GetSFN([In, Out] OpenFileName ofn)
        {
            return GetSaveFileName(ofn);
        }

        //弹出"选择目录"对话框
        [DllImport("Shell32.dll", SetLastError = true)]
        public static extern IntPtr SHBrowseForFolder([In, Out] BrowseInfo browse);

        //结果集解析函数
        [DllImport("Shell32.dll", SetLastError = true)]
        public static extern bool SHGetPathFromIDList([In] IntPtr dlist, [In] byte[] PathName);
    }

    #endregion

    /// <summary>
    /// 常用CMD命令
    /// </summary>
    public static class CmdCommand
    {
        /// <summary>
        /// 打开文件管理器
        /// </summary>
        public const string OpenFolder = "explorer.exe";

        /// <summary>
        /// 立即关机
        /// </summary>
        public const string AutoShutdown = "shutdown -s -t 0";

        /// <summary>
        /// 立即重启
        /// </summary>
        public const string AutoRestart = "shutdown -r -t 0";

        /// <summary>
        /// 取消关机
        /// </summary>
        public const string CancelShutdown = "shutdown -a";

        /// <summary>
        /// 刷新DNS
        /// </summary>
        public const string RefreshDns = "ipconfig /flushdns";

        /// <summary>
        /// 清空DNS缓存
        /// </summary>
        public const string ClearDnsCache = "ipconfig /displaydns";

        public static Dictionary<string, string> commandDic = new Dictionary<string, string>()
        {
            { "OpenFolder", OpenFolder },
            { "AutoShutdown", AutoShutdown },
            { "AutoRestart", AutoRestart },
            { "CancelShutdown", CancelShutdown },
            { "RefreshDns", RefreshDns },
            { "ClearDnsCache", ClearDnsCache },
        };
    }
#endif
}