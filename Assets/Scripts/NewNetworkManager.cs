using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NewNetworkManager : NetworkManager
{

    public GameObject Player;
    public GameObject PlayerFast;

    public struct PlayerSettings: NetworkMessage
    {
        public bool isFast;
        public int posX;
        public int posZ;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler<PlayerSettings>(SpawnPlayer);
    }

    public override void OnClientConnect()
    {
        base.OnClientConnect();

        PlayerSettings player = new PlayerSettings
        {
            isFast = true,
            posX = 2,
            posZ = 2
        };

        NetworkClient.Send(player);
    }

    void SpawnPlayer(NetworkConnectionToClient conn, PlayerSettings player)
    {
        GameObject gameobject;
        Vector3 playerPosition = new Vector3(player.posX, 0, player.posZ);

        if(player.isFast)
        {
            gameobject = Instantiate(PlayerFast, playerPosition, new Quaternion(0, 0, 0, 0));
        }
        else
        {
            gameobject = Instantiate(Player, playerPosition, new Quaternion(0, 0, 0, 0));
        }

        NetworkServer.AddPlayerForConnection(conn, gameobject);
    }

}
