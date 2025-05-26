using Nez;

namespace RoguelikeFNA
{
    public static class EntityListExt{
        public static TiledEntity GetByTiledId(this EntityList entityList, int id)
        {
            foreach (var entity in entityList.EntitiesOfType<TiledEntity>())
                if(entity.Enabled && entity.TiledId == id)
                    return entity;
            return null;
        }
    }

    public class TiledEntity : Entity
    {
        public int TiledId;

        public TiledEntity(string name, int tiledId) : base(name)
        {
            TiledId = tiledId;
        }
        
        public TiledEntity GetByTiledId(int id)
        {
            return Scene.Entities.GetByTiledId(id);
        }
    }
}