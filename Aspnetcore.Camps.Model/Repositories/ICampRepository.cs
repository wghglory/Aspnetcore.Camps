using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspnetcore.Camps.Model.Entities;

namespace Aspnetcore.Camps.Model.Repositories
{
    public interface ICampRepository
    {
        // Basic DB Operations
        void Add<T>(T entity) where T : class;

        void Delete<T>(T entity) where T : class;
        Task<bool> SaveAllAsync();

        // Camps
        IQueryable<Camp> GetAllCamps();

        Camp GetCamp(int id);
        Camp GetCampWithSpeakers(int id);
        Camp GetCampByMoniker(string moniker);
        Camp GetCampByMonikerWithSpeakers(string moniker);

        // Speakers
        IQueryable<Speaker> GetSpeakers(int id);

        IQueryable<Speaker> GetSpeakersWithTalks(int id);
        IQueryable<Speaker> GetSpeakersByMoniker(string moniker);
        IQueryable<Speaker> GetSpeakersByMonikerWithTalks(string moniker);
        Speaker GetSpeaker(int speakerId);
        Speaker GetSpeakerWithTalks(int speakerId);

        // Talks
        IQueryable<Talk> GetTalks(int speakerId);

        Talk GetTalk(int talkId);

        // CampUser
        CampUser GetUser(string userName);
    }
}