using Assets.GameAssets.Scripts.Maze.Model;
using UnityEngine;

namespace Assets.GameAssets.Scripts.MazeUI.ImageHandling
{
    public class LoadCellPrefab : ILoadCellPrefab
    {
        private readonly ISpriteCache _spriteCache;
        private readonly ICellInformationLoader _cellInformationLoader;

        public LoadCellPrefab(ISpriteCache spriteCache, ICellInformationLoader cellInformationLoader)
        {
            _spriteCache = spriteCache;
            _cellInformationLoader = cellInformationLoader;
        }

        public MazeCellPrefab GetPrefab(Direction flag)
        {
            var resource = Resources.Load<MazeCellPrefab>("MazeCell");
            var prefab = UnityEngine.Object.Instantiate(resource);
            var cellInformation = _cellInformationLoader.LoadCellSpriteNames(flag);
            prefab.background.sprite = _spriteCache.GetSprite(cellInformation.Background);
            if (cellInformation.Up != null)
            {
                prefab.up.sprite = _spriteCache.GetSprite(cellInformation.Up);
            }
            if (cellInformation.Down != null)
            {
                prefab.down.sprite = _spriteCache.GetSprite(cellInformation.Down);
            }
            return prefab;
        }
    }
}
