﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aspnetcore.Camps.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aspnetcore.Camps.Model.Repositories
{
    public class CampRepository : ICampRepository
    {
        private readonly CampContext _context;

        public CampRepository(CampContext context)
        {
            _context = context;
        }

        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public IEnumerable<Camp> GetAllCamps()
        {
            return _context.Camps.Include(c => c.Location).OrderBy(c => c.EventDate);
        }

        public Camp GetCamp(int id)
        {
            return _context.Camps.Include(c => c.Location).FirstOrDefault(c => c.Id == id);
        }

        public Camp GetCampWithSpeakers(int id)
        {
            return _context.Camps.Include(c => c.Location).Include(c => c.Speakers)
                .ThenInclude(s => s.Talks).FirstOrDefault(c => c.Id == id);
        }

        public Camp GetCampByMoniker(string moniker)
        {
            return _context.Camps.Include(c => c.Location)
                .FirstOrDefault(c => c.Moniker.Equals(moniker, StringComparison.CurrentCultureIgnoreCase));
        }

        public Camp GetCampByMonikerWithSpeakers(string moniker)
        {
            return _context.Camps.Include(c => c.Location).Include(c => c.Speakers)
                .ThenInclude(s => s.Talks)
                .FirstOrDefault(c => c.Moniker.Equals(moniker, StringComparison.CurrentCultureIgnoreCase));
        }

        public Speaker GetSpeaker(int speakerId)
        {
            return _context.Speakers.Include(s => s.Camp).FirstOrDefault(s => s.Id == speakerId);
        }

        public IEnumerable<Speaker> GetSpeakers(int id)
        {
            return _context.Speakers.Include(s => s.Camp).Where(s => s.Camp.Id == id)
                .OrderBy(s => s.Name);
        }

        public IEnumerable<Speaker> GetSpeakersWithTalks(int id)
        {
            return _context.Speakers.Include(s => s.Camp).Include(s => s.Talks)
                .Where(s => s.Camp.Id == id).OrderBy(s => s.Name);
        }

        public IEnumerable<Speaker> GetSpeakersByMoniker(string moniker)
        {
            return _context.Speakers.Include(s => s.Camp)
                .Where(s => s.Camp.Moniker.Equals(moniker, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(s => s.Name);
        }

        public IEnumerable<Speaker> GetSpeakersByMonikerWithTalks(string moniker)
        {
            return _context.Speakers.Include(s => s.Camp).Include(s => s.Talks)
                .Where(s => s.Camp.Moniker.Equals(moniker, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy(s => s.Name);
        }

        public Speaker GetSpeakerWithTalks(int speakerId)
        {
            return _context.Speakers.Include(s => s.Camp).Include(s => s.Talks)
                .FirstOrDefault(s => s.Id == speakerId);
        }

        public Talk GetTalk(int talkId)
        {
            return _context.Talks.Include(t => t.Speaker).ThenInclude(s => s.Camp)
                .Where(t => t.Id == talkId).OrderBy(t => t.Title).FirstOrDefault();
        }

        public IEnumerable<Talk> GetTalks(int speakerId)
        {
            return _context.Talks.Include(t => t.Speaker).ThenInclude(s => s.Camp)
                .Where(t => t.Speaker.Id == speakerId).OrderBy(t => t.Title);
        }

        public CampUser GetUser(string userName)
        {
            return _context.Users.Include(u => u.Claims).Include(u => u.Roles)
                .Where(u => u.UserName == userName).Cast<CampUser>().FirstOrDefault();
        }

        public async Task<bool> SaveAllAsync()
        {
            return (await _context.SaveChangesAsync()) > 0;
        }
    }
}