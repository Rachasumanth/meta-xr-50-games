using System;
using UnityEngine;

namespace MetaXRSim.App
{
    /// <summary>
    /// Immutable result of a gesture trace. Contains both the classification and
    /// enough geometric evidence for the UI to explain *why* a family was chosen
    /// (size, aspect, closure, confidence).
    /// </summary>
    public struct ShapeSignature
    {
        public ShapeKind Kind;
        public Vector3 CenterWorld;
        public Quaternion Basis;
        public Vector3 SizeMeters;
        public Vector3 PrincipalAxisWorld;
        public float DiameterMeters;
        public float AspectRatio;
        public float Closure;
        public float SweepDegrees;
        public float PlanarityRms;
        public float Confidence;
        public int PointCount;

        public bool IsUsable => Kind != ShapeKind.Unknown && PointCount >= 24 && DiameterMeters >= 0.015f;

        public string ShortSummary()
        {
            if (!IsUsable) return "No shape";
            return string.Format("{0}  {1:0} mm  conf {2:0}%",
                Kind.ToDisplayName(),
                DiameterMeters * 1000f,
                Mathf.RoundToInt(Confidence * 100f));
        }

        public static ShapeSignature Unknown(Vector3 center)
        {
            return new ShapeSignature
            {
                Kind = ShapeKind.Unknown,
                CenterWorld = center,
                Basis = Quaternion.identity,
                SizeMeters = Vector3.one * 0.05f,
                PrincipalAxisWorld = Vector3.up,
                DiameterMeters = 0.05f,
                AspectRatio = 1f,
                Closure = 0f,
                SweepDegrees = 0f,
                PlanarityRms = 1f,
                Confidence = 0f,
                PointCount = 0
            };
        }
    }

    /// <summary>
    /// Computes a stable air-gesture shape from a stream of fingertip samples.
    /// The implementation is pure C# (Unity math types only) so EditMode and
    /// PlayMode tests can drive it deterministically without a headset.
    ///
    /// Gesture grammar (also printed by the in-headset tutorial):
    ///   circle           -> true sphere
    ///   tall oval        -> prolate spheroid
    ///   wide / flat oval -> oblate spheroid
    ///   open arc cap  (intersection) -> hemisphere
    ///   open arc cup  (union)        -> domed bowl
    /// </summary>
    public sealed class ShapeGestureRecognizer
    {
        public const int MinPoints = 24;
        public const float MinSampleSpacingMeters = 0.0015f;
        public const float MaxTraceSeconds = 8f;
        public const float MinTraceSeconds = 0.35f;

        private readonly System.Collections.Generic.List<Vector3> _points = new System.Collections.Generic.List<Vector3>(256);
        private float _seconds;
        private float _lastSampleTime = -1f;
        private bool _active;

        public int PointCount => _points.Count;
        public bool IsActive => _active;
        public float DurationSeconds => _seconds;
        public float PathLengthMeters { get; private set; }

        public System.Collections.Generic.IReadOnlyList<Vector3> Points => _points;

        public void Begin()
        {
            _points.Clear();
            _seconds = 0f;
            _lastSampleTime = -1f;
            PathLengthMeters = 0f;
            _active = true;
        }

        public void Cancel()
        {
            _points.Clear();
            _active = false;
            _seconds = 0f;
            _lastSampleTime = -1f;
            PathLengthMeters = 0f;
        }

        /// <summary>
        /// Adds a sample. Returns false when the sample is a duplicate or the
        /// trace exceeded its time/size budget.
        /// </summary>
        public bool AddSample(Vector3 point, float now)
        {
            if (!_active) return false;

            if (_points.Count > 0)
            {
                float d = Vector3.Distance(point, _points[_points.Count - 1]);
                if (d < MinSampleSpacingMeters) return false;
                PathLengthMeters += d;
                if (PathLengthMeters > 8f) return false;
            }

            if (_lastSampleTime < 0f) _lastSampleTime = now;
            _seconds = Mathf.Max(0f, now - _lastSampleTime);
            if (_seconds > MaxTraceSeconds) return false;

            _points.Add(point);
            return true;
        }

        public ShapeSignature Complete(Vector3 fallbackCenter)
        {
            if (!_active) return ShapeSignature.Unknown(fallbackCenter);
            _active = false;

            if (_points.Count < MinPoints || _seconds < MinTraceSeconds)
                return ShapeSignature.Unknown(fallbackCenter);

            return Analyse(_points);
        }

