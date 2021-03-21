using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct MetaData
{
    public int width;
    public int height;
    public TextureFormat format;

    public override string ToString()
    {
        return $"{width},{height},{(int)format}";
    }

    public static MetaData Parse(string metaString)
    {
        string[] data = metaString.Split(',');

        return new MetaData
        {
            width = int.Parse(data[0]),
            height = int.Parse(data[1]),
            format = (TextureFormat)int.Parse(data[2]),
        };
    }
}

public static class MetaWriter
{
    public static void Write(string path, Texture2D texture)
    {
        MetaData meta = new MetaData
        {
            width = texture.width,
            height = texture.height,
            format = texture.format,
        };
        
        File.WriteAllText(path, meta.ToString());
    }
}
