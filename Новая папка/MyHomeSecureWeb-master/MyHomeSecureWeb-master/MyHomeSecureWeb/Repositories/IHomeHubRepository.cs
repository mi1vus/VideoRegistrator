using MyHomeSecureWeb.DataObjects;
using System;

namespace MyHomeSecureWeb.Repositories
{
    public interface IHomeHubRepository : IDisposable
    {
        HomeHub GetHub(string name);
        HomeHub GetHubById(string id);
        HomeHub AddHub(string name, byte[] tokenHash, byte[] salt);
        void SetLocation(string homeHubId, double latitude, double longitude, float radius);
    }
}