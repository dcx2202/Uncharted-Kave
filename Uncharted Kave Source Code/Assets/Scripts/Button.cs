using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Stores all information needed for a button to work properly
/// </summary>
public class Button : MonoBehaviour
{
    // Drag and drop fields
    [Tooltip("Manager that handles the game logic")]
    public GameManager gameManager;

    [Tooltip("Button codename")]
    public string name;

    // Other variables
    public AudioClip audioClip;
    public AudioSource audioSource;


    /// <summary>
    /// Called when something enters this button's collider
    /// </summary>
    void OnTriggerEnter(Collider col)
    {
        // Send button press to the game state machine. If it isn't handled by the current state then ignore it
        if (!gameManager.TouchButton(name))
            return;

        // If it was handled and if it is a keypad key then give visual feedback
        if (name.Contains("Key"))
        {
            // Enable key renderer
            Renderer renderer = GetComponent<Renderer>();
            renderer.enabled = true;

            // Schedule the key renderer to be disabled
            Invoke("DisableMeshRenderer", 0.5f);
        }

        // If this button has any audio clip assigned to it then play it
        if (audioSource != null && audioClip != null)
            audioSource.PlayOneShot(audioClip, 1f);
    }


    /// <summary>
    /// Disables this button's renderer
    /// </summary>
    private void DisableMeshRenderer()
    {
        // If this button is a keypad key then we can disable its renderer
        if (name.Contains("Key"))
        {
            Renderer renderer = GetComponent<Renderer>();
            renderer.enabled = false;
        }
    }


    // Not implemented
    void Awake()
    {

    }

    // Not implemented
    void Start()
    {

    }

    // Not implemented
    void Update()
    {

    }
}
