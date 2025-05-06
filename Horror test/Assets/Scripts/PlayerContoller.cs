using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float forwardSpeed = 5f;
    public float maxSpeed = 15f;
    public float speedIncreaseRate = 0.1f;
    public float slideSpeed = 8f;
    public float strafeSpeed = 10f;
    public float mouseSensitivity = 0.1f;
    public float strafeLimit = 3f;
    public float turnSpeed = 360f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float slideDuration = 1f;
    public float jumpDelay = 0.1f;
    public ScoreboardManager scoreboardManager;
    public string playerName = "Player";


    [Header("Camera Settings")]
    public Transform playerCamera;
    private Vector3 originalCameraLocalPosition;
    private Vector3 slideCameraLocalPosition;
    private float cameraSmoothSpeed = 8f;

    [Header("Score Settings")]
    public TMP_Text scoreText;
    public int pointsPerSecond = 10;
    public int wallHitPenalty = 50;

    private CharacterController characterController;
    private Animator animator;

    private Vector3 currentForward;
    private float verticalVelocity = 0f;
    private float targetStrafe = 0f;
    private Quaternion targetRotation;
    private bool isTurning = false;

    private bool isSliding = false;
    private float slideTimer = 0f;

    private bool jumpRequested = false;
    private float jumpRequestTimer = 0f;

    private float originalHeight;
    private Vector3 originalCenter;
    private float slideHeightMultiplier = 0.001f;

    private float survivalTime = 0f;
    private int score = 0;
    private float scoreFloat = 0f;

    private bool isPaused = false; // 🆕 added pause control

    [Header("Lose Conditions")]
    public float wallContactThreshold = 3f; // Time in seconds before losing
    public GameObject loseScreenUI;         // Assign a UI panel with "You Lose" text

    private float wallContactTime = 0f;
    private bool hasLost = false;

    private bool isTouchingWall = false;
    private float wallContactTimer = 0f;


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        Cursor.lockState = CursorLockMode.Locked;

        currentForward = transform.forward;
        targetRotation = transform.rotation;

        originalHeight = characterController.height;
        originalCenter = characterController.center;

        if (playerCamera != null)
        {
            originalCameraLocalPosition = playerCamera.localPosition;
            slideCameraLocalPosition = originalCameraLocalPosition + new Vector3(0, -0.5f, 0);
        }

        score = 0;
    }

    void Update()
    {
        if (isPaused) return; // 🛑 Prevent moving if paused!

        survivalTime += Time.deltaTime;
        scoreFloat += pointsPerSecond * Time.deltaTime;
        score = Mathf.FloorToInt(scoreFloat);

        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();

        HandleSpeedScaling();
        HandleTurnInput();
        SmoothTurn();
        HandleSlideInput();
        SmoothCameraSlide();

        bool isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;

        if (!characterController.isGrounded)
        {
            wallContactTime = 0f;
        }

        if (Input.GetKeyDown(InputManager.Instance.jumpKey) && isGrounded && !isSliding && !jumpRequested)
        {
            jumpRequested = true;
            jumpRequestTimer = jumpDelay;
            animator.SetTrigger("Jump");
        }

        if (jumpRequested)
        {
            jumpRequestTimer -= Time.deltaTime;
            if (jumpRequestTimer <= 0f)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpRequested = false;
            }
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 move = currentForward * forwardSpeed;

        if (!isTurning)
        {
            float mouseX = Input.GetAxis("Mouse X");
            targetStrafe += mouseX * mouseSensitivity * strafeSpeed;
            targetStrafe = Mathf.Clamp(targetStrafe, -strafeLimit, strafeLimit);
        }

        Vector3 currentRight = Vector3.Cross(Vector3.up, currentForward).normalized;
        move += currentRight * targetStrafe * strafeSpeed;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);

        animator.SetFloat("Speed", isSliding ? slideSpeed : forwardSpeed);
        animator.speed = Mathf.Lerp(1f, 1.5f, (forwardSpeed - 5f) / (maxSpeed - 5f));

        if (isSliding)
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0f)
            {
                StopSlide();
            }
        }
        if (isTouchingWall)
        {
            wallContactTimer += Time.deltaTime;

            if (wallContactTimer >= wallContactThreshold && !hasLost)
            {
                TriggerGameOver();
            }
        }
        else
        {
            wallContactTimer = 0f;
        }
        isTouchingWall = false; // Reset every frame — gets re-set in OnControllerColliderHit

    }

    void HandleSpeedScaling()
    {
        forwardSpeed += speedIncreaseRate * Time.deltaTime;
        forwardSpeed = Mathf.Clamp(forwardSpeed, 5f, maxSpeed);
        turnSpeed = Mathf.Lerp(360f, 600f, (forwardSpeed - 5f) / (maxSpeed - 5f));
    }

    void HandleTurnInput()
    {
        if (isTurning) return;

        if (Input.GetKeyDown(InputManager.Instance.rotateLeftKey))
            RotatePlayer(-90f);
        else if (Input.GetKeyDown(InputManager.Instance.rotateRightKey))
            RotatePlayer(90f);
    }

    void RotatePlayer(float angle)
    {
        targetRotation = Quaternion.Euler(0, angle, 0) * transform.rotation;
        isTurning = true;
    }

    void SmoothTurn()
    {
        if (!isTurning) return;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            transform.rotation = targetRotation;
            isTurning = false;
            currentForward = transform.forward;
            targetStrafe = 0f;
        }
    }

    void HandleSlideInput()
    {
        if (Input.GetKeyDown(InputManager.Instance.slideKey) && characterController.isGrounded && !isSliding)
        {
            StartSlide();
        }
    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;

        float newHeight = originalHeight * slideHeightMultiplier;
        characterController.height = newHeight;
        characterController.center = new Vector3(originalCenter.x, newHeight / 2f, originalCenter.z);

        animator.SetTrigger("Slide");
        Debug.Log("🛝 Started sliding!");
    }

    void StopSlide()
    {
        isSliding = false;
        characterController.height = originalHeight;
        characterController.center = originalCenter;
        Debug.Log("🛑 Stopped sliding!");
    }

    public void AddScore(int amount)
    {
        scoreFloat += amount;
        score = Mathf.FloorToInt(scoreFloat);

        if (scoreText != null)
            scoreText.text = "Score: " + score.ToString();
    }

    void SmoothCameraSlide()
    {
        if (playerCamera == null) return;

        Vector3 targetPos = isSliding ? slideCameraLocalPosition : originalCameraLocalPosition;
        playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, targetPos, cameraSmoothSpeed * Time.deltaTime);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Walls"))
        {
            isTouchingWall = true;

            Debug.Log("⚡ Player hit a wall! Lose points!");
            scoreFloat -= wallHitPenalty;
            scoreFloat = Mathf.Max(0, scoreFloat);
            score = Mathf.FloorToInt(scoreFloat);

            if (scoreText != null)
                scoreText.text = "Score: " + score.ToString();
        }
    }



    // 🆕 Called by PauseManager
    public void SetPaused(bool paused)
    {
        isPaused = paused;
    }
    public int GetScore()
    {
        return score;
    }
    void TriggerGameOver()
    {
        hasLost = true;
        SetPaused(true);
        Time.timeScale = 0f;

        if (scoreboardManager != null)
        {
            scoreboardManager.TryAddScore(GetScore(), playerName);
            scoreboardManager.UpdateScoreboardDisplay(); // Optional, if visible
        }

        if (loseScreenUI != null)
        {
            loseScreenUI.SetActive(true);
        }

        Debug.Log("💀 Game Over! You were stuck too long.");
    }


}
