using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Системный диалог выбора картинки.
/// В редакторе — EditorUtility, в Windows-сборке — Win32 GetOpenFileName.
/// </summary>
public static class NativeFileDialog
{
    public static bool IsAvailable
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            return true;
#else
            return false;
#endif
        }
    }

    public static bool TryPickImage(out string path)
    {
#if UNITY_EDITOR
        path = EditorUtility.OpenFilePanel("Choose image", "", "png,jpg,jpeg");
        return !string.IsNullOrEmpty(path);
#elif UNITY_STANDALONE_WIN
        return TryPickWindows(out path);
#else
        path = null;
        return false;
#endif
    }

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential,
        CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
    private struct OpenFileName
    {
        public int structSize;
        public IntPtr dlgOwner;
        public IntPtr instance;
        public string filter;
        public string customFilter;
        public int maxCustFilter;
        public int filterIndex;
        public IntPtr file;
        public int maxFile;
        public IntPtr fileTitle;
        public int maxFileTitle;
        public string initialDir;
        public string title;
        public int flags;
        public short fileOffset;
        public short fileExtension;
        public string defExt;
        public IntPtr custData;
        public IntPtr hook;
        public string templateName;
        public IntPtr reservedPtr;
        public int reservedInt;
        public int flagsEx;
    }

    [System.Runtime.InteropServices.DllImport("comdlg32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode,
        ExactSpelling = true)]
    private static extern bool GetOpenFileNameW(ref OpenFileName openFileName);

    private const int MaxPath = 1024;

    private static bool TryPickWindows(out string path)
    {
        path = null;
        IntPtr buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(MaxPath * 2);

        try
        {
            for (int i = 0; i < MaxPath; i++)
                System.Runtime.InteropServices.Marshal.WriteInt16(buffer, i * 2, 0);

            var dialog = new OpenFileName
            {
                structSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(OpenFileName)),
                filter = "Images\0*.png;*.jpg;*.jpeg\0",
                file = buffer,
                maxFile = MaxPath,
                title = "Choose image",
                defExt = "png",
                // FILEMUSTEXIST | PATHMUSTEXIST | NOCHANGEDIR
                flags = 0x00001000 | 0x00000800 | 0x00000008
            };

            if (!GetOpenFileNameW(ref dialog))
                return false;

            path = System.Runtime.InteropServices.Marshal.PtrToStringUni(buffer);
            return !string.IsNullOrEmpty(path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("NativeFileDialog: " + exception.Message);
            return false;
        }
        finally
        {
            System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer);
        }
    }

#endif
}
