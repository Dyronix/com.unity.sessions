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

        public readonly SessionStartedUnityEvent OnSessionStarted = new SessionStartedUnityEvent();
        public readonly SessionEndedUnityEvent OnSessionEnded = new SessionEndedUnityEvent();
        public readonly PlayerDataSetupUnityEvent OnPlayerDataSetup = new PlayerDataSetupUnityEvent();
        public readonly PlayerDataSet OnPlayerDataSet = new PlayerDataSet();

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