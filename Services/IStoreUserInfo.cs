using System.Threading.Tasks;

namespace Vabulu.Services
{
    internal interface IStoreUserInfo
    {
        Task DeleteUserDataAsync(Models.User user);
    }
}