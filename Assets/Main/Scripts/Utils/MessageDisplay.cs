using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageDisplay : MonoBehaviour
{
    public static MessageDisplay Instance;
    
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float messageLifetime = 5f; // Tiempo que dura cada mensaje
    [SerializeField] private int maxMessages = 5;

    // Ahora guardamos también el color del mensaje
    private List<MessageEntry> messages = new List<MessageEntry>();

    private class MessageEntry
    {
        public string text;
        public Color color;
        public float timeRemaining;

        public MessageEntry(string text, Color color, float duration)
        {
            this.text = text;
            this.color = color;
            this.timeRemaining = duration;
        }
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (messageText != null)
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
        {
            UpdateUIText();
        }

    }

    // Nuevo AddMessage: acepta string y Color
    public void AddMessageWithColor(string message, Color color)
    {
        if (messages.Count >= maxMessages)
            messages.RemoveAt(0);

        messages.Add(new MessageEntry(message, color, messageLifetime));
        UpdateUIText();
    }

    // Mantener compatibilidad: color blanco por defecto
    public void AddMessage(string message)
    {
        AddMessageWithColor(message, Color.white);
    }

    private void UpdateUIText()
    {
        // Construimos con etiquetas <color>
        List<string> lines = new List<string>();
        foreach (var m in messages)
        {
            string hex = ColorUtility.ToHtmlStringRGB(m.color);
            lines.Add($"<color=#{hex}>{m.text}</color>");
        }
        messageText.text = string.Join("\n", lines);
    }
}