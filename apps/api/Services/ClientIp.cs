namespace Api.Services;

// api.thefilmarchive.org sits behind Cloudflare, so Connection.RemoteIpAddress is
// a Cloudflare edge address - rate limiting on that would put every visitor in
// the world into a handful of buckets. CF-Connecting-IP carries the real caller.
//
// IMPORTANT: these headers are only trustworthy for traffic that actually came
// through Cloudflare. If the EC2 origin is reachable directly, anyone can set
// CF-Connecting-IP to a random value per request and walk straight past the
// per-IP limit. Keep the origin's security group restricted to Cloudflare's
// published ranges - the limiter's usefulness depends on it.
public static class ClientIp
{
    public static string Resolve(HttpContext context)
    {
        string? cloudflare = context.Request.Headers["CF-Connecting-IP"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(cloudflare))
            return cloudflare.Trim();

        string? forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(forwarded))
        {
            // Left-most entry is the original client; the rest are proxy hops.
            string first = forwarded.Split(',')[0].Trim();

            if (first.Length > 0)
                return first;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
