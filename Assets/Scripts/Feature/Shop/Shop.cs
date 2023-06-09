using Player.Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Shop : MonoBehaviour, IInteractable
{
    public List<Item> AvailableItems;
    [Header("References")]
    public GameObject shopPanel;
    public Transform shopItemList;
    public GameObject shopItemPrefab;
    public Transform playerTransform;
    private Collider collider;
    private Animator shopAnimator;
    private AudioSource shopAudioSource;

    [Header("Parameters")]
    public float minTeleportDistance = 10f;
    public float callPlayerDistance = 7f;

    [Header("Chara")]
    public AudioClip CallSound;
    public AudioClip OpenSound;
    public AudioClip AcceptSound;
    public AudioClip RejectSound;
    public AudioClip CloseSound;

    private bool playerCalled = false;

    private PlayerInventory targetInventory;
    private PlayerAction playerControls;

    public Action OnPanelOpen;

    private void Awake()
    {
        shopAnimator = GetComponentInChildren<Animator>();
        shopAudioSource = GetComponentInChildren<AudioSource>();
        collider = GetComponent<Collider>();
    }

    private void Start()
    {
        gameObject.SetActive(false);
        GameManager.Instance.DungeonNavMesh.OnDungeonNavMeshBuilt += () => { gameObject.SetActive(true); transform.position = RandomTeleportPoint(); };
        playerControls = InputManager.playerAction;
        RegisterInputCallback();
        foreach (var item in AvailableItems)
        {
            GameObject go = Instantiate(shopItemPrefab, shopItemList);
            var shopItem = go.GetComponent<ShopItemUI>();
            shopItem.Initialize(this, item);
        }
    }

    #region Callbacks
    private void RegisterInputCallback()
    {
        if (playerControls == null) return;

        // Close Shop
        playerControls.Panel.Cancel.performed += (ctx) => { CloseShop(); };
    }
    #endregion

    private void Update()
    {
        if (!playerCalled && Vector3.Distance(playerTransform.position, transform.position) < callPlayerDistance) CallPlayer();
    }

    private void CloseShop()
    {
        if (!shopPanel.activeSelf) return;

        shopPanel.SetActive(false);
        InputManager.ToggleActionMap(playerControls.Gameplay);

        collider.enabled = false;
        shopAudioSource.PlayOneShot(CloseSound);
        shopAnimator.SetTrigger("Bye");
        StartCoroutine(WaitToTeleport());
    }

    private IEnumerator WaitToTeleport()
    {
        yield return new WaitForSeconds(3f);
        transform.position = RandomTeleportPoint();
        playerCalled = false;
        collider.enabled = true;
        shopAnimator.ResetTrigger("Bye");
        shopAnimator.Play("default", 1);
    }

    private void CallPlayer()
    {
        shopAudioSource.PlayOneShot(CallSound);
        playerCalled = true;
    }

    private Vector3 RandomTeleportPoint()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        int maxIndices = navMeshData.indices.Length;
        float distance = 0f;
        Vector3 point = Vector3.zero;

        while (distance < minTeleportDistance)
        {
            point = navMeshData.vertices[navMeshData.indices[Random.Range(0, maxIndices)]];
            distance = Vector3.Distance(point, transform.position);
        }

        NavMeshPath path = new NavMeshPath();

        if (NavMesh.SamplePosition(point, out var hit, Mathf.Infinity, 1 << NavMesh.GetAreaFromName("Walkable")))
        {
            if (NavMesh.CalculatePath(playerTransform.position, hit.position, 1 << NavMesh.GetAreaFromName("Walkable"), path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    return hit.position;
                }
            }
        }

        return RandomTeleportPoint();
    }

    public void Interact()
    {
        InputManager.ToggleActionMap(playerControls.Panel);
        shopPanel.SetActive(true);
        shopAudioSource.PlayOneShot(OpenSound);
        OnPanelOpen?.Invoke();
    }

    public void Interact(GameObject other)
    {
        if (other.CompareTag("Player"))
        {
            targetInventory = other.GetComponent<PlayerInventory>();

            foreach (var item in AvailableItems) {

                if (item is SkillUpgrade)
                {
                    SkillUpgrade skillItem = item as SkillUpgrade;
                    int skillLevel = targetInventory.GetComponent<PlayerSkill>().playerWeapon.SkillMap[skillItem.baseSkill].skillLevel;
                    skillItem.currentUpgrade = skillLevel;
                    item.isDisabled = (skillLevel >= skillItem.totalSkillLevel);
                }
            }

            Interact();
        }
    }

    public void Buy(Item item)
    {
        if (item.isConsumable && targetInventory.IsFull) return;

        if (GameManager.Instance.DeductCoins(item.GetPrice()))
        {
            if (item.isConsumable) targetInventory.AddItem(item);
            if (item is SkillUpgrade)
            {
                // DO skill upgrade
                SkillUpgrade skillItem = item as SkillUpgrade;
                var playerSkill = targetInventory.GetComponent<PlayerSkill>();
                int skillLevel = playerSkill.playerWeapon.SkillMap[skillItem.baseSkill].skillLevel;
                if (skillItem.GetSkill(skillLevel).skillLevel >= skillItem.totalSkillLevel)
                {
                    item.isDisabled = true;
                    playerSkill.AddSkill(skillItem.GetSkill(skillLevel));
                    return;
                }
                skillItem.currentUpgrade = skillItem.GetSkill(skillLevel).skillLevel;
                playerSkill.AddSkill(skillItem.GetSkill(skillLevel));
            }
            shopAudioSource.PlayOneShot(AcceptSound);
        }
        else
        {
            shopAudioSource.PlayOneShot(RejectSound);
            Debug.Log("Not enough money to buy " + item.name);
        }
    }
}
