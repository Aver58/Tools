#region Copyright © 2020 Aver. All rights reserved.
/*
=====================================================
 AverFrameWork v1.0
 Filename:    VisualStep.cs
 Author:      Zeng Zhiwei
 Time:        2021\1\20 星期三 22:51:03
=====================================================
*/
#endregion

namespace MyNamespace
{
    /// <summary>
    /// 算法步骤可视化
    /// </summary>
    public interface IVisualStep
    {
        void Execute();
    }

    public abstract class VisualStep : IVisualStep
    {
        protected GridItem _grid;
        protected PathFindManager _mgr;

        public VisualStep(GridItem tile, PathFindManager mgr)
        {
            _grid = tile;
            _mgr = mgr;
        }

        public abstract void Execute();
    }

    // 开始
    public class MarkStartTileStep : VisualStep
    {
        public MarkStartTileStep(GridItem tile, PathFindManager mgr) : base(tile, mgr)
        {
        }

        public override void Execute()
        {
            _grid.gridType = GridType.Start;
            _mgr.ResetGrid(_grid);
        }
    }

    // 结束
    public class MarkEndTileStep : VisualStep
    {
        public MarkEndTileStep(GridItem tile, PathFindManager mgr) : base(tile, mgr)
        {
        }

        public override void Execute()
        {
            _grid.gridType = GridType.End;
            _mgr.ResetGrid(_grid);
        }
    }

    // 路径
    public class MarkPathTileStep : VisualStep
    {
        public MarkPathTileStep(GridItem tile, PathFindManager mgr) : base(tile, mgr)
        {
        }

        public override void Execute()
        {
            _grid.gridType = GridType.Path;
            _mgr.ResetGrid(_grid);
        }
    }

    // 最外圈的环
    public class PushTileInFrontierStep : VisualStep
    {
        private int _cost;

        public PushTileInFrontierStep(GridItem tile, PathFindManager mgr, int cost) : base(tile, mgr)
        {
            _cost = cost;
        }

        public override void Execute()
        {
            _grid.gridType = GridType.Frontier;
            _mgr.ResetGrid(_grid);
            _grid.SetText(_cost != 0 ? _cost.ToString() : "");
        }
    }

    public class PushTileInOpenSetStep : VisualStep
    {
        public PushTileInOpenSetStep(GridItem tile, PathFindManager mgr) : base(tile, mgr)
        {
        }

        public override void Execute()
        {
            _grid.gridType = GridType.Frontier;
            _mgr.ResetGrid(_grid);
            _grid.SetText(string.Format("f:{0}\ng:{1}\nh:{2}",_grid.fCost, _grid.gCost, _grid.hCost));
        }
    }

    // 访问过的格子
    public class VisitedTileStep : VisualStep
    {
        public VisitedTileStep(GridItem tile, PathFindManager mgr) : base(tile, mgr)
        {
        }

        public override void Execute()
        {
            _grid.gridType = GridType.Visited;
            _mgr.ResetGrid(_grid);
        }
    }
}