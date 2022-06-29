using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GUISceneManager : MonoBehaviour
{
    public List<GameObject> listGUIScene;
    public enum E_GUI_STATE { TITLE, THEEND, GAMEOVER, MISSION, PLAY}
    public E_GUI_STATE curGUIState;    
    public ItemInventoryObject itemInventoryObject;
    public GameObject inventory;

    void Awake()
    {     
        SetGUIStatus(curGUIState);            
    }

    private void Update()
    {
        UpdateGUIStatus();
    }

    public void ShowScene(E_GUI_STATE state)
    {
        for (int i = 0; i < listGUIScene.Count; i++)
        {
            if (i == (int)state)
            {
                listGUIScene[i].SetActive(true);
            }
            else
            {
                listGUIScene[i].SetActive(false);
            }
        }
    }

    public void SetGUIStatus(E_GUI_STATE state)
    {
        switch (state)
        {
            case E_GUI_STATE.TITLE:
                Time.timeScale = 0;
                break;
            case E_GUI_STATE.THEEND:
                Time.timeScale = 0;
                break;
            case E_GUI_STATE.GAMEOVER:
                Time.timeScale = 0;
                break;
            case E_GUI_STATE.MISSION:
                Time.timeScale = 0;
                break;
            case E_GUI_STATE.PLAY:
                Time.timeScale = 1;
                break;
        }

        ShowScene(state);
        curGUIState = state;
    }

    public void UpdateGUIStatus()
    {     
        switch (curGUIState)
        {
            case E_GUI_STATE.TITLE:
                PlayerHealth.instance.health = PlayerHealth.instance.maxHealth;
                PlayerHealth.instance.killEnemy = 0;
                itemInventoryObject.container.Clear();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            case E_GUI_STATE.THEEND:
                EventGameRestart();            
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            case E_GUI_STATE.GAMEOVER:
                EventGameRestart();
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            case E_GUI_STATE.MISSION:
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                break;
            case E_GUI_STATE.PLAY:
                PlayerHealth.instance.Set();
                EventGameOver();
                EventGameEnd();
                if (inventory.activeSelf == false)
                {
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                }
                if (Input.GetButtonDown("Inventory"))
                {
                    SetPopup();
                }
                break;
        }
    }

    public void SetPopup()
    {
        if (inventory.activeSelf == false)
        {
            inventory.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            inventory.SetActive(false);
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void EventGameOver()
    {      
        if (PlayerHealth.instance.health <= 0)
        {
            SetGUIStatus(E_GUI_STATE.GAMEOVER);
        }
    }

    public void EventGameEnd()
    {     
        if (PlayerHealth.instance.killEnemy >= 30)
        {
            SetGUIStatus(E_GUI_STATE.THEEND);
        }
    }

    public void EventGUISceneChange(int idx)
    {
        SetGUIStatus((E_GUI_STATE)idx);
    }

    public void EventGameRestart()
    {       
        if ((PlayerHealth.instance.health <= 0 || PlayerHealth.instance.killEnemy >= 30) && Input.GetMouseButtonDown(0))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
