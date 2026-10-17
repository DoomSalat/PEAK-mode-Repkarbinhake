using System;
using System.Collections.Generic;
using System.Numerics;
using Repkarbinhake.Core.Physics;

namespace Repkarbinhake.Core.Visual
{
    public sealed class RopeWrapPath
    {
        public const int MaxContacts = 24;
        public const float ContactLift = 2f;
        public const float TautTolerance = 0.01f;
        public const float MaxEndJump = 3f;

        private const float MinGap = 2f;
        private const int EdgeSearchSteps = 10;
        private const float SlackFlowRate = 4f;
        private const float MinMaterial = 0.01f;
        private const float FollowRate = 10f;
        private const float ClimbFriction = 0.36f;
        private const float ClimbSpeed = 2f;
        private const float ClimbClearance = 0.5f;

        private static readonly Vector3[] ClimbBlends = // Static: shared direction table (forward, up, side)
        {
            new Vector3(1f, 1f, 0f),
            new Vector3(1f, 1f, 1f),
            new Vector3(1f, 1f, -1f),
            new Vector3(0f, 1f, 1f),
            new Vector3(0f, 1f, -1f),
            new Vector3(1f, 2f, 0f),
            new Vector3(1f, 0f, 0f),
        };

        private readonly List<Vector3> _points = new List<Vector3>();
        private readonly List<float> _materials = new List<float>();
        private readonly List<Vector3> _remoteContacts = new List<Vector3>();
        private readonly List<float> _remoteMaterials = new List<float>();
        private Vector3 _lastStart;
        private Vector3 _lastEnd;
        private float _lastFirstReel;
        private float _lastSecondReel;
        private int _climbTurn;

        public IReadOnlyList<Vector3> Points => _points;
        public IReadOnlyList<float> Materials => _materials;
        public int ContactCount => _points.Count < 2 ? 0 : _points.Count - 2;
        public int Version { get; private set; }
        public bool IsFollowing { get; private set; }

        public void Update(Vector3 start, Vector3 end, RopeLengthState length, float radius, float deltaTime, IRopeObstacles obstacles)
        {
            if (_points.Count < 2 || Vector3.Distance(start, _lastStart) > MaxEndJump || Vector3.Distance(end, _lastEnd) > MaxEndJump)
                Begin(start, end, length);

            _points[0] = start;
            _points[_points.Count - 1] = end;

            if (IsFollowing)
                FollowRemote(length, deltaTime);
            else
                Simulate(length, radius, deltaTime, obstacles);

            _lastStart = start;
            _lastEnd = end;
            _lastFirstReel = length.FirstReel;
            _lastSecondReel = length.SecondReel;
        }

        public void Follow(IReadOnlyList<Vector3> contacts, IReadOnlyList<float> materials)
        {
            _remoteContacts.Clear();
            _remoteContacts.AddRange(contacts);
            _remoteMaterials.Clear();
            _remoteMaterials.AddRange(materials);
            IsFollowing = true;
        }

        public void Lead()
        {
            IsFollowing = false;
        }

        public float Chord(int segment)
        {
            return Vector3.Distance(_points[segment], _points[segment + 1]);
        }

        public bool IsTaut(int segment)
        {
            return _materials[segment] <= Chord(segment) * (1f + TautTolerance);
        }

        public float PathLength()
        {
            float total = 0f;

            for (int i = 1; i < _points.Count; i++)
                total += Vector3.Distance(_points[i - 1], _points[i]);

            return total;
        }

        public bool TrySpan(bool fromStart, out RopeSpan span)
        {
            span = default;

            if (_points.Count < 2)
                return false;

            int last = _points.Count - 1;
            Vector3 self = fromStart ? _points[0] : _points[last];
            Vector3 next = fromStart ? _points[1] : _points[last - 1];
            Vector3 offset = next - self;
            float offsetLength = offset.Length();

            if (offsetLength < float.Epsilon)
                return false;

            span = new RopeSpan(offset / offsetLength, PathLength());
            return true;
        }

        public void Clear()
        {
            _points.Clear();
            _materials.Clear();
        }

        private void Simulate(RopeLengthState length, float radius, float deltaTime, IRopeObstacles obstacles)
        {
            ApplyReel(length);
            Climb(radius, deltaTime, obstacles);
            Unwrap(radius, obstacles);
            WrapStart(radius, obstacles);
            WrapEnd(radius, obstacles);
            FlowSlack(deltaTime);
        }

