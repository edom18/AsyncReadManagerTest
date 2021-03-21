using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Networking;

public class UnityWebRequestAwaiter : INotifyCompletion
{
    private UnityWebRequestAsyncOperation _asyncOp = null;
    private Action _continuation = null;

    public bool IsCompleted => _asyncOp.isDone;

    public UnityWebRequestAwaiter(UnityWebRequestAsyncOperation asyncOp)
    {
        _asyncOp = asyncOp;
        _asyncOp.completed += OnRquestCompleted;
    }

    public void OnCompleted(Action continuation)
    {
        _continuation = continuation;
    }

    public void GetResult()
    {
        _asyncOp.completed -= OnRquestCompleted;
        _continuation = null;
        _asyncOp = null;
    }

    private void OnRquestCompleted(AsyncOperation asyncOp)
    {
        _continuation?.Invoke();
    }
}

public static class UnityWebRequestAsyncOperationExtension
{
    public static UnityWebRequestAwaiter GetAwaiter(this UnityWebRequestAsyncOperation asyncOp)
    {
        return new UnityWebRequestAwaiter(asyncOp);
    }
}