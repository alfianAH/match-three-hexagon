using UnityEngine;

public class BoardManager : MonoBehaviour
{
    #region Singleton

    private static BoardManager instance;

    public static BoardManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BoardManager>();

                if (instance == null)
                    Debug.LogError("BoardManager not Found");
            }

            return instance;
        }
    }

    #endregion
}
