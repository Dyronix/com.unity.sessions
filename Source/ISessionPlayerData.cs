namespace Unity.Sessions
{
    public interface ISessionPlayerData
    {
        bool IsConnected { get; set; }
        ulong ClientID { get; set; }
        
        void Reinitialize();
    }
}