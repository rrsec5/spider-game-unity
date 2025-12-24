using UnityEngine;
using UnityEngine.UI;

public class WinTrigger : MonoBehaviour
{
    public GameObject winPanel; // Ссылка на панель с сообщением
    public Button okButton;    // Ссылка на кнопку OK

    void Start()
    {
        // Скрываем окно в начале
        winPanel.SetActive(false);

        // Добавляем слушатель на кнопку OK
        okButton.onClick.AddListener(ExitGame);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, что игрок дошел до определенного места
        if (collision.CompareTag("Player"))
        {
            winPanel.SetActive(true); // Показываем окно
            Time.timeScale = 0; // Останавливаем игру
        }
    }

    private void ExitGame()
    {
        Time.timeScale = 1; // Возвращаем игру к нормальному состоянию (если нужно)
        Application.Quit(); // Выход из игры
    }
}
