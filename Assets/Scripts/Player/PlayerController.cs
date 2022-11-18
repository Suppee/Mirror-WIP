using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    private Camera camera;
    public NavMeshAgent player;
    private PlayerScore playerScoreUI;
    private GlobalScore globalScoreUI;

    public Material playerBodyNormal;
    public Material playerBodyStunned;
    public Material playerBodyInvincible;

    public GameObject wallPrefab;

    [SyncVar]
    private bool isStunned = false;

    [SyncVar]
    private bool isInvincible = false;

    [SyncVar(hook = nameof(DisplayPlayerScore))]
    public int playerScore = 0;

    public void DisplayPlayerScore(int oldScore, int newPlayerScore)
    {
        if(isLocalPlayer)
        {
            playerScoreUI.DisplayPlayerScore(newPlayerScore);
        }
    }

    private void Awake()
    {
        camera = FindObjectOfType<Camera>();
        playerScoreUI = FindObjectOfType<PlayerScore>();
        globalScoreUI = FindObjectOfType<GlobalScore>();
    }

    // Start is called before the first frame update
    void Start()
    {
        gameObject.tag = "Player";
    }

    // Update is called once per frame
    void Update()
    {
        if(isLocalPlayer)
        {
            if (Input.GetMouseButtonDown(0) && !isStunned)
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit destination;

                if (Physics.Raycast(ray, out destination))
                {
                    player.SetDestination(destination.point);
                }
            }

            if(Input.GetKeyDown("space") && !isStunned)
            {
                Shoot();
            }
            if (Input.GetKeyDown("b") && !isStunned)
            {
                CmdBuildWall();
            }
        }
    }

    [Client]
    void Shoot()
    {
        RaycastHit shot;
        Vector3 forward = transform.TransformDirection(Vector3.forward);

        if(Physics.Raycast(transform.position, (forward), out shot))
        {
            if(shot.collider.tag == "Player")
            {
                bool isStunnedEnemy = shot.collider.gameObject.GetComponent<PlayerController>().isStunned;
                bool isInvincibleEnemy = shot.collider.gameObject.GetComponent<PlayerController>().isInvincible;

                if (!isStunnedEnemy && !isInvincibleEnemy)
                {
                    print("hit Player");
                    CmdSetPlayerScore(2);
                    NetworkIdentity networkIdentity = shot.collider.gameObject.GetComponent<NetworkIdentity>();
                    shot.collider.gameObject.GetComponent<PlayerController>().CmdGotShot(networkIdentity);
                }
                else
                {
                    print("hit stunned player");
                }
            }
            else
            {
                print("hit wall");
                CmdDestroyWall(shot.collider.gameObject);
            }
        }
        else
        {
            print("hit nothing");
        }
    }

    [Command(requiresAuthority = false)]
    void CmdGotShot(NetworkIdentity networkIdentity)
    {
        globalScoreUI.globalScoreValue++; 
        CmdSetPlayerScore(-1);
        StartCoroutine(DisplayHit(networkIdentity));
        
    }

    [Command(requiresAuthority = false)]
    public void CmdSetStunned(bool stunned)
    {
        isStunned = stunned;
    }

    [Command(requiresAuthority = false)]
    public void CmdSetInvincible(bool invincible)
    {
        isInvincible = invincible;
    }

    [Command(requiresAuthority =false)]
    public void CmdSetPlayerScore(int change)
    {
        playerScore += change;
    }

    [Command]
    public void CmdDestroyWall(GameObject wall)
    {
        NetworkServer.Destroy(wall);
    }

    [Command]
    public void CmdBuildWall()
    {
        Vector3 playerPos = transform.position;
        Vector3 playerDir = transform.forward;
        float spawnDistance = 2;
        Vector3 spawnPos = playerPos + playerDir * spawnDistance;

        GameObject wallObj = Instantiate(wallPrefab, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(wallObj);
    }

    [Server]
    public IEnumerator DisplayHit(NetworkIdentity networkIdentity)
    {
        CmdSetStunned(true);
        RpcSwitchMaterial(1);

        float coolDownTime = 0.0f;
        while (coolDownTime < 10.0f)
        {
            coolDownTime += Time.deltaTime;
            yield return null;
        }

        RpcSwitchMaterial(0);
        CmdSetStunned(false);

        TargetMakeInvincible(networkIdentity.connectionToClient);
    }

    [ClientRpc]
    public void RpcSwitchMaterial(int material)
    {
        switch (material)
        {
            case 0:
                gameObject.GetComponent<MeshRenderer>().material = playerBodyNormal;
                break;

            case 1:
                gameObject.GetComponent<MeshRenderer>().material = playerBodyStunned;
                break;

            default:
                break;
        }
    }

    [TargetRpc]
    public void TargetMakeInvincible(NetworkConnection target)
    {
        StartCoroutine(DisplayInvincible());
    }

    [Client]
    public IEnumerator DisplayInvincible()
    {
        CmdSetInvincible(true);
        gameObject.GetComponent<MeshRenderer>().material = playerBodyInvincible;

        float coolDownTime = 0.0f;
        while (coolDownTime < 10.0f)
        {
            coolDownTime += Time.deltaTime;
            yield return null;
        }

        CmdSetInvincible(false);
        gameObject.GetComponent<MeshRenderer>().material = playerBodyNormal;
    }
}
