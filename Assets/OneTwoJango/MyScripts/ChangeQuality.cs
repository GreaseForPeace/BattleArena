using UnityEngine;

public class ChangeQuality : MonoBehaviour
{

    public void Quality(int level)
    {
        
            QualitySettings.SetQualityLevel(level);
     }

        
}