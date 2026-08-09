namespace Waypoint.Community.Domain;

/// <summary>
/// Phase 7 scope decision: the docs sketch a 4-tier model (private/followers/community/public)
/// but no follows/followers table exists anywhere — that tier referenced a social graph that was
/// never designed. Shipping 3 tiers instead; Public is schema-ready but behaves identically to
/// Community today since there's no unauthenticated/external sharing surface yet either.
/// </summary>
public enum PostVisibility { Private, Community, Public }
