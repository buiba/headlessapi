namespace EPiServer.ContentApi.IntegrationTests.TestSetup
{
    /// <summary>
    /// Components that holds request scoped data can implement this interface to get notification when a new RestRequest is initiatied
    /// </summary>
    public interface IRestRequestInitializer
    {
        void InitiateRequest();
    }
}
