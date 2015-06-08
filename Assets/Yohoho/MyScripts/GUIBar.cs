using UnityEngine;

public class GUIBar : MonoBehaviour
{
   // GameObject hero = GameObject.Find("Jora");
    private float _maxHealth;
    private float _curHealth;
	private float _healthBarLen;	// Для вывода на экран
	public GUISkin MySkin;	// Скин
	public int Width;	// Настройка отображения
	public int Height;

    void Start()
    {
        var hero = GetComponent<PlayerClass>();
      //  transform.position = new Vector3(hero.transform.position.x, hero.transform.position.y + 60,
         //   hero.transform.position.z);
        _maxHealth = hero.GetComponent<PlayerClass>().MaxHp ; // общее количество жизней
        _curHealth = hero.GetComponent<PlayerClass>().CurrHp;	// Текущее количество
    }
    
    void Update () {
        
		_healthBarLen = _curHealth/_maxHealth;

	}
	
    void OnGUI()
    {
		GUI.skin = MySkin;
		
		// Преобразования позиция из мирового пространства в пространство экрана.
        Vector3 screenPosition = gameObject.transform.position;
        
		
		//Находится ли объект перед камерой
     //   Vector3 cameraRelative = Camera.main.transform.InverseTransformPoint(transform.position);
      //  if (cameraRelative.z > 0)
      //  {
        Rect position = new Rect(screenPosition.x, screenPosition.y, 60f, 15f);
        Debug.Log("Jora at " + gameObject.transform.position);
        Debug.Log("HpBar at " + position);
        GUI.Box(new Rect(screenPosition.x + 20, Screen.height - screenPosition.y - 100, 60f * _healthBarLen, 15f), " ", GUI.skin.GetStyle("Fon")); 
			GUI.Box(position, " ", GUI.skin.GetStyle("Bar"));
      //  }

    }

} 