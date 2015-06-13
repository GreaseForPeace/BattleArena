using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour {

	public Transform bullet;
	public Transform MuzzleFlash;
	public int BulletForce = 600;
	public int MaxAmmoFly = 0;
	public float shootCooldown = 0.4f;
	public float Timer = 0;
	public float Cooldown = 0f;
	public float MuzzleFlashLifetime = 0.0f;
	public bool isShoot = true;
	public AudioClip Fire;


	void Start (){	

		MuzzleFlash.active = false;
		Cooldown = 0f;
		isShoot = true;
		MaxAmmoFly = 0;
		Timer = 0f;
	}


	void Update (){

		if(MaxAmmoFly ==1 && isShoot == false){
			MaxAmmoFly = 0;
		}

		if(!isShoot){
			if(Timer<shootCooldown){
				Timer += Time.deltaTime;
			}
			else{
				Timer = 0f;
				isShoot = true;
			}
		}

		Cooldown +=Time.deltaTime;
		if(Input.GetKey("n"))
		{
			if(Cooldown>=0.10f && isShoot == true){
				Cooldown=0f;
				Transform BulletInstance = (Transform) Instantiate(bullet, GameObject.Find("BulletSpawnPoint").transform.position, Quaternion.identity);
				BulletInstance.rigidbody.AddForce(-transform.up * BulletForce);
				audio.PlayOneShot(Fire);
				MuzzleFlash.active = true;
				MuzzleFlashLifetime = 0.1f;
				MaxAmmoFly++;
				if(MaxAmmoFly ==2){
					MaxAmmoFly = 0;
					isShoot = false;
					Timer = 0f;
				}
			}
		}

		if(MuzzleFlashLifetime>0)
		{
			MuzzleFlashLifetime -= Time.deltaTime;
		}

		if(MuzzleFlashLifetime<=0)
		{
			MuzzleFlash.active = false;
		}

	}
}

