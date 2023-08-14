using UnityEngine.Events;

namespace Unity.Sessions
{
    public abstract class BaseSessionManager : ISessionManager
    {
        //--------------------------------------------------------------------------------------
        // Events
        public class SessionStartedUnityEvent : UnityEvent {};
        public class SessionEndedUnityEvent : UnityEvent {};
        public class PlayerDataSetupUnityEvent : UnityEvent<ulong, string, ISessionPlayerData> { }
        public class PlayerDataSet : UnityEvent<ulong, ISessionPlayerData> {};

        public SessionStartedUnityEvent OnSessionStarted;
        public SessionEndedUnityEvent OnSessionEnded;
        public PlayerDataSetupUnityEvent OnPlayerDataSetup;
        public PlayerDataSet OnPlayerDataSet;

        //--------------------------------------------------------------------------------------
        public abstract void DisconnectClient(ulong clientId);
        //--------------------------------------------------------------------------------------
        public abstract bool IsDuplicateConnection(string playerId);
        //--------------------------------------------------------------------------------------
        public abstract string GetPlayerId(ulong clientId);
        //--------------------------------------------------------------------------------------
        public abstract void StartSession();
        //--------------------------------------------------------------------------------------
        public abstract void EndSession();
        //--------------------------------------------------------------------------------------
        public abstract void OnServerEnded();
    }
}