using System.Threading.Tasks;

namespace WebApp.Services.Interfaces
{
    public interface IConfigurationService
    {
        Task DeactivateProfile(int userId);
    }
}