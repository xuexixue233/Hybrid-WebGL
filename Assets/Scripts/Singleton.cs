using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    
    private static object _instLock = new object();

    static bool _disposing = false;
    
    public T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
            }

            if (!_disposing)
                return _instance;
            else
                return null;
        }

        set
        {
            lock (_instLock) 
            {
                if (_instance == null) 
                    _instance = value;
                else if
                    (_instance ==
                     this) 
                    _instance = value;
            }
        }
    }


    public virtual void Awake()
    {
        if (Instance == null)
            Instance = _instance;
        else if (Instance != this) //Check to make sure there isn't already an instance of this class.
        {
            //If there is already an instance of this class, destroy the object we just created.
            Debug.LogWarning("Attempted to spawn more then one singleton object. destroying new instance of " +
                             gameObject.ToString() +
                             "\nIf you would like more than one instance, ensure that the class you have written does not Inherit from a Singleton class.");
            Dispose();
        }
    }

    // Proper cleanup of a Singleton instance includes nullifying any references to it in order to allow Garbage Collection
    protected virtual void Dispose()
    {
        _disposing = true;
        Instance = null;

        Destroy(gameObject);
    }

    // In case it is destroyed in an improper manner
    public virtual void OnDestroy()
    {
        if (!_disposing)
            _disposing = true;

        if (Instance != null)
            Instance = null;
    }
}