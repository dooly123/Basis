// Copyright HTC Corporation All Rights Reserved.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace VIVE.OpenXR.Samples
{
    public class QuitAp : MonoBehaviour
    {
        public void ExitGame()
        {
            Debug.Log("ExitGame() " + SceneManager.GetActiveScene().name);
            Application.Quit();
        }

        public void BackToUpLayer()
        {
            Debug.Log("BackToUpLayer() " + SceneManager.GetActiveScene().name);
            SceneManager.LoadScene(0);
        }
    }
}