        public ShapeSignature Peek(Vector3 fallbackCenter)
        {
            if (_points.Count < MinPoints) return ShapeSignature.Unknown(fallbackCenter);
            return Analyse(_points);
        }

        // ------------------------------------------------------------------
        // Geometry
        // ------------------------------------------------------------------
        public static ShapeSignature Analyse(System.Collections.Generic.IReadOnlyList<Vector3> pts)
        {
            int n = pts.Count;
            if (n < 3) return ShapeSignature.Unknown(n > 0 ? pts[0] : Vector3.zero);

            // Centroid.
            Vector3 centroid = Vector3.zero;
            for (int i = 0; i < n; i++) centroid += pts[i];
            centroid /= n;

            // 3x3 covariance.
            double[,] cov = new double[3, 3];
            for (int i = 0; i < n; i++)
            {
                double x = pts[i].x - centroid.x;
                double y = pts[i].y - centroid.y;
                double z = pts[i].z - centroid.z;
                cov[0, 0] += x * x; cov[1, 1] += y * y; cov[2, 2] += z * z;
                cov[0, 1] += x * y; cov[0, 2] += x * z; cov[1, 2] += y * z;
            }
            cov[0, 1] /= n; cov[0, 2] /= n; cov[1, 2] /= n;
            cov[1, 0] = cov[0, 1]; cov[2, 0] = cov[0, 2]; cov[2, 1] = cov[1, 2];

            double[] eigenvalues;
            double[,] eigenvectors;
            JacobiEigen(cov, out eigenvalues, out eigenvectors);

            // Sort descending.
            int[] order = { 0, 1, 2 };
            for (int i = 0; i < 2; i++)
                for (int j = i + 1; j < 3; j++)
                    if (eigenvalues[order[j]] > eigenvalues[order[i]])
                    {
                        int t = order[i]; order[i] = order[j]; order[j] = t;
                    }

            Vector3 axis0 = new Vector3((float)eigenvectors[0, order[0]], (float)eigenvectors[1, order[0]], (float)eigenvectors[2, order[0]]).normalized;
            Vector3 axis1 = new Vector3((float)eigenvectors[0, order[1]], (float)eigenvectors[1, order[1]], (float)eigenvectors[2, order[1]]).normalized;
            Vector3 normal = new Vector3((float)eigenvectors[0, order[2]], (float)eigenvectors[1, order[2]], (float)eigenvectors[2, order[2]]).normalized;
            if (axis0.sqrMagnitude < 1e-8f) axis0 = Vector3.right;
            if (axis1.sqrMagnitude < 1e-8f) axis1 = Vector3.forward;
            if (normal.sqrMagnitude < 1e-8f) normal = Vector3.Cross(axis0, axis1);
            if (Vector3.Dot(Vector3.Cross(axis0, axis1), normal) < 0f) normal = -normal;

            // Projected 2D coordinates in the best-fit plane.
            Vector2[] p2 = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                Vector3 d = pts[i] - centroid;
                p2[i] = new Vector2(Vector3.Dot(d, axis0), Vector3.Dot(d, axis1));
            }

