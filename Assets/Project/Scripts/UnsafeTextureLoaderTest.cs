using System;
using System.IO;
using System.Net;
using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;

public class UnsafeTextureLoaderTest : MonoBehaviour
{
    [SerializeField] private string _url = "https://blogs.unity3d.com/wp-content/uploads/2021/03/image12-640x360.jpg";

    private string DirPath => Path.Combine(Application.dataPath, "Project/download");
    private string FilePath => Path.Combine(DirPath, "downloaded-image.png");
    private string MetaFilePath => Path.Combine(DirPath, "downloaded-image.png.texturemeta");

    private Texture2D _texture = null;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 130, 30), "Download"))
        {
            Download();
        }

        if (GUI.Button(new Rect(10, 50, 130, 30), "Load"))
        {
            Load();
        }
        
        if (_texture == null)
        {
            return;
        }
        
        GUI.DrawTexture(new Rect(0, 0, _texture.width, _texture.height), _texture);
    }
    
    private async void Download()
    {
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(_url))
        {
            await req.SendWebRequest();
    
            Texture2D tex = DownloadHandlerTexture.GetContent(req);
    
            byte[] data = tex.GetRawTextureData();

            if (!Directory.Exists(DirPath))
            {
                Directory.CreateDirectory(DirPath);
            }
    
            File.WriteAllBytes(FilePath, data);
            
            MetaWriter.Write(MetaFilePath, tex);
        }
    }

    private void Load()
    {
        string metaString = File.ReadAllText(MetaFilePath);
        MetaData meta = MetaData.Parse(metaString);
        
        _texture = new Texture2D(meta.width, meta.height, meta.format, false);
        
        AsyncReader asyncReader = new AsyncReader(_texture, _ =>
        {
            Debug.Log("Done");
        });

        asyncReader.Load(FilePath);
    }
}