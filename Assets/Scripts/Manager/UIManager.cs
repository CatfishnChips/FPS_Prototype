using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    #region Singleton Pattern
     
    public static UIManager Instance;

    private void SingletonPattern() {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #endregion

    [SerializeField] private TextMeshProUGUI _velocityText;
    [SerializeField] private Transform _player;
    private Vector3 _previousLocation;
    private float _velocityInfo;

    private void Awake() {
        SingletonPattern();
        
        _player = FindObjectOfType<PlayerManager>().transform;
        _previousLocation = _player.position;
    }

    private void Start() {
        StartCoroutine(RefreshVelocityInfo());
    }

    private void OnDisable() {
        StopAllCoroutines();
    }

    public void SetVelocityInfo(float value) {
        _velocityInfo = value;
    }

    private IEnumerator RefreshVelocityInfo() {
        while (true) {
            yield return new WaitForSecondsRealtime(0.05f);
            _velocityText.SetText(((int)_velocityInfo).ToString());  
        }
    }
}
