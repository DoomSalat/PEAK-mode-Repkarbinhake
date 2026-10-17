using System;
using System.Collections.Generic;
using System.Numerics;

namespace Repkarbinhake.Core.Visual
{
    public sealed class CoilShape
    {
        private const float FullTurn = MathF.PI * 2f;
        private const int PointsPerTurn = 10;

        private readonly float _radius;
        private readonly float _maxTurns;
        private readonly float _dropPerTurn;

        public CoilShape(float radius, float maxTurns, float dropPerTurn)
        {
            _radius = radius;
            _maxTurns = maxTurns;
            _dropPerTurn = dropPerTurn;
        }

        public float TurnsFor(float reel)
        {
            return Math.Clamp(reel / (FullTurn * _radius), 0f, _maxTurns);
        }

        public void Build(Vector3 top, Vector3 side, Vector3 up, float reel, List<Vector3> points)
        {
            points.Clear();
            float turns = TurnsFor(reel);
            int count = Math.Max((int)MathF.Ceiling(turns * PointsPerTurn), 0) + 1;

            if (count < 2)
                return;

            Vector3 center = top - up * _radius;

            for (int i = 0; i < count; i++)
            {
                float turn = turns * i / (count - 1);
                float angle = turn * FullTurn;
                Vector3 ring = (up * MathF.Cos(angle) + side * MathF.Sin(angle)) * _radius;
                points.Add(center + ring - up * (_dropPerTurn * turn));
            }
        }
    }
}
