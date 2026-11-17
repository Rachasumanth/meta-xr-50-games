using System;
using System.Collections.Generic;
using UnityEngine;

namespace MetaXRSim.App
{
    public struct GameMatch
    {
        public GameDefinition Game;
        public float Score;
        public string Reason;
        public ShapeKind ResolvedShape;
        public GameFamily ResolvedFamily;

        public bool HasGame => Game != null;
    }

    /// <summary>
    /// Resolves (gesture + real object) -> one of the 50 mini-games. Scoring is
    /// deterministic for a given seed so QA can reproduce a selection, while a
    /// per-session seed still rotates between equally valid games.
    /// </summary>
    public sealed class GameMatchmaker
    {
        public const float SpherePinchPalmBoundaryMeters = 0.085f;
        public const float DomePinchHandBoundaryMeters = 0.155f;

        public GameMatch Match(ShapeSignature shape, RealObjectProfile realObject, int seed = 0)
        {
            var ranked = Rank(shape, realObject, seed, 3);
            if (ranked.Count == 0) return default;
            var best = ranked[0];
            var bestDef = best.Game;
            var match = new GameMatch
            {
                Game = bestDef,
                Score = best.Score,
                Reason = best.Reason
            };
            ResolveFamily(shape, realObject, out match.ResolvedShape, out match.ResolvedFamily);
            return match;
        }

        public List<GameMatch> Rank(ShapeSignature shape, RealObjectProfile realObject, int seed, int count)
        {
            ShapeKind resolved;
            GameFamily family;
            ResolveFamily(shape, realObject, out resolved, out family);
            if (family == (GameFamily)(-1))
                return new List<GameMatch>();

            float physicalSize = PhysicalSizeMeters(shape, realObject);
            string label = (realObject.HasObject ? realObject.Label : string.Empty) ?? string.Empty;
            label = label.ToLowerInvariant();

            var all = GameCatalog.ByFamily(family);
            var scored = new List<GameMatch>(all.Count);
            foreach (var game in all)
            {
                float score = 0f;

                // Size mismatch in log space keeps 20mm and 40mm close while
                // separating 20mm from 200mm strongly.
                float nominal = Mathf.Max(0.001f, game.NominalSizeMeters);
                score += Mathf.Abs(Mathf.Log(Mathf.Max(0.001f, physicalSize) / nominal)) * 2.4f;

                // Sub-shape affinity.
                if (resolved.IsSpheroid())
                {
                    if (game.Shape != resolved) score += 1.1f;
                }
                else if (resolved.IsDome())
                {
                    if (resolved == ShapeKind.DomedBowl && game.Shape != ShapeKind.DomedBowl) score += 0.55f;
                    if (resolved == ShapeKind.Hemisphere && game.Shape != ShapeKind.Hemisphere) score += 0.55f;
                }

                // Real object semantics: a table prefers a dome/bowl host, a
                // couch prefers spheroids, a ball prefers spheres.
                if (realObject.IsValid && !string.IsNullOrEmpty(label))
                {
                    if (game.RealObjectKeywords != null)
                    {
                        for (int k = 0; k < game.RealObjectKeywords.Length; k++)
                        {
                            string kw = game.RealObjectKeywords[k];
                            if (!string.IsNullOrEmpty(kw) && label.Contains(kw.ToLowerInvariant()))
                            {
                                score -= 0.75f;
                                break;
                            }
                        }
                    }
                }

                // Confident gestures are trusted more; low confidence keeps the
                // family but strongly prefers the family's median-size games.
                if (shape.IsUsable && shape.Confidence < 0.45f)
                {
                    score += Mathf.Abs(game.NominalSizeMm - FamilyMedianMm(family)) / 500f;
                }

                // Stable pseudo-random tie-break so repeated gestures at the
                // same size do not always select the same title.
                uint h = Hash(game.Slug, seed);
                score += (h % 1000) / 1000f * 0.18f;

                scored.Add(new GameMatch
                {
                    Game = game,
                    Score = score,
                    ResolvedShape = resolved,
                    ResolvedFamily = family,
                    Reason = BuildReason(shape, realObject, game, physicalSize)
                });
            }

            scored.Sort((a, b) => a.Score.CompareTo(b.Score));
            if (count > 0 && scored.Count > count) scored.RemoveRange(count, scored.Count - count);
            return scored;
        }

        public void ResolveFamily(ShapeSignature shape, RealObjectProfile obj,
            out ShapeKind resolvedShape, out GameFamily family)
        {
            resolvedShape = shape.Kind;
            float size = PhysicalSizeMeters(shape, obj);

            if (resolvedShape == ShapeKind.Unknown && obj.IsValid)
                resolvedShape = obj.InferShape();

            switch (resolvedShape)
            {
                case ShapeKind.TrueSphere:
                    family = size < SpherePinchPalmBoundaryMeters ? GameFamily.SpherePinch : GameFamily.SpherePalm;
                    break;
                case ShapeKind.ProlateSpheroid:
                case ShapeKind.OblateSpheroid:
                    family = GameFamily.Spheroid;
                    break;
                case ShapeKind.Hemisphere:
                case ShapeKind.DomedBowl:
                    family = size < DomePinchHandBoundaryMeters ? GameFamily.HemispherePinch : GameFamily.HemisphereHand;
                    break;
                default:
                    family = (GameFamily)(-1);
                    break;
            }

            // A large real object always promotes to the hand-scale family,
            // even when a small air gesture was drawn nearby.
            if (obj.IsValid)
            {
                float objectSize = obj.MaxDimension;
                if (family == GameFamily.SpherePinch && objectSize >= SpherePinchPalmBoundaryMeters)
                    family = GameFamily.SpherePalm;
                if (family == GameFamily.HemispherePinch && objectSize >= DomePinchHandBoundaryMeters)
                    family = GameFamily.HemisphereHand;
            }
        }

        public static float PhysicalSizeMeters(ShapeSignature shape, RealObjectProfile obj)
        {
            float s = shape.IsUsable ? shape.DiameterMeters : 0.10f;
            if (obj.IsValid)
            {
                // Weighted blend; measured scene bounds are more trustworthy for
                // big objects, the gesture is more precise for small ones.
                float measured = obj.MaxDimension;
                float weight = Mathf.Clamp01(measured / 0.25f) * 0.75f;
                s = Mathf.Lerp(s, measured, weight);
            }
            return Mathf.Clamp(s, 0.015f, 0.60f);
        }

        private static float FamilyMedianMm(GameFamily family)
        {
            switch (family)
            {
                case GameFamily.SpherePinch: return 55f;
                case GameFamily.SpherePalm: return 135f;
                case GameFamily.Spheroid: return 155f;
                case GameFamily.HemispherePinch: return 100f;
                case GameFamily.HemisphereHand: return 240f;
                default: return 100f;
            }
        }

        private static string BuildReason(ShapeSignature shape, RealObjectProfile obj, GameDefinition game, float size)
        {
            if (obj.IsValid)
                return string.Format("{0} ({1:0} mm) -> {2}", obj.Label, size * 1000f, game.Title);
            if (shape.IsUsable)
                return string.Format("{0} gesture ({1:0} mm) -> {2}", shape.Kind.ToDisplayName(), size * 1000f, game.Title);
            return "Selected " + game.Title;
        }

        private static uint Hash(string slug, int seed)
        {
            unchecked
            {
                uint h = 2166136261u ^ (uint)seed;
                for (int i = 0; i < slug.Length; i++)
                {
                    h ^= slug[i];
                    h *= 16777619u;
                }
                return h;
            }
        }
    }
}
