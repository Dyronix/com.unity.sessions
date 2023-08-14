namespace Unity.Sessions
{
    public abstract class BaseSessionManager : ISessionManager
    {
        //--------------------------------------------------------------------------------------
        // Events
        public class SessionStartedUnityEvent : UnityEvent {};
        public class SessionEndedUnityEvent : UnityEvent {};
        public class PlayerDataSetupUnityEvent : UnityEvent<ulong, string, T> { }
        public class PlayerDataSet : UnityEvent<ulong, T> {};

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
        public abstract void OnSessionStarted();
        //--------------------------------------------------------------------------------------
        public abstract void OnSessionEnded();
        //--------------------------------------------------------------------------------------
        public abstract void OnServerEnded();
    }
}