using Structure.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Other.Interactions
{
    public class PlaySoundOnButtonPress : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private AudioClip _clip;
        [SerializeField] private Vector2 _pitch;

        private void PlaySound() => AudioManager.Instance.Play(_clip, _pitch);

        private void OnEnable() => _button.onClick.AddListener(PlaySound);
        private void OnDisable() => _button.onClick.RemoveListener(PlaySound);
    }
}