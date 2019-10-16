﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Her0inEnemy : MonoBehaviour
{
	[Header("Infectee")]
	public bool isAttack;
	public float findRadius;
	public float attackRange;
	public float moveSpeed;
	public LayerMask humanMask;
	public RagDollDIeCtrl myRagDollCtrl;

	[Header("if generated by generator check")]
	public bool isGenerated = false;

	[Header("Limpid")]
	public bool isLimpid;
	public bool isDissolved;
	public float dissolveDistance;
	public SpawnEffect spawnEffect;
	public DamagedEffect damagedEffect;

	Animator anim;
	Rigidbody rgbd;
	NavMeshAgent navMesh;
	[HideInInspector] public Transform target;

	private ChangeRagDoll myChange;
	[HideInInspector] public Vector3 hitPos;
	public bool followTarget;
	bool settingTrigger;
	bool corTrigger;
	bool foundTarget;
	public GameObject player;

	Health info;

	void OnEnable()
	{
		if (isGenerated)
		{
			if (player)
			{
				target = player.transform;
				anim.applyRootMotion = false;

				StartCoroutine(SetNextMove());
			}
		}
	}

	void Awake()
	{
		damagedEffect = FindObjectOfType<DamagedEffect>();
		anim = GetComponent<Animator>();
		rgbd = GetComponent<Rigidbody>();
		navMesh = GetComponent<NavMeshAgent>();
		myChange = GetComponentInParent<ChangeRagDoll>();
		player = FindObjectOfType<PlayerCtrl>().gameObject;
	}

	void Start()
	{
		damagedEffect = FindObjectOfType<DamagedEffect>();
		info = GetComponent<Health>();
		info.diedByBullet.AddListener(AfterDie);

		int random = Random.Range(0, 2);

		if (random == 1)
		{
			//anim.SetBool("Walk", true);
			anim.SetTrigger("Walk");
			anim.applyRootMotion = true;
		}

		StartCoroutine(SetNextMove());
	}

	IEnumerator SetNextMove()
	{
		AnimatorStateInfo info2 = anim.GetCurrentAnimatorStateInfo(0);
		if (!followTarget && !isGenerated)
		{
			Collider[] humanInRadius = Physics.OverlapSphere(transform.position, findRadius, humanMask);
			for (int i = 0; i < humanInRadius.Length; i++)
			{
				if (humanInRadius[i].transform.CompareTag("Player"))
				{
					target = humanInRadius[i].transform;
					if (navMesh.isOnNavMesh) navMesh.SetDestination(target.position);
					StartCoroutine(Follow());
					followTarget = true;
				}
			}
		}

		if (navMesh.enabled && target != null) navMesh.SetDestination(target.transform.position);

		if (info2.IsName("Attack") || info2.IsName("Run"))
		{
			Vector3 calcuatledtarget = new Vector3(target.transform.position.x, this.transform.position.y, target.transform.position.z);
			transform.LookAt(calcuatledtarget);
		}

		if (isLimpid)
		{
			if (!isDissolved)
			{
				if (navMesh.enabled && navMesh.remainingDistance != 0 && navMesh.remainingDistance < navMesh.stoppingDistance + dissolveDistance)
				{
					spawnEffect.enabled = true;
					isDissolved = true;
				}

				if (spawnEffect.enabled && !isGenerated)
				{
					target = player.transform;
					if (navMesh.isOnNavMesh) navMesh.SetDestination(target.position);
					StartCoroutine(Follow());
					followTarget = true;
					isDissolved = true;
				}
			}
		}

		yield return new WaitForSeconds(.1f);
		StartCoroutine(SetNextMove());
	}

	public IEnumerator Follow()
	{
		if (!navMesh.enabled)
		{
			navMesh.enabled = true;
			target = player.transform;
			navMesh.SetDestination(target.position);
		}
		rgbd.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
		rgbd.drag = 50;
		//rgbd.mass = 100;

		anim.applyRootMotion = false;
		navMesh.speed = moveSpeed;

		anim.SetBool("Run", true);
		navMesh.enabled = true;

		while (navMesh.enabled && navMesh.remainingDistance >= navMesh.stoppingDistance)
		{
			yield return null;
		}

		navMesh.enabled = false;
		anim.SetBool("Run", false);

		StartCoroutine(Attack());
	}

	public float attackSpeed;

	public IEnumerator Attack()
	{
		if (!foundTarget)
		{
			foundTarget = true;
		}

		else
		{
			if (player.transform.name.Equals("Player")) anim.SetTrigger("Attack");
		}

		yield return new WaitForSeconds(attackSpeed);

		navMesh.speed = 0f;
		navMesh.enabled = true;

		while (navMesh.remainingDistance.Equals(0))
			yield return null;

		if (navMesh.enabled && navMesh.remainingDistance >= navMesh.stoppingDistance)
		{
			StartCoroutine(Follow());
		}

		if (navMesh.enabled && navMesh.remainingDistance <= navMesh.stoppingDistance)
		{
			StartCoroutine(Attack());
		}
	}

	//public void SetHitPos(Vector3 pos)
	//{
	//    hitPos = pos;
	//}

	//private void OnDrawGizmosSelected()
	//{
	//	if (!isGenerated)
	//	{
	//		Gizmos.color = Color.red;
	//		Gizmos.DrawWireSphere(transform.position, findRadius);
	//	}
	//	else return;
	//}

	public void AfterDie(Vector3 pos)
	{
		hitPos = pos;
		myRagDollCtrl.speed = navMesh.velocity.magnitude;
		myRagDollCtrl.AttackedPos = hitPos;
		myRagDollCtrl.hitByBullet = true;
		isGenerated = true;
		settingTrigger = false;

		if (DefenseGenerator.Instance)
			DefenseGenerator.Instance.killedInfectee += 1;
		myChange.StartCoroutine(myChange.ChangeRagdoll());
	}

	public void AfterDie()
	{
		myRagDollCtrl.speed = navMesh.velocity.magnitude;
		myRagDollCtrl.AttackedPos = hitPos;
		myRagDollCtrl.hitByBullet = false;
		isGenerated = true;
		myChange.StartCoroutine(myChange.ChangeRagdoll());
	}

	void OnCollisionEnter(Collision other)
	{
		if (other.gameObject.CompareTag("Floor"))
		{
			if (!settingTrigger)
			{
				rgbd.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;

				if (isGenerated) StartCoroutine(Follow());
				settingTrigger = true;
			}

			else return;
		}
	}

	void OnCollisionStay(Collision other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			PlayerCtrl.Instance.m_WalkSpeed = 4;
			PlayerCtrl.Instance.m_RunSpeed = 4;
		}
	}

	void OnCollisionExit(Collision other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			PlayerCtrl.Instance.m_WalkSpeed = 4;
			PlayerCtrl.Instance.m_RunSpeed = 10;
		}
	}
}