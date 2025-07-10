using MagicVilla_Web.Models.Dto;

namespace MagicVilla_Web.Services.IServices
{
    public interface IAuthService
    {
        public Task<T> LoginAsync<T>(LoginRequestDTO objToCreate);
        public Task<T> RegisterAsync<T>(RegisterRequestDTO objToCreate);
    }
}
