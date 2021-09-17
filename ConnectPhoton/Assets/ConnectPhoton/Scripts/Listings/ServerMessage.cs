using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class ServerMessage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI message;
    [SerializeField] float destroyAfterSeconds = 4f;
    float timer;
    public void SetText(string text, Color textColor)
    {
        message.text = text;
        message.color = textColor;
    }
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= destroyAfterSeconds) Destroy(this.gameObject);
    }
}
