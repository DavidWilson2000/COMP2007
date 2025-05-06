using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    public KeyCode rotateLeftKey = KeyCode.Q;
    public KeyCode rotateRightKey = KeyCode.E;
    public KeyCode slideKey = KeyCode.S;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode toggleViewKey = KeyCode.K;

    private string keyToRebind = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadKeys();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartRebindRotateLeft()
    {
        keyToRebind = "RotateLeft";
        Debug.Log("Press a new key for Rotate Left...");
    }

    public void StartRebindRotateRight()
    {
        keyToRebind = "RotateRight";
        Debug.Log("Press a new key for Rotate Right...");
    }

    public void StartRebindSlide()
    {
        keyToRebind = "Slide";
        Debug.Log("Press a new key for Slide...");
    }

    public void StartRebindJump()
    {
        keyToRebind = "Jump";
        Debug.Log("Press a new key for Jump...");
    }

    public void StartRebindToggleView()
    {
        keyToRebind = "ToggleView";
        Debug.Log("Press a new key for Toggle View...");
    }

    private void Update()
    {
        if (!string.IsNullOrEmpty(keyToRebind))
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    SetKey(keyToRebind, keyCode);
                    Debug.Log($"Bound {keyToRebind} to {keyCode}");
                    keyToRebind = "";
                    break;
                }
            }
        }
    }

    public void SetKey(string keyName, KeyCode newKey)
    {
        switch (keyName)
        {
            case "RotateLeft":
                rotateLeftKey = newKey;
                PlayerPrefs.SetString("RotateLeft", newKey.ToString());
                break;
            case "RotateRight":
                rotateRightKey = newKey;
                PlayerPrefs.SetString("RotateRight", newKey.ToString());
                break;
            case "Slide":
                slideKey = newKey;
                PlayerPrefs.SetString("Slide", newKey.ToString());
                break;
            case "Jump":
                jumpKey = newKey;
                PlayerPrefs.SetString("Jump", newKey.ToString());
                break;
            case "ToggleView":
                toggleViewKey = newKey;
                PlayerPrefs.SetString("ToggleView", newKey.ToString());
                break;
        }

        PlayerPrefs.Save();
    }

    private void LoadKeys()
    {
        rotateLeftKey = GetSavedKey("RotateLeft", KeyCode.Q);
        rotateRightKey = GetSavedKey("RotateRight", KeyCode.E);
        slideKey = GetSavedKey("Slide", KeyCode.S);
        jumpKey = GetSavedKey("Jump", KeyCode.Space);
        toggleViewKey = GetSavedKey("ToggleView", KeyCode.K);
    }

    private KeyCode GetSavedKey(string key, KeyCode defaultKey)
    {
        string keyString = PlayerPrefs.GetString(key, defaultKey.ToString());
        if (System.Enum.TryParse(keyString, out KeyCode loadedKey))
        {
            return loadedKey;
        }
        return defaultKey;
    }
}
