using UnityEngine;
using Mirror;

    public class NetworkRoomPlayerExt : NetworkRoomPlayer
    {
        public override void OnStartClient()
        {
            //Debug.Log($"OnStartClient {gameObject}");
        }

        public override void OnClientEnterRoom()
        {
            //Debug.Log($"OnClientEnterRoom {SceneManager.GetActiveScene().path}");
            //Access.GameSettings().OnlinePlayerAlias = Constants.GO_PLAYER + this.index.ToString();
            gameObject.name = "Room" + Constants.GO_PLAYER + this.index.ToString();
        }

        public override void OnClientExitRoom()
        {
            //Debug.Log($"OnClientExitRoom {SceneManager.GetActiveScene().path}");
        }

        public override void IndexChanged(int oldIndex, int newIndex)
        {
            //Debug.Log($"IndexChanged {newIndex}");
            //Access.GameSettings().OnlinePlayerAlias = Constants.GO_PLAYER + this.index.ToString();
            gameObject.name = "Room" + Constants.GO_PLAYER + this.index.ToString();
        }

        public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
        {
            //Debug.Log($"ReadyStateChanged {newReadyState}");
        }

        public override void OnGUI()
        {
            base.OnGUI();
        }
    }

