namespace Unity.Sessions
{
    public interface ISessionManager
    {
        public void DisconnectClient(ulong clientId);
        public bool IsDuplicateConnection(string playerId);
        public string GetPlayerId(ulong clientId);
        public void StartSession();
        public void EndSession();
        public void OnServerEnded();
    }
}