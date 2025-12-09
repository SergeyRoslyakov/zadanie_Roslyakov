using MauiApp4.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MauiApp4.Services
{
    internal interface IApiService
    {
        Task<List<ContactDto>> GetContactsAsync(string search = null);
        Task<ContactDto> GetContactAsync(int id);
        Task<ContactDto> CreateContactAsync(CreateContactDto contact);
        Task UpdateContactAsync(int id, UpdateContactDto contact);
        Task DeleteContactAsync(int id);
    }
}