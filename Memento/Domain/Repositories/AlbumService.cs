using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Domain.Repository
{
    public class AlbumService : BaseService
    {
        private DropsService dropService;
        public AlbumService(DropsService dropsService) {
            this.dropService = dropsService;
        }

        public async Task<AlbumModel> Get(int userId, int albumId) {
            var album = await Context.Albums.SingleOrDefaultAsync(x => x.UserId == userId && x.AlbumId == albumId);
            if (album == null) throw new NotFoundException("Album not found");
            return MapAlbumModel(album);
        }

        public async Task<List<AlbumModel>> GetAll(int userId) {
            var albums = await Context.Albums.Where(x => x.UserId == userId).OrderBy(x => x.Name).ToListAsync();
            return albums.Select(MapAlbumModel).ToList();
        }

        public async Task<List<AlbumModel>> GetActive(int userId) {
            var albums = await GetAll(userId);
            return albums.Where(x => !x.Archived).ToList();
        }

        public async Task Delete(int userId, int albumId) {
            var album = await Context.Albums.SingleOrDefaultAsync(x => x.AlbumId == albumId && userId == x.UserId);
            if (album != null) {
                Context.Albums.Remove(album);
                await Context.SaveChangesAsync();
            }
        }

        public async Task<AlbumModel> Create(int userId, string name) {
            var album = await Context.Albums.SingleOrDefaultAsync(x => x.Name == name && userId == x.UserId);
            if (album != null) throw new ConflictException("Album already exists");
            album = new Album { UserId = userId, Name = name, Created = DateTime.UtcNow };
            Context.Albums.Add(album);
            await Context.SaveChangesAsync();
            return MapAlbumModel(album);
        }

        public async Task<AlbumModel> Update(int userId, int albumId, string name, bool archived)
        {
            var album = await Context.Albums.SingleOrDefaultAsync(x => x.UserId == userId && x.AlbumId == albumId);
            if (album == null) throw new NotFoundException("Album not found");
            album.Name = name;
            album.Archived = archived;
            await Context.SaveChangesAsync();
            return MapAlbumModel(album);
        }

        public async Task<List<AlbumViewModel>> GetAlbumsForMoment(int userId, int id)
        {
            if (!dropService.CanView(userId, id)) throw new NotAuthorizedException("");
            
            var momentAlbums = await Context.AlbumDrops.Where(x => x.DropId == id
                && x.Album.UserId == userId).Select(s => s.AlbumId).ToListAsync();
            var allAlbums = await GetActive(userId);
            return allAlbums.Select(s => new AlbumViewModel {
                Selected = momentAlbums.Contains(s.AlbumId),
                AlbumId = s.AlbumId,
                Name = s.Name
            }).ToList();
        }

        public async Task AddToMoment(int userId, int id, int momentId)
        {
            if(!dropService.CanView(userId, momentId)) throw new NotAuthorizedException("");
            
            var momentAlbum = await Context.AlbumDrops.FirstOrDefaultAsync(x => x.DropId == momentId 
                && x.AlbumId == momentId
                && x.Album.UserId == userId);
            if (momentAlbum == null) {
                momentAlbum = new AlbumDrop {
                    AlbumId = id,
                    DropId = momentId,
                    Created = DateTime.UtcNow,
                };
                Context.AlbumDrops.Add(momentAlbum);
                await Context.SaveChangesAsync();
            }
        }

        public async Task RemoveToMoment(int userId, int id, int momentId)
        {
            var momentAlbum = await Context.AlbumDrops.FirstOrDefaultAsync(x => x.DropId == momentId
                && x.AlbumId == id
                && x.Album.UserId == userId);
            if (momentAlbum != null)
            {
                Context.AlbumDrops.Remove(momentAlbum);
                await Context.SaveChangesAsync();
            }
        }

        private AlbumModel MapAlbumModel(Album album) {
            return new AlbumModel { AlbumId = album.AlbumId, Name = album.Name, Archived = album.Archived };
        }
    }
}
