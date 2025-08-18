using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageDisplay : MonoBehaviour
{
    public static MessageDisplay Instance;

    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float messageLifetime = 5f; // Tiempo que dura cada mensaje
    [SerializeField] private int maxMessages = 5;

    private List<MessageEntry> messages = new List<MessageEntry>(); // lista de clase mensajes

    private class MessageEntry // me guardo la duracion del texto y lo que dice en esta clase
    {
        public string text;
        public float timeRemaining;

        public MessageEntry(string text, float duration)
        {
            this.text = text;
            this.timeRemaining = duration;
        }
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        messageText.text = "";
    }

    void Update()
    {
        bool changed = false;

        for (int i = messages.Count - 1; i >= 0; i--)
        {
            messages[i].timeRemaining -= Time.deltaTime;

            if (messages[i].timeRemaining <= 0)
            {
                messages.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
            UpdateUIText();
    }

    public void AddMessage(string message)
    {
        if (messages.Count >= maxMessages)
        {
            messages.RemoveAt(0);
        }

        messages.Add(new MessageEntry(message, messageLifetime));
        UpdateUIText();
    }

    private void UpdateUIText()
    {
        messageText.text = string.Join("\n", messages.ConvertAll(m => m.text));
    }
}