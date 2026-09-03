---
name: find-public-domain-films
description: Research additional U.S. public-domain films with verified IMDb IDs and multiple playable archive.org sources per film for TheFilmArchive, deduplicated against the existing seed manifest. Use when asked to find more public-domain films, grow the seed list, or expand the archive's catalog.
---

# Find public-domain films for TheFilmArchive

Reproduces the research process used to build the original 211-film seed
manifest at `apps/api/Storage/seed-films.json`. Produces new candidate films
with verified IMDb IDs, PD status, and **at least two** working archive.org
source URLs each — with films already in the manifest (or already in the
live DB) filtered out automatically. Multiple sources per film matter: it
gives the film detail page's source list something real to show and gives
redundancy if one item gets taken down or turns out to be a partial/low-
quality copy.

## 0. Load the exclusion set first

Before spawning any research agents, build the set of films to exclude so
agents never waste time re-surfacing something already imported.

```bash
# IMDb IDs and TMDB IDs already in the seed manifest
jq -r '.[] | .imdbId // empty' apps/api/Storage/seed-films.json | sort -u > /tmp/existing_imdb_ids.txt
jq -r '.[] | .tmdbId // empty' apps/api/Storage/seed-films.json | sort -u > /tmp/existing_tmdb_ids.txt
jq -r '.[] | "\(.title)|\(.year)"' apps/api/Storage/seed-films.json | sort -u > /tmp/existing_title_year.txt
```

If a live database is reachable (real deployment, not this sandbox), also
pull films actually imported since the manifest was written — the manifest
only reflects what was *seeded*, not what an admin has added one-off via
"Add film" since:

```sql
SELECT ImdbId, TmdbId, Title, ReleaseYear FROM Films;
```

Merge any such rows into the same exclusion lists, then also merge in the
permanent blocklist below (these were manually pulled from the manifest by
the maintainer, so they won't show up as "already in the manifest" on
their own — they must be excluded explicitly regardless of PD status or
source quality):

```bash
cat >> /tmp/existing_imdb_ids.txt <<'EOF'
tt0182989
tt0058220
tt11843560
tt0401153
tt4560132
tt0142639
tt0148647
tt5226414
tt0002844
tt0026652
tt0159281
tt0151672
tt0260369
tt0260173
tt0038528
EOF
sort -u -o /tmp/existing_imdb_ids.txt /tmp/existing_imdb_ids.txt
```

| Title | Year | IMDb ID |
|---|---|---|
| Divide and Conquer | 1943 | tt0182989 |
| I Eat Your Skin | 1971 | tt0058220 |
| Let's Face It | 1954 | tt11843560 |
| About Fallout | 1963 | tt0401153 |
| The Time of Apollo | 1975 | tt4560132 |
| Making Good | 1932 | tt0142639 |
| Puss in Boots | 1934 | tt0148647 |
| The Flight of Apollo 11: Eagle Has Landed | 1969 | tt5226414 |
| Fantômas: In the Shadow of the Guillotine | 1913 | tt0002844 |
| The Lost City | 1935 | tt0026652 |
| Billy Mouse's Akwakade | 1940 | tt0159281 |
| The Museum (Toby the Pup) | 1930 | tt0151672 |
| Signal 30 | 1959 | tt0260369 |
| Mechanized Death | 1961 | tt0260173 |
| The Fleet That Came to Stay | 1945 | tt0038528 |

Hand the three `/tmp/existing_*.txt` files (or the merged list) to every
research agent in step 1 — each agent's own instructions must tell it to
check candidates against these files and drop any match before including
them in its output.

## 1. Derive a fresh category split, then fan out research agents

Don't reuse the same category list run after run — the point of a category
split is to partition the search space so parallel agents don't collide,
and a repeated list just re-plows ground already covered by earlier runs
and steers every run toward the same handful of well-known titles.
Regenerate the split each time from the manifest's current composition:

1. Tally what's already in `apps/api/Storage/seed-films.json` by rough era
   (decade), genre, and country/language of origin — the `title`/`year`
   fields plus a quick pass of well-known titles is enough, no need for
   exhaustive metadata.
2. Favor slices that look thin or absent over ones that are already dense
   — e.g. if silent-era and 1950s sci-fi are both heavily represented but
   there's little pre-1950 foreign cinema, serials, documentaries/newsreels,
   or industrial/educational shorts, weight the new split toward those.
3. Cut categories along more than one axis so the split stays encompassing
   rather than a rehash of the usual genre buckets — mix in angles like:
   era/decade, genre, country or language of origin, format (theatrical
   feature vs. short vs. serial vs. newsreel/documentary/industrial),
   studio or production context (e.g. race films, government/military
   productions, independent/regional productions), and notable
   directors/actors/franchises whose PD-eligible work hasn't been mined
   yet.
4. Size the split to the gaps found, not to a fixed number — use more than
   10 categories when the manifest is already broad and the remaining gaps
   are narrow and scattered, fewer when a handful of wide-open areas would
   cover more ground. Each category should still be narrow enough for one
   agent to search deeply rather than skim.
5. Always keep one general sweep / catch-all category to net anything the
   deliberately-targeted categories miss.

Spawn one agent per category in parallel. Give every agent the same brief
(fill in `{category}` and attach the
exclusion files from step 0):

