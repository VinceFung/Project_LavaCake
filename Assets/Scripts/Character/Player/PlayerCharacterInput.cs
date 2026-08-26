using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerCharacterInput : MonoBehaviour
{
    public Entity entity;
    public Gun gun;
    public Camera mainCamera;
    public CharacterMovement movement;
    public CharacterTargeting targeting;

    public RectTransform Inventory;
    public bool InventoryOpen = false;

    [System.Serializable]
    public class AbilitySlot
    {
        public Ability ability;
        public InputActionReference action; // Reference to the InputAction for this ability
    }
    public AbilitySlot[] abilities;
    [Space(5)]
    public PlayerHealthPotion playerHealthPotionScript;
    public AbilitySlot HealAbility;

    public float dashBufferTime = 0.15f;
    private float dashBufferTimer = 0f;

    // Input Actions
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference dashAction;
    public InputActionReference fireAction;
    public InputActionReference fire2Action;
    public InputActionReference armAction;
    public InputActionReference inventoryAction;
    public InputActionReference interactAction;
    public InputActionReference abilityCursorAction;

    [Header("Ability Cursor UI")]
    public RectTransform abilityCursorCanvas;
    public RectTransform abilityCursorRect;
    public CanvasGroup abilityCursorCanvasGroup;
    public float abilityCursorSpeed = 1000f;
    public float cursorFadeSpeed = 8f; // Speed of cursor fade in/out
    public float timeSlowdownDelay = 0.25f; // Delay before time slowdown kicks in

    [Header("Ability Cursor Glow")]
    public Image highlightCursorGlowImage;
    public Image soulTouchedGlowImage;
    public float cursorHighlightRadius = 2.5f;
    public float glowFadeSpeed = 10f;

    [Header("Detection")]
    public LayerMask corpseLayerMask;
    public LayerMask characterLayerMask;
    
    [Header("Interaction")]
    public float interactionRadius = 3f;
    public TMPro.TextMeshProUGUI interactionPromptText; // UI Text for interaction prompt

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool dashPressed;
    private bool firePressed;
    private bool fire2Pressed;
    private bool armPressed;
    private bool inventoryPressed;
    private bool interactPressed;

    Vector2 lastMousePosition;

    public PlayerInput playerInput;

    private bool abilityCursorActive = false;
    private Vector2 abilityCursorPosition;
    private float abilityCursorActivationTime = 0f;
    private InteractableObject nearestInteractable = null;

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        dashAction.action.Enable();
        fireAction.action.Enable();
        fire2Action.action.Enable();
        armAction.action.Enable();
        inventoryAction.action.Enable();
        interactAction.action.Enable();
        abilityCursorAction.action.Enable();

        dashAction.action.performed += OnDash;
        inventoryAction.action.performed += OnInventory;
        interactAction.action.performed += OnInteract;
        abilityCursorAction.action.performed += OnAbilityCursor;
        abilityCursorAction.action.canceled += OnAbilityCursorRelease;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        dashAction.action.Disable();
        fireAction.action.Disable();
        fire2Action.action.Disable();
        armAction.action.Disable();
        inventoryAction.action.Disable();
        interactAction.action.Disable();
        abilityCursorAction.action.Disable();

        dashAction.action.performed -= OnDash;
        inventoryAction.action.performed -= OnInventory;
        interactAction.action.performed -= OnInteract;
        abilityCursorAction.action.performed -= OnAbilityCursor;
        abilityCursorAction.action.canceled -= OnAbilityCursorRelease;
    }

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = UnitManager.Instance.mainCamera;
        }
    }

    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = UnitManager.Instance.mainCamera;
        }

        moveInput = moveAction.action.ReadValue<Vector2>();
        lookInput = lookAction.action.ReadValue<Vector2>();
        firePressed = fireAction.action.IsPressed();
        fire2Pressed = fire2Action.action.WasPressedThisFrame();
        armPressed = armAction.action.IsPressed();

        if (InventoryOpen || UnitManager.Instance.consoleObject.activeSelf)
        {
            movement.moveInput = Vector3.zero;
        }
        else
        {
            if (!abilityCursorActive)
            {
                gun.IsArming = armPressed;

                if (firePressed && gun.Armed)
                {
                    gun.Shoot();
                }

                if (entity.meleeWeapon != null && !gun.IsArming)
                {
                    if (fireAction.action.WasPressedThisFrame())
                    {
                        entity.meleeWeapon.LightAttack();
                    }

                    if (fire2Action.action.WasPressedThisFrame())
                    {
                        entity.meleeWeapon.HeavyAttack();
                    }
                }
            }

            MoveInput();
            LookInput();
            AbilityInput();
        }

        // Update interaction detection and prompt
        UpdateInteractionDetection();

        // Dash input buffering logic
        if (dashBufferTimer > 0f)
        {
            dashBufferTimer -= Time.deltaTime;
            if (CanDash())
            {
                movement.Dash();
                dashBufferTimer = 0f;
            }
        }

        /*if (InventoryOpen)
        {
            Inventory.localPosition = new Vector3(Inventory.transform.localPosition.x, 0f, Inventory.transform.localPosition.z);
        }
        else
        {
            Inventory.localPosition = new Vector3(Inventory.transform.localPosition.x, 2000f, Inventory.transform.localPosition.z);
        }*/
        Inventory.gameObject.SetActive(InventoryOpen);

        if (abilityCursorActive)
        {
            // Only slow time after the delay to allow for quick taps
            float timeSinceActivation = Time.unscaledTime - abilityCursorActivationTime;
            if (timeSinceActivation >= timeSlowdownDelay)
            {
                Time.timeScale = 0.2f; // Slow time
            }
            
            // Fade in the cursor smoothly
            if (abilityCursorCanvasGroup != null)
            {
                abilityCursorCanvasGroup.alpha = Mathf.Lerp(abilityCursorCanvasGroup.alpha, 1f, Time.unscaledDeltaTime * cursorFadeSpeed);
            }

            Vector2 stickInput = lookAction.action.ReadValue<Vector2>();
            abilityCursorPosition += stickInput * abilityCursorSpeed * Time.unscaledDeltaTime;

            // Clamp to canvas bounds (assuming screen space overlay)
            Vector2 canvasSize = abilityCursorCanvas.sizeDelta;
            abilityCursorPosition.x = Mathf.Clamp(abilityCursorPosition.x, -canvasSize.x / 2f, canvasSize.x / 2f);
            abilityCursorPosition.y = Mathf.Clamp(abilityCursorPosition.y, -canvasSize.y / 2f, canvasSize.y / 2f);

            abilityCursorRect.anchoredPosition = abilityCursorPosition;

            // --- GLOW LOGIC START ---
            float highlightTargetAlpha = 0f;
            float soulTouchedTargetAlpha = 0f;

            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                abilityCursorCanvas.GetComponentInParent<Canvas>().worldCamera,
                abilityCursorRect.position
            );
            Ray cameraRay = mainCamera.ScreenPointToRay(screenPoint);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));
            float rayLength;
            if (groundPlane.Raycast(cameraRay, out rayLength))
            {
                Vector3 worldPoint = cameraRay.GetPoint(rayLength);
                Collider[] hits = Physics.OverlapSphere(worldPoint, cursorHighlightRadius, corpseLayerMask);
                foreach (Collider hit in hits)
                {
                    Entity hitEntity = hit.GetComponent<Entity>();
                    if (hitEntity != null && hitEntity.EntityType == Entity.EntityTypes.Corpse)
                    {
                        bool isSoulTouched = false;
                        foreach (var debuff in hitEntity.activeDebuffs)
                        {
                            if (debuff.DebuffName == "Soul Touched")
                            {
                                isSoulTouched = true;
                                break;
                            }
                        }
                        if (isSoulTouched)
                        {
                            soulTouchedTargetAlpha = 1f;
                        }
                        else
                        {
                            highlightTargetAlpha = 1f;
                        }
                        break;
                    }
                }
            }

            // Lerp alpha for each image
            if (highlightCursorGlowImage != null)
            {
                Color c = highlightCursorGlowImage.color;
                c.a = Mathf.Lerp(c.a, highlightTargetAlpha, Time.unscaledDeltaTime * glowFadeSpeed);
                highlightCursorGlowImage.color = c;
            }
            if (soulTouchedGlowImage != null)
            {
                Color c = soulTouchedGlowImage.color;
                c.a = Mathf.Lerp(c.a, soulTouchedTargetAlpha, Time.unscaledDeltaTime * glowFadeSpeed);
                soulTouchedGlowImage.color = c;
            }
            // --- GLOW LOGIC END ---
        }
        else
        {
            Time.timeScale = 1f;
            
            // Fade out the cursor smoothly
            if (abilityCursorCanvasGroup != null)
            {
                abilityCursorCanvasGroup.alpha = Mathf.Lerp(abilityCursorCanvasGroup.alpha, 0f, Time.unscaledDeltaTime * cursorFadeSpeed * 2f);
            }
            
            // Fade out both glows
            if (highlightCursorGlowImage != null)
            {
                Color c = highlightCursorGlowImage.color;
                c.a = Mathf.Lerp(c.a, 0f, Time.unscaledDeltaTime * glowFadeSpeed);
                highlightCursorGlowImage.color = c;
            }
            if (soulTouchedGlowImage != null)
            {
                Color c = soulTouchedGlowImage.color;
                c.a = Mathf.Lerp(c.a, 0f, Time.unscaledDeltaTime * glowFadeSpeed);
                soulTouchedGlowImage.color = c;
            }
        }
    }

    void MoveInput()
    {
        movement.moveInput = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
    }

    void OnDash(InputAction.CallbackContext ctx)
    {
        if (CanDash())
        {
            movement.Dash();
            dashBufferTimer = 0f;
        }
        else
        {
            dashBufferTimer = dashBufferTime;
        }
    }

    bool CanDash()
    {
        return movement.IsGrounded && movement.dashDur <= 0f && entity.Stamina >= movement.dashStaminaCost;
    }

    void LookInput()
    {
        if (playerInput.currentControlScheme == "Gamepad")
        {
            if (lookInput.magnitude > 0.1f)
            {
                Vector3 playerPos = entity.transform.position + Vector3.up * 0.8f;
                Vector3 lookDir = new Vector3(lookInput.x, 0f, lookInput.y).normalized;

                // Aim assist when gun is armed
                if (gun.IsArming && gun.Armed)
                {
                    float assistRadius = 20f;
                    float assistAngle = 20f;
                    Entity bestTarget = null;
                    float bestDot = -1f;

                    Collider[] hits = Physics.OverlapSphere(playerPos, assistRadius, characterLayerMask);
                    foreach (Collider hit in hits)
                    {
                        Entity targetEntity = hit.GetComponent<Entity>();
                        if (targetEntity != null && targetEntity != entity && targetEntity.EntityType == Entity.EntityTypes.Character && targetEntity.Team != entity.Team)
                        {
                            // Only consider targets on screen
                            Vector3 screenPoint = mainCamera.WorldToViewportPoint(targetEntity.transform.position + Vector3.up * 0.8f);
                            if (screenPoint.z > 0f && screenPoint.x >= 0f && screenPoint.x <= 1f && screenPoint.y >= 0f && screenPoint.y <= 1f)
                            {
                                // Only consider targets in line of sight
                                Vector3 targetPoint = targetEntity.transform.position + Vector3.up * 0.8f;
                                Vector3 dir = targetPoint - playerPos;
                                float dist = dir.magnitude;
                                RaycastHit hitInfo;
                                bool hasLOS = !Physics.Raycast(playerPos, dir.normalized, out hitInfo, dist, ~characterLayerMask, QueryTriggerInteraction.Ignore)
                                    || hitInfo.collider.GetComponent<Entity>() == targetEntity;

                                if (hasLOS)
                                {
                                    Vector3 toTarget = (targetEntity.transform.position - playerPos).normalized;
                                    float dot = Vector3.Dot(lookDir, toTarget);
                                    float angle = Vector3.Angle(lookDir, toTarget);
                                    if (angle < assistAngle && dot > bestDot)
                                    {
                                        bestDot = dot;
                                        bestTarget = targetEntity;
                                    }
                                }
                            }
                        }
                    }

                    if (bestTarget != null)
                    {
                        targeting.pointToLook = bestTarget.transform.position + Vector3.up * 0.8f;
                        return;
                    }
                }

                // Default controller aiming
                if (lookDir.sqrMagnitude > 0.01f)
                {
                    targeting.pointToLook = playerPos + lookDir * 10f;
                }
            }
            // If lookInput is neutral, do NOT update targeting.pointToLook at all!
        }
        else if (playerInput.currentControlScheme == "Keyboard&Mouse")
        {
            // Mouse aiming
            Ray cameraRay = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, 0.8f, 0f));
            float rayLength;
            if (groundPlane.Raycast(cameraRay, out rayLength)) targeting.pointToLook = cameraRay.GetPoint(rayLength);
            lastMousePosition = Input.mousePosition;
        }
    }

    void AbilityInput()
    {
        for (int i = 0; i < abilities.Length; i++)
        {
            AbilitySlot item = abilities[i];
            if (item.action != null && item.action.action.WasPressedThisFrame())
            {
                // For the first ability (index 0) on gamepad, require ability cursor to be active
                if (i == 0 && playerInput.currentControlScheme == "Gamepad" && !abilityCursorActive)
                {
                    continue; // Skip casting this ability
                }

                Ray cameraRay;
                if (playerInput.currentControlScheme == "Gamepad" && abilityCursorActive)
                {
                    // Use ability cursor position for raycast
                    Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                        abilityCursorCanvas.GetComponentInParent<Canvas>().worldCamera,
                        abilityCursorRect.position
                    );
                    cameraRay = mainCamera.ScreenPointToRay(screenPoint);
                }
                else // Keyboard & Mouse
                {
                    cameraRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                }

                Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));
                float rayLength;
                if (groundPlane.Raycast(cameraRay, out rayLength))
                {
                    item.ability.CastAbility(cameraRay.GetPoint(rayLength));
                }
            }
        }

        if (HealAbility.action != null && HealAbility.action.action.WasPressedThisFrame() && playerHealthPotionScript.PotionCount > 0)
        {
            Ray cameraRay;
            if (playerInput.currentControlScheme == "Gamepad" && abilityCursorActive)
            {
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(
                    abilityCursorCanvas.GetComponentInParent<Canvas>().worldCamera,
                    abilityCursorRect.position
                );
                cameraRay = mainCamera.ScreenPointToRay(screenPoint);
            }
            else // Keyboard & Mouse
            {
                cameraRay = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            }

            Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, 0f, 0f));
            float rayLength;
            if (groundPlane.Raycast(cameraRay, out rayLength))
            {
                HealAbility.ability.CastAbility(cameraRay.GetPoint(rayLength));
            }
        }
    }

    public void OnInventory(InputAction.CallbackContext ctx)
    {
        if (!UnitManager.Instance.consoleObject.activeSelf)
        {
            InventoryOpen = !InventoryOpen;
        }
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (InventoryOpen == false && nearestInteractable != null)
        {
            nearestInteractable.OnInteract(entity);
            // Force immediate update after interaction
            nearestInteractable = null;
            UpdateInteractionPrompt();
        }
    }

    private void UpdateInteractionDetection()
    {
        // Clear reference if current interactable was destroyed or disabled
        if (nearestInteractable != null && (nearestInteractable == null || !nearestInteractable.gameObject.activeInHierarchy || !nearestInteractable.enabled))
        {
            nearestInteractable = null;
            UpdateInteractionPrompt();
            return;
        }

        Vector3 playerPosition = entity.transform.position;
        Collider[] hits = Physics.OverlapSphere(playerPosition, interactionRadius);
        
        InteractableObject closestInteractable = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider hit in hits)
        {
            InteractableObject interactable = hit.GetComponent<InteractableObject>();
            if (interactable != null && interactable.enabled && interactable.CanInteract())
            {
                // Use horizontal distance for interaction
                Vector3 horizontalDistance = hit.transform.position - playerPosition;
                horizontalDistance.y = 0;
                float distance = horizontalDistance.magnitude;
                
                // Use the interactable's own radius if it's smaller than our detection radius
                float effectiveRadius = Mathf.Min(interactionRadius, interactable.interactionRadius);
                
                if (distance <= effectiveRadius && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }
        
        // Always update - this will clear nearestInteractable if no valid objects found
        if (closestInteractable != nearestInteractable)
        {
            nearestInteractable = closestInteractable;
            UpdateInteractionPrompt();
        }
    }
    
    private void UpdateInteractionPrompt()
    {
        if (interactionPromptText != null)
        {
            // Show prompt only if we have a valid interactable and inventory is not open
            if (nearestInteractable != null && !InventoryOpen)
            {
                interactionPromptText.text = nearestInteractable.interactionPrompt;
                interactionPromptText.gameObject.SetActive(true);
            }
            else
            {
                // No valid interactable or inventory is open - hide prompt
                interactionPromptText.gameObject.SetActive(false);
            }
        }
    }

    public void OnAbilityCursor(InputAction.CallbackContext ctx)
    {
        if (playerInput.currentControlScheme == "Gamepad")
        {
            abilityCursorActive = true;
            abilityCursorPosition = Vector2.zero;
            abilityCursorRect.anchoredPosition = abilityCursorPosition;
            abilityCursorActivationTime = Time.unscaledTime; // Record when it was activated
            
            // Start cursor fade from 0 for smooth appearance
            if (abilityCursorCanvasGroup != null)
            {
                abilityCursorCanvasGroup.alpha = 0f;
            }
        }
    }

    public void OnAbilityCursorRelease(InputAction.CallbackContext ctx)
    {
        if (abilityCursorActive && playerInput.currentControlScheme == "Gamepad")
        {
            // Check if this was a quick tap (less than 0.3 seconds)
            float holdDuration = Time.unscaledTime - abilityCursorActivationTime;
            if (holdDuration < 0.3f)
            {
                // Quick tap - cast first ability on nearest soul-touched corpse around player
                CastFirstAbilityOnNearestSoulTouchedCorpse();
            }
        }
        abilityCursorActive = false;
    }
    
    private void CastFirstAbilityOnNearestSoulTouchedCorpse()
    {
        if (abilities.Length == 0 || abilities[0].ability == null) return;
        
        // Find all corpses within detection radius around the PLAYER, not the cursor
        Vector3 playerPosition = entity.transform.position;
        Collider[] hits = Physics.OverlapSphere(playerPosition, 20f, corpseLayerMask);
        Entity nearestSoulTouchedCorpse = null;
        float nearestDistance = float.MaxValue;
        
        foreach (Collider hit in hits)
        {
            Entity hitEntity = hit.GetComponent<Entity>();
            if (hitEntity != null && hitEntity.EntityType == Entity.EntityTypes.Corpse)
            {
                // Check if this corpse has Soul Touched debuff
                bool isSoulTouched = false;
                foreach (var debuff in hitEntity.activeDebuffs)
                {
                    if (debuff.DebuffName == "Soul Touched")
                    {
                        isSoulTouched = true;
                        break;
                    }
                }
                
                if (isSoulTouched)
                {
                    // Use horizontal distance for soul-touched corpse detection
                    Vector3 horizontalDistance = hitEntity.transform.position - playerPosition;
                    horizontalDistance.y = 0;
                    float distance = horizontalDistance.magnitude;
                    
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestSoulTouchedCorpse = hitEntity;
                    }
                }
            }
        }
        
        // Cast ability on the nearest soul-touched corpse if found
        if (nearestSoulTouchedCorpse != null)
        {
            abilities[0].ability.CastAbility(nearestSoulTouchedCorpse.transform.position);
        }
    }
}