        private void FollowRemote(RopeLengthState length, float deltaTime)
        {
            Vector3 start = _points[0];
            Vector3 end = _points[_points.Count - 1];

            if (ContactCount != _remoteContacts.Count)
            {
                _points.Clear();
                _points.Add(start);
                _points.AddRange(_remoteContacts);
                _points.Add(end);
                Version++;
            }
            else
            {
                float share = Math.Clamp(FollowRate * deltaTime, 0f, 1f);

                for (int i = 0; i < _remoteContacts.Count; i++)
                    _points[i + 1] = Vector3.Lerp(_points[i + 1], _remoteContacts[i], share);
            }

            _materials.Clear();

            if (_remoteMaterials.Count == _points.Count - 1)
                _materials.AddRange(_remoteMaterials);
            else
                SpreadByChords(length.Total);

            int last = _materials.Count - 1;
            _materials[last] = MathF.Max(_materials[last] + length.Total - Sum(), MinMaterial);
        }

        private void Climb(float radius, float deltaTime, IRopeObstacles obstacles)
        {
            float lift = radius * ContactLift;
            Vector3 blend = ClimbBlends[_climbTurn];
            _climbTurn = (_climbTurn + 1) % ClimbBlends.Length;

            for (int i = 1; i < _points.Count - 1; i++)
            {
                if (!IsTaut(i - 1) || !IsTaut(i))
                    continue;

                Vector3 contact = _points[i];
                Vector3 pull = Direction(contact, _points[i - 1]) + Direction(contact, _points[i + 1]);

                if (pull.LengthSquared() < float.Epsilon)
                    continue;

                Vector3 forward = Vector3.Normalize(pull);
                Vector3 side = Vector3.Cross(forward, Vector3.UnitY);

                if (side.LengthSquared() < float.Epsilon)
                    continue;

                Vector3 direction = Vector3.Normalize(forward * blend.X + Vector3.UnitY * blend.Y + Vector3.Normalize(side) * blend.Z);
                Vector3 candidate = obstacles.Resolve(contact, contact + direction * ClimbSpeed * deltaTime, lift);

                if (CanClimb(i, contact, candidate, pull.Length(), radius, obstacles))
                    _points[i] = candidate;
            }
        }

        private bool CanClimb(int index, Vector3 contact, Vector3 candidate, float pull, float radius, IRopeObstacles obstacles)
        {
            float moved = Vector3.Distance(contact, candidate);

            if (moved < float.Epsilon || candidate.Y < contact.Y)
                return false;

            float drive = (SpanThrough(index, contact) - SpanThrough(index, candidate)) / moved;
            float pressure = MathF.Sqrt(MathF.Max(pull * pull - drive * drive, 0f));

            if (drive <= ClimbFriction * pressure)
                return false;

            float clearance = radius * ClimbClearance;
            return !obstacles.TryHit(_points[index - 1], candidate, clearance, out _, out _) && !obstacles.TryHit(candidate, _points[index + 1], clearance, out _, out _);
        }

        private void SpreadByChords(float total)
        {
            float chords = PathLength();

            for (int segment = 0; segment < _points.Count - 1; segment++)
                _materials.Add(chords < float.Epsilon ? total / (_points.Count - 1) : total * Chord(segment) / chords);
        }

        private void Begin(Vector3 start, Vector3 end, RopeLengthState length)
        {
            _points.Clear();
            _materials.Clear();
            _points.Add(start);
            _points.Add(end);
            _materials.Add(length.Total);
            _lastStart = start;
            _lastEnd = end;
            _lastFirstReel = length.FirstReel;
            _lastSecondReel = length.SecondReel;
            Version++;
        }

        private void ApplyReel(RopeLengthState length)
        {
            int last = _materials.Count - 1;
            Take(last, -1, length.FirstReel - _lastFirstReel);
            Take(0, 1, length.SecondReel - _lastSecondReel);
            _lastFirstReel = length.FirstReel;
            _lastSecondReel = length.SecondReel;

            float drift = length.Total - Sum();

            if (MathF.Abs(drift) > MinMaterial)
                Take(last, -1, -drift);
        }

        private void Take(int segment, int inward, float amount)
        {
            if (amount < 0f)
            {
                _materials[segment] -= amount;
                return;
            }

            for (int i = segment; i >= 0 && i < _materials.Count && amount > 0f; i += inward)
            {
                float taken = MathF.Min(amount, MathF.Max(_materials[i] - MinMaterial, 0f));
                _materials[i] -= taken;
                amount -= taken;
            }
        }

        private void FlowSlack(float deltaTime)
        {
            float share = Math.Clamp(SlackFlowRate * deltaTime, 0f, 1f);

            for (int contact = 1; contact < _points.Count - 1; contact++)
            {
                int before = contact - 1;

                if (IsTaut(before) && IsTaut(contact))
                    continue;

                float chordBefore = Chord(before);
                float chordAfter = Chord(contact);
                float chords = chordBefore + chordAfter;

                if (chords < float.Epsilon)
                    continue;

                float total = _materials[before] + _materials[contact];
                float wanted = total * chordBefore / chords;
                float moved = (wanted - _materials[before]) * share;
                _materials[before] += moved;
                _materials[contact] -= moved;
            }
        }

