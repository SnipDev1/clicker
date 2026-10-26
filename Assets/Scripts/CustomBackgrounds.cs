using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Свои картинки игрока. Файлы лежат в persistentDataPath/CustomBackgrounds,
/// при запуске подхватываются автоматически и попадают в каталог фонов.
/// </summary>
public static class CustomBackgrounds
{
    public const string FolderName = "CustomBackgrounds";
    private const int Width = 720;
    private const int Height = 1280;

    public static string FolderPath => Path.Combine(Application.persistentDataPath, FolderName);

    /// <summary>
    /// Перечитать папку с нуля. Нужно на старте: при выключенном Domain Reload статический
    /// каталог живёт между запусками, а спрайты прошлой сессии Unity уже уничтожила.
    /// </summary>
    public static int ReloadAll()
    {
        foreach (BackgroundDefinition definition in BackgroundsCatalog.All)
        {
            if (definition.style != BackgroundStyle.Custom)
                continue;

            if (definition.customSprite != null)
            {
                if (definition.customSprite.texture != null)
                    UnityEngine.Object.Destroy(definition.customSprite.texture);
                UnityEngine.Object.Destroy(definition.customSprite);
            }

            definition.customSprite = null;
        }

        BackgroundsCatalog.RemoveCustoms();
        return RegisterFolder();
    }

    /// <summary>Удалить свою картинку: файл из папки игры и спрайт.</summary>
    public static void Delete(BackgroundDefinition definition)
    {
        if (definition == null)
            return;

        try
        {
            if (!string.IsNullOrEmpty(definition.customPath) && File.Exists(definition.customPath))
                File.Delete(definition.customPath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("CustomBackgrounds: не удалось удалить файл — " + exception.Message);
        }

        if (definition.customSprite != null)
        {
            if (definition.customSprite.texture != null)
                UnityEngine.Object.Destroy(definition.customSprite.texture);
            UnityEngine.Object.Destroy(definition.customSprite);
        }

        definition.customSprite = null;
    }

    /// <summary>Добавить в каталог все новые картинки из папки. Возвращает, сколько добавилось.</summary>
    public static int RegisterFolder()
    {
        int added = 0;

        foreach (string path in GetImageFiles())
        {
            string id = IdFor(path);
            if (BackgroundsCatalog.Get(id) != null)
                continue;

            BackgroundDefinition definition = Load(path, id);
            if (definition == null)
                continue;

            BackgroundsCatalog.Register(definition);
            added++;
        }

        return added;
    }

    /// <summary>Скопировать выбранный файл в папку игры и сразу загрузить.</summary>
    public static BackgroundDefinition Import(string sourcePath)
    {
        try
        {
            Directory.CreateDirectory(FolderPath);

            string target = Path.Combine(FolderPath, Path.GetFileName(sourcePath));
            if (File.Exists(target))
            {
                string name = Path.GetFileNameWithoutExtension(sourcePath);
                string extension = Path.GetExtension(sourcePath);
                target = Path.Combine(FolderPath, name + "_" + DateTime.Now.Ticks + extension);
            }

            File.Copy(sourcePath, target);

            string id = IdFor(target);
            BackgroundDefinition existing = BackgroundsCatalog.Get(id);
            return existing ?? Load(target, id);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("CustomBackgrounds: не удалось добавить картинку — " + exception.Message);
            return null;
        }
    }

    public static void OpenFolder()
    {
        try
        {
            Directory.CreateDirectory(FolderPath);
            Application.OpenURL("file://" + FolderPath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("CustomBackgrounds: не удалось открыть папку — " + exception.Message);
        }
    }

    private static IEnumerable<string> GetImageFiles()
    {
        string[] files;

        try
        {
            Directory.CreateDirectory(FolderPath);
            files = Directory.GetFiles(FolderPath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("CustomBackgrounds: не удалось прочитать папку — " + exception.Message);
            return new string[0];
        }

        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        var result = new List<string>();
        foreach (string path in files)
        {
            if (IsImage(path))
                result.Add(path);
        }
        return result;
    }

    private static bool IsImage(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();
        return extension == ".png" || extension == ".jpg" || extension == ".jpeg";
    }

    private static string IdFor(string path)
    {
        return "custom_" + Path.GetFileName(path).ToLowerInvariant();
    }

    private static BackgroundDefinition Load(string path, string id)
    {
        byte[] bytes;

        try
        {
            bytes = File.ReadAllBytes(path);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("CustomBackgrounds: не удалось прочитать файл — " + exception.Message);
            return null;
        }

        var source = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!source.LoadImage(bytes))
        {
            UnityEngine.Object.Destroy(source);
            Debug.LogWarning("CustomBackgrounds: не картинка — " + path);
            return null;
        }

        Texture2D cover = CreateCover(source, Width, Height);
        UnityEngine.Object.Destroy(source);

        Sprite sprite = Sprite.Create(cover, new Rect(0, 0, Width, Height), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = Path.GetFileNameWithoutExtension(path);

        return new BackgroundDefinition
        {
            id = id,
            displayName = ShortName(Path.GetFileNameWithoutExtension(path)),
            price = 0,
            style = BackgroundStyle.Custom,
            customSprite = sprite,
            customPath = path,
            ownedByDefault = true
        };
    }

    /// <summary>Центральный кроп под 9:16, чтобы картинка заполнила экран без искажений.</summary>
    private static Texture2D CreateCover(Texture2D source, int width, int height)
    {
        float targetAspect = width / (float)height;
        float sourceAspect = source.width / (float)source.height;

        float scaleX = 1f;
        float scaleY = 1f;
        float offsetX = 0f;
        float offsetY = 0f;

        if (sourceAspect > targetAspect)
        {
            scaleX = targetAspect / sourceAspect;
            offsetX = (1f - scaleX) * 0.5f;
        }
        else
        {
            scaleY = sourceAspect / targetAspect;
            offsetY = (1f - scaleY) * 0.5f;
        }

        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = source.name,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.DontSave
        };

        RenderTexture renderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.sRGB);
        Graphics.Blit(source, renderTexture, new Vector2(scaleX, scaleY), new Vector2(offsetX, offsetY));

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTexture);

        return texture;
    }

    private static string ShortName(string value)
    {
        value = value.Replace('_', ' ').Trim();
        if (string.IsNullOrEmpty(value))
            return "my image";

        return value.Length <= 18 ? value : value.Substring(0, 18);
    }
}
