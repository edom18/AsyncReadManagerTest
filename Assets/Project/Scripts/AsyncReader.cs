using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// Read an image from a file with async.
/// </summary>
public class AsyncReader
{
    private ReadHandle _readHandle = default;
    private NativeArray<ReadCommand> _readCommands = default;
    private long _fileSize = 0;

    private Texture2D _texture = null;
    private Action<Texture2D> _callback = null;

    public AsyncReader(Texture2D outTexture, Action<Texture2D> callback)
    {
        _texture = outTexture;
        _callback = callback;
    }

    public unsafe void Load(string path)
    {
        FileInfo info = new FileInfo(path);
        _fileSize = info.Length;

        _readCommands = new NativeArray<ReadCommand>(1, Allocator.Persistent);
        _readCommands[0] = new ReadCommand
        {
            Offset = 0,
            Size = _fileSize,
            Buffer = (byte*)UnsafeUtility.Malloc(_fileSize, UnsafeUtility.AlignOf<byte>(), Allocator.Persistent),
        };

        _readHandle = AsyncReadManager.Read(path, (ReadCommand*)_readCommands.GetUnsafePtr(), 1);

        CheckLoop();
    }

    private async void CheckLoop()
    {
        while (true)
        {
            if (_readHandle.Status == ReadStatus.InProgress)
            {
                await Task.Delay(Mathf.FloorToInt(Time.deltaTime * 1000));
                continue;
            }

            if (_readHandle.Status != ReadStatus.Complete)
            {
                Dispose();
                NotifyCallback();
                break;
            }

            ReadTexture();
            NotifyCallback();
            break;
        }
    }

    private void NotifyCallback()
    {
        _callback?.Invoke(_texture);
    }

    private unsafe void ReadTexture()
    {
        IntPtr ptr = (IntPtr)_readCommands[0].Buffer;

        try
        {
            _texture.LoadRawTextureData(ptr, (int)_fileSize);
            _texture.Apply();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            Dispose();
        }
    }

    private unsafe void Dispose()
    {
        _readHandle.Dispose();
        UnsafeUtility.Free(_readCommands[0].Buffer, Allocator.Persistent);
        _readCommands.Dispose();
    }
}