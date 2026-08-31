using HouseOfHoundAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HouseOfHoundAPI.Data
{
    public interface ITherapistRepository
    {
        IEnumerable<TherapistDto> GetAll();
        TherapistDto Get(int id);
        int Create(CreateTherapistDto dto);
        void Update(int id, UpdateTherapistDto dto);
        void Delete(int id);
    }
}
