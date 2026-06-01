using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;

public class SettingsMenuManager : MonoBehaviour
{
    const string SoundKey = "settings_sound";
    const string MusicKey = "settings_music";
    const string SensitivityKey = "settings_sensitivity";
    const string MovementKey = "settings_movement";

    public enum MovementMode
    {
        Teleport = 0,
        Smooth = 1
    }

    [Header("UI")]
    [SerializeField] Slider soundSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] Toggle teleportToggle;
    [SerializeField] Toggle smoothMoveToggle;

    [Header("Audio")]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] string soundVolumeParameter = "SFXVolume";
    [SerializeField] string musicVolumeParameter = "MusicVolume";
    [SerializeField] AudioSource musicSource;

    [Header("Locomotion")]
    [SerializeField] ContinuousMoveProvider smoothMoveProvider;
    [SerializeField] ContinuousTurnProvider turnProvider;
    [SerializeField] Behaviour[] teleportBehaviours;
    [SerializeField] GameObject[] teleportObjects;
    [SerializeField] Behaviour[] smoothMoveBehaviours;
    [SerializeField] GameObject[] smoothMoveObjects;

    [Header("Sensitivity")]
    [SerializeField] float minTurnSpeed = 30f;
    [SerializeField] float maxTurnSpeed = 150f;
    [SerializeField] float minMoveSpeed = 0.75f;
    [SerializeField] float maxMoveSpeed = 2.5f;

    [Header("Defaults")]
    [SerializeField, Range(0f, 1f)] float defaultSound = 0.8f;
    [SerializeField, Range(0f, 1f)] float defaultMusic = 0.6f;
    [SerializeField, Range(0f, 1f)] float defaultSensitivity = 0.5f;
    [SerializeField] MovementMode defaultMovementMode = MovementMode.Teleport;

    MovementMode currentMovementMode;

    void Awake()
    {
        SetupSlider(soundSlider);
        SetupSlider(musicSlider);
        SetupSlider(sensitivitySlider);
    }

    void OnEnable()
    {
        LoadSettings();
    }

    public void OnSoundChanged(float value)
    {
        ApplySound(value);
    }

    public void OnMusicChanged(float value)
    {
        ApplyMusic(value);
    }

    public void OnSensitivityChanged(float value)
    {
        ApplySensitivity(value);
    }

    public void SelectTeleport()
    {
        SetMovementMode(MovementMode.Teleport);
    }

    public void SelectSmoothMove()
    {
        SetMovementMode(MovementMode.Smooth);
    }

    public void OnTeleportToggleChanged(bool isOn)
    {
        if (isOn)
            SetMovementMode(MovementMode.Teleport);
    }

    public void OnSmoothMoveToggleChanged(bool isOn)
    {
        if (isOn)
            SetMovementMode(MovementMode.Smooth);
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetFloat(SoundKey, soundSlider != null ? soundSlider.value : defaultSound);
        PlayerPrefs.SetFloat(MusicKey, musicSlider != null ? musicSlider.value : defaultMusic);
        PlayerPrefs.SetFloat(SensitivityKey, sensitivitySlider != null ? sensitivitySlider.value : defaultSensitivity);
        PlayerPrefs.SetInt(MovementKey, (int)currentMovementMode);
        PlayerPrefs.Save();
    }

    public void ResetSettings()
    {
        ApplyAll(defaultSound, defaultMusic, defaultSensitivity, defaultMovementMode, true);
        SaveSettings();
    }

    public void LoadSettings()
    {
        var sound = PlayerPrefs.GetFloat(SoundKey, defaultSound);
        var music = PlayerPrefs.GetFloat(MusicKey, defaultMusic);
        var sensitivity = PlayerPrefs.GetFloat(SensitivityKey, defaultSensitivity);
        var movement = (MovementMode)PlayerPrefs.GetInt(MovementKey, (int)defaultMovementMode);

        ApplyAll(sound, music, sensitivity, movement, true);
    }

    void ApplyAll(float sound, float music, float sensitivity, MovementMode movement, bool updateUi)
    {
        if (updateUi)
        {
            SetSliderWithoutNotify(soundSlider, sound);
            SetSliderWithoutNotify(musicSlider, music);
            SetSliderWithoutNotify(sensitivitySlider, sensitivity);
        }

        ApplySound(sound);
        ApplyMusic(music);
        ApplySensitivity(sensitivity);
        SetMovementMode(movement);
    }

    void ApplySound(float value)
    {
        var clamped = Mathf.Clamp01(value);

        if (audioMixer != null && !string.IsNullOrWhiteSpace(soundVolumeParameter))
            audioMixer.SetFloat(soundVolumeParameter, SliderToDecibel(clamped));
        else
            AudioListener.volume = clamped;
    }

    void ApplyMusic(float value)
    {
        var clamped = Mathf.Clamp01(value);

        if (audioMixer != null && !string.IsNullOrWhiteSpace(musicVolumeParameter))
            audioMixer.SetFloat(musicVolumeParameter, SliderToDecibel(clamped));

        if (musicSource != null)
            musicSource.volume = clamped;
    }

    void ApplySensitivity(float value)
    {
        var clamped = Mathf.Clamp01(value);

        if (turnProvider != null)
            turnProvider.turnSpeed = Mathf.Lerp(minTurnSpeed, maxTurnSpeed, clamped);

        if (smoothMoveProvider != null)
            smoothMoveProvider.moveSpeed = Mathf.Lerp(minMoveSpeed, maxMoveSpeed, clamped);
    }

    void SetMovementMode(MovementMode movementMode)
    {
        currentMovementMode = movementMode;
        var teleportEnabled = movementMode == MovementMode.Teleport;
        var smoothEnabled = movementMode == MovementMode.Smooth;

        SetToggleWithoutNotify(teleportToggle, teleportEnabled);
        SetToggleWithoutNotify(smoothMoveToggle, smoothEnabled);

        SetBehavioursEnabled(teleportBehaviours, teleportEnabled);
        SetObjectsEnabled(teleportObjects, teleportEnabled);
        SetBehavioursEnabled(smoothMoveBehaviours, smoothEnabled);
        SetObjectsEnabled(smoothMoveObjects, smoothEnabled);
    }

    static void SetupSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    static void SetSliderWithoutNotify(Slider slider, float value)
    {
        if (slider != null)
            slider.SetValueWithoutNotify(Mathf.Clamp01(value));
    }

    static void SetToggleWithoutNotify(Toggle toggle, bool value)
    {
        if (toggle != null)
            toggle.SetIsOnWithoutNotify(value);
    }

    static void SetBehavioursEnabled(Behaviour[] behaviours, bool enabled)
    {
        if (behaviours == null)
            return;

        foreach (var behaviour in behaviours)
        {
            if (behaviour != null)
                behaviour.enabled = enabled;
        }
    }

    static void SetObjectsEnabled(GameObject[] objects, bool active)
    {
        if (objects == null)
            return;

        foreach (var item in objects)
        {
            if (item != null)
                item.SetActive(active);
        }
    }

    static float SliderToDecibel(float value)
    {
        return Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f;
    }
}