        private void Unwrap(float radius, IRopeObstacles obstacles)
        {
            int i = 1;

            while (i < _points.Count - 1)
            {
                if (obstacles.TryHit(_points[i - 1], _points[i + 1], radius, out _, out _))
                {
                    i++;
                    continue;
                }

                _materials[i - 1] += _materials[i];
                _materials.RemoveAt(i);
                _points.RemoveAt(i);
                Version++;
                i = 1;
            }
        }

        private void WrapStart(float radius, IRopeObstacles obstacles)
        {
            while (ContactCount < MaxContacts)
            {
                Vector3 oldEnd = ContactCount == 0 ? _lastEnd : _points[1];

                if (!TryFindEdge(_lastStart, oldEnd, _points[0], _points[1], radius, obstacles, out Vector3 contact) || IsCrowded(contact, 0, 1, radius))
                    return;

                Split(1, contact);
            }
        }

        private void WrapEnd(float radius, IRopeObstacles obstacles)
        {
            while (ContactCount < MaxContacts)
            {
                int last = _points.Count - 1;

                if (!TryFindEdge(_points[last - 1], _lastEnd, _points[last - 1], _points[last], radius, obstacles, out Vector3 contact) || IsCrowded(contact, last - 1, last, radius))
                    return;

                Split(last, contact);
            }
        }

        private void Split(int index, Vector3 contact)
        {
            int segment = index - 1;
            float before = Vector3.Distance(_points[segment], contact);
            float after = Vector3.Distance(contact, _points[index]);
            float material = _materials[segment];
            float share = before + after < float.Epsilon ? 0.5f : before / (before + after);
            _points.Insert(index, contact);
            _materials[segment] = material * share;
            _materials.Insert(index, material * (1f - share));
            Version++;
        }

        private bool TryFindEdge(Vector3 oldFrom, Vector3 oldTo, Vector3 from, Vector3 to, float radius, IRopeObstacles obstacles, out Vector3 contact)
        {
            contact = Vector3.Zero;

            if (!obstacles.TryHit(from, to, radius, out Vector3 point, out Vector3 normal))
                return false;

            Vector3 away = normal;

            if (!obstacles.TryHit(oldFrom, oldTo, radius, out _, out _))
            {
                float clear = 0f;
                float blocked = 1f;

                for (int step = 0; step < EdgeSearchSteps; step++)
                {
                    float middle = (clear + blocked) * 0.5f;

                    if (obstacles.TryHit(Vector3.Lerp(oldFrom, from, middle), Vector3.Lerp(oldTo, to, middle), radius, out Vector3 edgePoint, out Vector3 edgeNormal))
                    {
                        blocked = middle;
                        point = edgePoint;
                        normal = edgeNormal;
                    }
                    else
                    {
                        clear = middle;
                    }
                }

                Vector3 free = ClosestOnSegment(Vector3.Lerp(oldFrom, from, clear), Vector3.Lerp(oldTo, to, clear), point) - point;
                away = free.LengthSquared() > float.Epsilon ? Vector3.Normalize(free) : normal;
            }

            Vector3 lifted = point + away * (radius * ContactLift);
            contact = obstacles.Resolve(lifted, lifted, radius * ContactLift);
            return true;
        }

        private bool IsCrowded(Vector3 contact, int before, int after, float radius)
        {
            float gap = radius * MinGap;
            return Vector3.Distance(contact, _points[before]) < gap || Vector3.Distance(contact, _points[after]) < gap;
        }

        private float Sum()
        {
            float total = 0f;

            foreach (float material in _materials)
                total += material;

            return total;
        }

        private float SpanThrough(int contact, Vector3 position)
        {
            return Vector3.Distance(_points[contact - 1], position) + Vector3.Distance(position, _points[contact + 1]);
        }

        private static Vector3 Direction(Vector3 from, Vector3 to) // Static: pure geometry helper
        {
            Vector3 offset = to - from;
            return offset.LengthSquared() < float.Epsilon ? Vector3.Zero : Vector3.Normalize(offset);
        }

        private static Vector3 ClosestOnSegment(Vector3 a, Vector3 b, Vector3 point) // Static: pure geometry helper
        {
            Vector3 ab = b - a;
            float lengthSquared = ab.LengthSquared();

            if (lengthSquared < float.Epsilon)
                return a;

            float t = Math.Clamp(Vector3.Dot(point - a, ab) / lengthSquared, 0f, 1f);
            return a + ab * t;
        }
    }
}
