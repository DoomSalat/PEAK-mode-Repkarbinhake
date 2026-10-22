using System.Collections.Generic;
using NumericsVector3 = System.Numerics.Vector3;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class OwnRopeAim
    {
        private const float SharedStart = 0.6f;
        private const float HeadingReach = 1.5f;

        private readonly List<NumericsVector3> _trimmed = new List<NumericsVector3>();

        public IReadOnlyList<NumericsVector3> Trim(IReadOnlyList<NumericsVector3> points, bool fromStart)
        {
            _trimmed.Clear();
            int count = points.Count;

            if (count < 2)
                return _trimmed;

            float walked = 0f;
            NumericsVector3 previous = At(points, 0, fromStart);

            for (int i = 1; i < count; i++)
            {
                NumericsVector3 current = At(points, i, fromStart);
                float step = NumericsVector3.Distance(previous, current);

                if (_trimmed.Count == 0 && walked + step >= SharedStart)
                    _trimmed.Add(NumericsVector3.Lerp(previous, current, step > 0f ? (SharedStart - walked) / step : 0f));

                if (_trimmed.Count > 0)
                    _trimmed.Add(current);

                walked += step;
                previous = current;
            }

            return _trimmed;
        }

        public NumericsVector3 HeadingPoint()
        {
            float walked = 0f;

            for (int i = 1; i < _trimmed.Count; i++)
            {
                float step = NumericsVector3.Distance(_trimmed[i - 1], _trimmed[i]);

                if (walked + step >= HeadingReach)
                    return NumericsVector3.Lerp(_trimmed[i - 1], _trimmed[i], step > 0f ? (HeadingReach - walked) / step : 0f);

                walked += step;
            }

            return _trimmed[_trimmed.Count - 1];
        }

        private NumericsVector3 At(IReadOnlyList<NumericsVector3> points, int index, bool fromStart)
        {
            return fromStart ? points[index] : points[points.Count - 1 - index];
        }
    }
}