> Research U.S. public-domain films in the category "{category}" for a
> film archive site. For each candidate:
>
> - **Verify PD status, don't assume it.** Cross-check against Wikipedia's
>   "List of films in the public domain in the United States" and/or
>   explicit rights metadata on the film's archive.org item page (e.g.
>   `"licenseurl"` or a stated public-domain notice in the metadata API at
>   `https://archive.org/metadata/{identifier}`). Note the specific reason
>   (e.g. "copyright not renewed", "published without notice",
>   "US govt work").
> - **Be skeptical of commonly mislabeled "PD" films.** Many films
>   circulating as "public domain" on cheap DVD compilations and even on
>   archive.org are not actually PD (underlying story/music rights still
>   held, foreign copyright restored under the URAA, or the claim is simply
>   wrong). Don't include a film solely because it's on archive.org — verify
>   independently.
> - **Verify the IMDb ID directly** against IMDb or Wikipedia's infobox —
>   never guess or pattern-match an ID.
> - **Find at least two working archive.org sources per film**, not just
>   one. For each candidate item, confirm it actually hosts a full
>   watchable copy (not just a trailer or partial reel) by checking its
>   `/metadata/{identifier}` file list for a full-length video file. Search
>   for alternate uploads of the same film (different identifiers,
>   restorations, transfers) rather than stopping at the first hit — this is
>   a hard target, not a nice-to-have. Only fall back to a single source if
>   you've genuinely searched and a second copy doesn't exist on
>   archive.org; call that out explicitly in the `Confidence` field (e.g.
>   "single-source, no second copy found") so it can be revisited later.
> - **TMDB ID is optional** — include it if easily found, otherwise leave
>   null; the sync step resolves it from the IMDb ID at import time.
> - **Check against the exclusion list before including anything** — skip
>   any film whose IMDb ID, TMDB ID, or (title, year) pair appears in the
>   provided exclusion files.
>
> Output each result as one CSV row:
> `Title,Year,Category,ImdbId,TmdbId,ArchiveOrgUrls,PdReason,Confidence`
> (semicolon-separate multiple archive.org URLs within the field). Find as
> many genuinely-verified candidates as you can; quality and correctness
> matter far more than volume.

Run these as background agents (`Agent` tool, `run_in_background: true` by
default) so they can work independently; collect their CSV output when each
reports back.

## 2. Compile and re-check for duplicates

Concatenate all agent output into one CSV, then run a second dedup pass —
agents can still overlap with each other even after excluding existing
films:

```python
import csv, json

with open('apps/api/Storage/seed-films.json') as f:
    existing = json.load(f)
existing_imdb = {e['imdbId'] for e in existing if e.get('imdbId')}
existing_tmdb = {str(e['tmdbId']) for e in existing if e.get('tmdbId')}
existing_title_year = {(e['title'].lower(), e['year']) for e in existing}

# Permanently blocked - pulled from the manifest by the maintainer, won't be
# caught by the checks above since they're no longer in seed-films.json.
existing_imdb |= {
    "tt0182989", "tt0058220", "tt11843560", "tt0401153", "tt4560132",
    "tt0142639", "tt0148647", "tt5226414", "tt0002844", "tt0026652",
    "tt0159281", "tt0151672", "tt0260369", "tt0260173", "tt0038528"
}

seen_imdb = set()
rows = []
with open('/tmp/claude-.../scratchpad/candidates.csv') as f:
    for row in csv.DictReader(f):
        imdb = row['ImdbId'].strip()
        tmdb = row['TmdbId'].strip()
        key = (row['Title'].strip().lower(), int(row['Year']))

        if imdb in existing_imdb or tmdb in existing_tmdb or key in existing_title_year:
            continue  # already in the archive
        if imdb in seen_imdb:
            continue  # duplicate across agents
        seen_imdb.add(imdb)
        rows.append(row)
```

Manually spot-check a sample (5-10 films) of what survives — re-verify PD
status and the archive.org URLs by hand for anything borderline before
trusting an agent's confidence rating. Flag (don't silently drop) any film
that only has one source — decide per-film whether to keep it as-is or send
it back for a second source before including it in the manifest.

## 3. Append to the seed manifest

Convert surviving rows into the manifest's schema and append (don't
overwrite):

```python
new_entries = [{
    "title": row["Title"].strip(),
    "year": int(row["Year"]),
    "imdbId": row["ImdbId"].strip() or None,
    "tmdbId": row["TmdbId"].strip() or None,
    "archiveUrls": [u.strip() for u in row["ArchiveOrgUrls"].split(";") if u.strip()],
} for row in rows]

with open('apps/api/Storage/seed-films.json') as f:
    existing = json.load(f)

existing.extend(new_entries)

with open('apps/api/Storage/seed-films.json', 'w') as f:
    json.dump(existing, f, indent=2)
```

## 4. Wrap up

- Report how many new films were found per category, how many were dropped
  as duplicates or failed PD verification, and how many landed with only a
  single source (these are the weakest entries — worth a follow-up pass).
- Commit `apps/api/Storage/seed-films.json` with a message describing the
  batch (e.g. category names and count added).
- The bulk-sync admin page picks up new manifest entries automatically on
  its next run — no code changes needed unless the manifest schema itself
  changes.
