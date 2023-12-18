using System.Collections.Generic;
using WineryApi.Models;

namespace WineryApi.Data.Interfaces
{
    public interface IWineryRepository
    {
        public bool Add(ref Winery winery);
        public bool Delete(string id);
        public List<Winery> GetWineries();
        public Winery GetWineryById(string id);
        public bool UpdateRating(string id, int rating);
        public bool UpdateWinery(string id, Winery wineryIn);

    }
}
