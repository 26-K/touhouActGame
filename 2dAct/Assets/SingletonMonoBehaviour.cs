using System;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
where T : SingletonMonoBehaviour<T>
{
    public static T Inst { get; private set; }
    [Obsolete("Use 'Inst' Instead.")] public static T Ins => Inst;

#if UNITY_EDITOR
    /// <summary>
    /// Play中でない時でもアクセスできます。
    /// 
    /// ただしBuild時に消滅するので #if UNITY_EDITOR で括ってください。
    /// </summary>
    public static T UNITY_EDITOR
    {
        get
        {
            if (Inst != null)
            {
                return Inst;
            }
            else
            {
                Inst = GameObject.FindObjectOfType<T>();
                return Inst;
            }
        }
    }
#endif

    void Awake()
    {
        if (Inst == null)
        {
            Inst = this as T;
            UnityAwake();
            return;
        }
        else if (Inst == this)
        {
            Assert.IsTrue(false, "SingletonMonoBehaviour:既に作られています。同じコンポーネントが２つあるかも？" + this);
            return;
        }
        else
        {
            Assert.IsTrue(false, "SingletonMonoBehaviour:同じクラスのインスタンスが複数あります。:" + this);
            return;
        }
    }

    void OnDestroy()
    {
        // Debug.Log($"  終了処理:class={this.name}, gameObj ={gameObject.name} (SingletonMonoBehaviour.OnDestroy)");
        OnUnityDestroy();
        Inst = null;
    }


    protected abstract void UnityAwake();
    protected virtual void OnUnityDestroy() { }
}
