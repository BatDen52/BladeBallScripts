using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class GLFlusher : MonoBehaviour
{

    private const string LIBRARY_NAME = "glflusher";

    [DllImport(LIBRARY_NAME)]
    private static extern int GetSomeNumber();

    [DllImport(LIBRARY_NAME)]
    private static extern IntPtr GetFlushFuncPtr();

    [DllImport(LIBRARY_NAME)]
    private static extern IntPtr GetFinishFuncPtr();

    enum MODE
    {
        FLUSH,
        FINISH,
        MIXED
    };

    private IntPtr _flushFuncPtr;
    private IntPtr _finishFuncPtr;
    private int _counter = 0;

    private MODE _mode = MODE.FLUSH;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
#if !UNITY_EDITOR
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3)
        {
            DontDestroyOnLoad(this.gameObject);
            Debug.LogError(GetSomeNumber()); //test function to see plugin is loaded

            _flushFuncPtr = GetFlushFuncPtr();
            _finishFuncPtr = GetFinishFuncPtr();
        } else
#endif
#endif
        {
            Destroy(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch(_mode)
        {
            case(MODE.FLUSH):
                {
                    GL.IssuePluginEvent(_flushFuncPtr, 0);
                    //Debug.LogError("FLUSHING GL");
                    break;
                }
            case (MODE.FINISH):
                {
                    GL.IssuePluginEvent(_finishFuncPtr, 0);
                    break;
                }
            case (MODE.MIXED):
                {
                    if (_counter == 0)
                    {
                        GL.IssuePluginEvent(_flushFuncPtr, 0);
                    }
                    else
                        GL.IssuePluginEvent(_finishFuncPtr, 0);
                    _counter = (_counter + 1) % 2;
                    break;
                }
        }
        
    }
}