            // 2D covariance / principal axis.
            Vector2 c2 = Vector2.zero;
            for (int i = 0; i < n; i++) c2 += p2[i];
            c2 /= n;
            double sxx = 0, syy = 0, sxy = 0;
            for (int i = 0; i < n; i++)
            {
                double x = p2[i].x - c2.x, y = p2[i].y - c2.y;
                sxx += x * x; syy += y * y; sxy += x * y;
            }
            sxx /= n; syy /= n; sxy /= n;
            double theta = 0.5 * Math.Atan2(2.0 * sxy, sxx - syy);
            Vector2 dir2 = new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta));
            Vector2 perp2 = new Vector2(-dir2.y, dir2.x);

            float minA = float.MaxValue, maxA = float.MinValue, minB = float.MaxValue, maxB = float.MinValue;
            for (int i = 0; i < n; i++)
            {
                float a = Vector2.Dot(p2[i] - c2, dir2);
                float b = Vector2.Dot(p2[i] - c2, perp2);
                minA = Mathf.Min(minA, a); maxA = Mathf.Max(maxA, a);
                minB = Mathf.Min(minB, b); maxB = Mathf.Max(maxB, b);
            }
            float extentA = Mathf.Max(0.0001f, maxA - minA);
            float extentB = Mathf.Max(0.0001f, maxB - minB);
            float extentLong = Mathf.Max(extentA, extentB);
            float extentShort = Mathf.Min(extentA, extentB);
            float aspect = extentLong / extentShort;

            // 2D circle fit (Kasa) for a robust diameter.
            double sumX = 0, sumY = 0, sumXX = 0, sumYY = 0, sumXY = 0, sumX3 = 0, sumY3 = 0, sumXY2 = 0, sumX2Y = 0;
            for (int i = 0; i < n; i++)
            {
                double x = p2[i].x, y = p2[i].y;
                sumX += x; sumY += y;
                sumXX += x * x; sumYY += y * y; sumXY += x * y;
                sumX3 += x * x * x; sumY3 += y * y * y; sumXY2 += x * y * y; sumX2Y += x * x * y;
            }
            double A = 2.0 * (sumXX - sumX * sumX / n);
            double B = 2.0 * (sumXY - sumX * sumY / n);
            double C = 2.0 * (sumYY - sumY * sumY / n);
            double D = (sumX3 + sumXY2 - (sumXX + sumYY) * sumX / n);
            double E = (sumY3 + sumX2Y - (sumXX + sumYY) * sumY / n);
            double det = A * C - B * B;
            float circleDiameter = extentLong;
            if (Math.Abs(det) > 1e-9)
            {
                double cx = (D * C - B * E) / det;
                double cy = (A * E - B * D) / det;
                double r2 = 0;
                for (int i = 0; i < n; i++)
                {
                    double dx = p2[i].x - cx, dy = p2[i].y - cy;
                    r2 += dx * dx + dy * dy;
                }
                float r = (float)Math.Sqrt(r2 / n);
                if (r > 0.005f && r < 3f) circleDiameter = r * 2f;
            }

            // In-plane RMS (planarity).
            float planeDev = 0f;
            for (int i = 0; i < n; i++)
            {
                float d = Vector3.Dot(pts[i] - centroid, normal);
                planeDev += d * d;
            }
            planeDev = Mathf.Sqrt(planeDev / n);

            // Path topology.
            float perimeter = 0f;
            for (int i = 1; i < n; i++) perimeter += Vector3.Distance(pts[i], pts[i - 1]);
            float gap = Vector3.Distance(pts[0], pts[n - 1]);
            float closure = perimeter > 1e-5f ? Mathf.Clamp01(gap / perimeter) : 1f;

            // Swept angle around the 2D centroid.
            float sweep = 0f;
            float prevAngle = 0f;
            for (int i = 0; i < n; i++)
            {
                Vector2 d = p2[i] - c2;
                float ang = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
                if (i > 0)
                {
                    float delta = Mathf.DeltaAngle(prevAngle, ang);
                    sweep += Mathf.Abs(delta);
                }
                prevAngle = ang;
            }
            if (sweep > 360f) sweep = 360f;

            // Curvature sign for an open arc: compare the arc midpoint height
            // above the straight chord.  ∩ is a dome, ∪ is a bowl.
            bool arcOpensDown = false;
            if (closure > 0.18f && sweep > 90f)
            {
                Vector3 a = pts[0];
                Vector3 b = pts[n - 1];
                Vector3 mid = pts[n / 2];
                float chordY = (a.y + b.y) * 0.5f;
                arcOpensDown = mid.y < chordY; // cup opens upward => bowl
            }

            // Longest silhouette axis in world space.
            Vector3 longAxisWorld = (axis0 * dir2.x + axis1 * dir2.y).normalized;
            float upAlignment = Mathf.Abs(Vector3.Dot(longAxisWorld, Vector3.up));

            // 3D extents (not used as the size metric but useful diagnostics).
            float e0 = 2f * Mathf.Sqrt(Mathf.Max(0f, (float)eigenvalues[order[0]])) * 2f;
            float e1 = 2f * Mathf.Sqrt(Mathf.Max(0f, (float)eigenvalues[order[1]])) * 2f;
            float e2 = 2f * Mathf.Sqrt(Mathf.Max(0f, (float)eigenvalues[order[2]])) * 2f;

            float diameter = Mathf.Max(circleDiameter, extentLong);
            diameter = Mathf.Clamp(diameter, 0.01f, 1.5f);

            bool closed = closure < 0.16f;
            ShapeKind kind = ShapeKind.Unknown;

            if (closed)
            {
                if (aspect < 1.16f)
                {
                    kind = ShapeKind.TrueSphere;
                }
                else if (aspect < 3.0f)
                {
                    // A closed oval: tall = prolate egg, flat = oblate pebble.
                    kind = upAlignment > 0.55f ? ShapeKind.ProlateSpheroid : ShapeKind.OblateSpheroid;
                }
                else
                {
                    kind = upAlignment > 0.55f ? ShapeKind.ProlateSpheroid : ShapeKind.OblateSpheroid;
                }
            }
            else if (sweep >= 130f && sweep <= 350f)
            {
                kind = arcOpensDown ? ShapeKind.DomedBowl : ShapeKind.Hemisphere;
            }
            else if (sweep > 350f)
            {
                // Nearly full revolution with a small gap.
                kind = aspect < 1.16f ? ShapeKind.TrueSphere :
                    (upAlignment > 0.55f ? ShapeKind.ProlateSpheroid : ShapeKind.OblateSpheroid);
            }

            float sizeQuality = Mathf.Clamp01(1f - Mathf.Abs(diameter - 0.10f) / 0.30f);
            float planarityQuality = Mathf.Clamp01(1f - planeDev / Mathf.Max(0.02f, diameter * 0.6f));
            float closureQuality = closed ? Mathf.Clamp01(1f - closure / 0.16f)
                                          : Mathf.Clamp01((sweep - 110f) / 120f);
            float pointQuality = Mathf.Clamp01((n - MinPoints) / 80f);
            float confidence = Mathf.Clamp01(0.30f + 0.30f * closureQuality + 0.20f * planarityQuality +
                                             0.15f * pointQuality + 0.05f * sizeQuality);
            if (kind == ShapeKind.Unknown) confidence *= 0.25f;

            Quaternion basis = Quaternion.LookRotation(normal, axis0);
            Vector3 size = new Vector3(diameter, diameter, Mathf.Max(diameter * 0.6f, e2));

            return new ShapeSignature
            {
                Kind = kind,
                CenterWorld = centroid,
                Basis = basis,
                SizeMeters = new Vector3(Mathf.Max(extentA, extentB), Mathf.Max(extentA, extentB), Mathf.Max(0.004f, planeDev * 4f)),
                PrincipalAxisWorld = longAxisWorld,
                DiameterMeters = diameter,
                AspectRatio = aspect,
                Closure = closure,
                SweepDegrees = sweep,
                PlanarityRms = planeDev,
                Confidence = confidence,
                PointCount = n
            };
        }

        // ------------------------------------------------------------------
        // Symmetric 3x3 Jacobi eigen decomposition.
        // ------------------------------------------------------------------
        public static void JacobiEigen(double[,] input, out double[] eigenvalues, out double[,] eigenvectors)
        {
            double[,] a = (double[,])input.Clone();
            double[,] v = new double[3, 3];
            for (int i = 0; i < 3; i++) v[i, i] = 1.0;

            for (int sweep = 0; sweep < 32; sweep++)
            {
                double off = Math.Abs(a[0, 1]) + Math.Abs(a[0, 2]) + Math.Abs(a[1, 2]);
                if (off < 1e-12) break;

                for (int p = 0; p < 2; p++)
                {
                    for (int q = p + 1; q < 3; q++)
                    {
                        if (Math.Abs(a[p, q]) < 1e-15) continue;
                        double theta = (a[q, q] - a[p, p]) / (2.0 * a[p, q]);
                        double t = Math.Sign(theta) / (Math.Abs(theta) + Math.Sqrt(theta * theta + 1.0));
                        if (Math.Abs(theta) < 1e-15) t = 1.0;
                        double c = 1.0 / Math.Sqrt(t * t + 1.0);
                        double s = t * c;

                        for (int k = 0; k < 3; k++)
                        {
                            double akp = a[k, p], akq = a[k, q];
                            a[k, p] = c * akp - s * akq;
                            a[k, q] = s * akp + c * akq;
                        }
                        for (int k = 0; k < 3; k++)
                        {
                            double apk = a[p, k], aqk = a[q, k];
                            a[p, k] = c * apk - s * aqk;
                            a[q, k] = s * apk + c * aqk;
                        }
                        for (int k = 0; k < 3; k++)
                        {
                            double vkp = v[k, p], vkq = v[k, q];
                            v[k, p] = c * vkp - s * vkq;
                            v[k, q] = s * vkp + c * vkq;
                        }
                    }
                }
            }

            eigenvalues = new double[3];
            for (int i = 0; i < 3; i++) eigenvalues[i] = a[i, i];
            eigenvectors = v;
        }
    }
}
