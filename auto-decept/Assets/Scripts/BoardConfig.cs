using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/BoardConfig")]
public class BoardConfig : ScriptableObject
{
    [System.Serializable]
    public struct Layout
    {
        public int rows;
        public int cols;
        public int CellCount()
        {
            return rows * cols;
        }
    }

    [Header("Available Layouts (order matters)")]
    public Layout[] layouts = new Layout[]
    {
    new Layout{ rows = 2, cols = 2},
    new Layout{ rows = 3, cols = 4},
    new Layout{ rows = 4, cols = 4},
    new Layout{ rows = 5, cols = 6},
    };

    [Space]
    [SerializeField] private int _currentIndex = 0;
    [SerializeField] private bool wraparound = true;

    public int CurrentIndex
    {
        get { return _currentIndex; }
    }

    public Layout Current
    {
        get
        {
            if (layouts == null || layouts.Length == 0)
            {
                return default;
            }
            int safe = Mathf.Clamp(_currentIndex, 0, layouts.Length - 1);
            return layouts[safe];
        }
    }

    public void SetIndex(int i)
    {
        if (layouts == null || layouts.Length == 0)
        {
            _currentIndex = 0;
            return;
        }

        if (wraparound)
        {
            int n = layouts.Length;
            _currentIndex = ((i % n) + n) % n; // mod even for negatives
        }
        else
        {
            if (i < 0 || i >= layouts.Length) i = 0; // able to predict fallback
            _currentIndex = i;
        }
    }

    public void Next()
    {
        SetIndex(_currentIndex + 1);
    }

    public void Prev()
    {
        SetIndex(_currentIndex - 1);
    }

    [ContextMenu("Cycle -> Next")]
    private void CtxNext()
    {
        Next();
    }

    [ContextMenu("Cycle -> Prev")]
    private void CtxPrev()
    {
        Prev();
    }

    private void OnValidate()
    {
        if (layouts == null || layouts.Length == 0)
        {
            _currentIndex = 0;
            return;
        }

        // reusing SetIndex logic i.e classic call
        SetIndex(_currentIndex);

        //reminder: if support only in pairs, warn when odd cell counts
        // if(Current.CellCount() % 2 != 0) Debug.LogWarning("Odd cell count in BoardConfig.");


    }
}



