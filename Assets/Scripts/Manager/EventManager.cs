using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    #region Singleton Pattern
     
    public static EventManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #endregion

    #region UnityEvents

    public UnityAction<Vector2> OnMovePerformed;

    public UnityAction OnJumpPressed;

    public UnityAction<Vector2> OnMousePerformed;

    #endregion
}
