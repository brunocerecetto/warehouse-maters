namespace WM.Core
{
    /// <summary>Local persistence boundary. Real impl arrives in Phase 6 (SAVE-01).</summary>
    public interface ISaveService
    {
        void Save(string key, string json);
        string Load(string key);
        bool HasKey(string key);
    }
}
