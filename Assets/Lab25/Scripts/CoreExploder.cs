﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoreExploder : MonoBehaviour
{
	public Transform player;
	public GameObject pushPlayerBlock;
	public GameObject[] computers;

	public float pushForce = 10f;
	public Light blueLight;
	public Light directionalLight;

	public AudioSource[] audioSource; // run, stop, spark, explode, glass

	public GlassBreak[] glassBreaker;
	public bool isReadyToOverPower;
	public bool isOverPowering;

	float currentExplodeRate = 0;
	public float maxExplodeRate = 3;
	bool isExploded;

	public ParticleSystem[] sparkParticle;
	public ParticleSystem[] explodeParticle;

	MotionBlur motionBlur;
	ParticleSystem[] spartkPs;
	ParticleSystem[] explodePs;

	public TextMeshProUGUI explanationText;
	public TextMeshProUGUI explodeRateText;

	TestShake testShake;

	IEnumerator volumeController;

	bool isPowerdOnFirst;
    ExplosionEffect explosioneffect;

    void Awake() {
		spartkPs = sparkParticle;
		explodePs = explodeParticle;
		for (int i = 0; i < explodePs.Length; i++) explodePs[i].Stop();
		testShake = GetComponent<TestShake>();
        explosioneffect = GetComponent<ExplosionEffect>();
		motionBlur = GetComponent<MotionBlur>();

	}
	float lockY;
	void Update()
	{
		if (!isReadyToOverPower) return;
		if (isReadyToOverPower) explodeRateText.text = currentExplodeRate.ToString("N2");

		if (Input.GetKey(KeyCode.F) && !isExploded)
		{
			currentExplodeRate = Mathf.Clamp(currentExplodeRate +=Time.deltaTime * 1.7f, 0, maxExplodeRate);
			blueLight.intensity += Time.deltaTime * 10f;
			isOverPowering = true;
			isPowerdOnFirst = true;
			lightOff = false;
			if (!audioSource[0].GetComponent<AudioSource>().isPlaying) audioSource[0].GetComponent<AudioSource>().Play();
		}

		if(Input.GetKeyUp(KeyCode.F) && !isExploded)
		{
			if (!isExploded) currentExplodeRate = 0;
			isOverPowering = false;
			isTurnOff = true;
			lightOff = true;
		}

		if (!isExploded && currentExplodeRate >= maxExplodeRate)
		{
			StartCoroutine(Explode());
			isExploded = true;
		}

		EffectUpdate();
		SoundControl();
	}
	bool lightOff;
	void FixedUpdate()
	{
		Push();
	}

	bool isTurnOff;
	float timeCount;
	float waitForSecond;

	void Push()
	{
		if (!isExploded) return;

		waitForSecond += Time.fixedDeltaTime;

		if (waitForSecond < .7f) return;

		if (!pushPlayerBlock.activeSelf)
		{
			pushPlayerBlock.transform.position = player.transform.position - Vector3.right * 5f;
			pushPlayerBlock.SetActive(true);
		}

		if (timeCount < .5f)
		{
			pushPlayerBlock.transform.Translate(Vector3.right * pushForce * Time.fixedDeltaTime);
			timeCount += Time.fixedDeltaTime;
		}

		else pushPlayerBlock.SetActive(false);
	}

	void SoundControl()
	{
		if (!isTurnOff) return;

		if (!audioSource[1].GetComponent<AudioSource>().isPlaying) audioSource[1].GetComponent<AudioSource>().Play();

		audioSource[0].GetComponent<AudioSource>().volume -= Time.deltaTime * 0.18f;

		if (audioSource[1].GetComponent<AudioSource>().time > 1.5)
		{
			if(audioSource[0].GetComponent<AudioSource>().isPlaying) audioSource[0].GetComponent<AudioSource>().Stop();
			audioSource[1].GetComponent<AudioSource>().volume -= Time.deltaTime * 0.15f;
		}

		if (audioSource[1].GetComponent<AudioSource>().time > 1.95)
		{
			audioSource[1].GetComponent<AudioSource>().Stop();
		}

		if (!audioSource[1].GetComponent<AudioSource>().isPlaying)
		{
			audioSource[1].GetComponent<AudioSource>().Stop();
			audioSource[0].GetComponent<AudioSource>().volume = 0.2f;
			audioSource[1].GetComponent<AudioSource>().volume = 0.2f;
			isTurnOff = false;
		}
	}

	IEnumerator SoundOff()
	{
		while(audioSource[0].GetComponent<AudioSource>().volume <= 0f)
		{
			audioSource[0].GetComponent<AudioSource>().volume -= Time.deltaTime;
			yield return null;
		}
		audioSource[0].GetComponent<AudioSource>().Stop();
		audioSource[0].GetComponent<AudioSource>().volume = 0.2f;
		isReadyToOverPower = false;
	}

	IEnumerator Explode()
	{
		yield return new WaitForSeconds(1f);
        explosioneffect.AfterExplosion();

		for(int i = 0; i < computers.Length; i++)
		{
			computers[i].GetComponent<Rigidbody>().useGravity = true;
		}

        StartCoroutine(LightChanger());
		motionBlur.DoMotionBlur(4f);

		for (int i = 0; i < explodePs.Length; i++) explodePs[i].Play();
		for (int i = 0; i < spartkPs.Length; i++) spartkPs[i].Stop();
		for (int i = 0; i < glassBreaker.Length; i++) glassBreaker[i].Break();

		audioSource[0].GetComponent<AudioSource>().Stop();
		if (!audioSource[2].GetComponent<AudioSource>().isPlaying) audioSource[2].GetComponent<AudioSource>().Play();
		if (!audioSource[3].GetComponent<AudioSource>().isPlaying) audioSource[3].GetComponent<AudioSource>().Play();

		StartCoroutine(cameraShake());
		currentExplodeRate = 0;
		explodeRateText.text = "0.00";
		isReadyToOverPower = false;

		explanationText.color = new Color(explanationText.color.r, explanationText.color.g, explanationText.color.b, 0.2f);
		explodeRateText.color = new Color(explodeRateText.color.r, explodeRateText.color.g, explodeRateText.color.b, 0.2f);

		yield return new WaitForSeconds(.2f);
		motionBlur.DoMotionBlur(5f);
		if (!audioSource[4].GetComponent<AudioSource>().isPlaying) audioSource[4].GetComponent<AudioSource>().Play();

		yield return null;
	}

	IEnumerator LightChanger()
	{
		float alpha = 0;

		while(alpha <= 1f)
		{
			blueLight.intensity = Random.Range(1f, 15f);
			alpha += Time.deltaTime;
			yield return null;
		}

		while (directionalLight.intensity >= 0.1f)
		{
			directionalLight.intensity -= Time.deltaTime * 2f;
			yield return null;
		}

		blueLight.intensity = 2f;

		yield return null;
	}

	//IEnumerator soundPlay()
	//{
	//	// explode sound play
	//	if (!audioSource[2].GetComponent<AudioSource>().isPlaying) audioSource[2].GetComponent<AudioSource>().Play();
	//	if (!audioSource[3].GetComponent<AudioSource>().isPlaying) audioSource[3].GetComponent<AudioSource>().Play();
	//	yield return null;
	//}

	IEnumerator cameraShake()
	{
		// cameraShake
		testShake.Shake();
		yield return new WaitForSeconds(.2f);
        testShake.Shake();
		yield return null;
	}

	void EffectUpdate()
	{
		if (lightOff && blueLight.intensity > 5) blueLight.intensity -= Time.deltaTime * 5f;
		if (isOverPowering)
		{
			for (int i = 0; i < spartkPs.Length; i++)
			{
				spartkPs[i].startLifetime = Mathf.Clamp(spartkPs[i].startLifetime += 0.05f, 0, 1.5f);
			}
		}

		else
		{
			for (int i = 0; i < spartkPs.Length; i++)
			{
				spartkPs[i].startLifetime = Mathf.Clamp(spartkPs[i].startLifetime -= 0.1f, 0, 1.5f);
			}
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.CompareTag("Player"))
		{
			StartCoroutine(IncreaseLight());
			this.gameObject.GetComponent<BoxCollider>().enabled = false;
		}
	}

	IEnumerator IncreaseLight()
	{
		while(directionalLight.intensity < 0.4f)
		{
			directionalLight.intensity += Time.deltaTime;
			yield return null;
		}
	}
}
