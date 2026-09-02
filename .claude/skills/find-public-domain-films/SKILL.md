---
name: find-public-domain-films
description: Research additional U.S. public-domain films with verified IMDb IDs and playable archive.org sources for TheFilmArchive, deduplicated against the existing seed manifest. Use when asked to find more public-domain films, grow the seed list, or expand the archive's catalog.
---

# Find public-domain films for TheFilmArchive

Reproduces the research process used to build the original 211-film seed
manifest at `apps/api/Storage/seed-films.json`. Produces new candidate films
with verified IMDb IDs, PD status, and at least one working archive.org
source URL — with films already in the manifest (or already in the live DB)
filtered out automatically.

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

Merge any such rows into the same exclusion lists. Hand the three
`/tmp/existing_*.txt` files (or the merged list) to every research agent in
step 1 — each agent's own instructions must tell it to check candidates
against these files and drop any match before including them in its output.

## 1. Fan out research agents by category

Spawn agents in parallel (10 worked well previously), one per category, so
each can go deep without duplicating another's search space. Categories
used before, adjust to taste:

1. Silent era (pre-1930)
2. 1930s-40s horror / sci-fi
3. Film noir / crime
4. 1950s-60s sci-fi & monster B-movies
5. Comedy / screwball
6. Westerns
7. Animated shorts and features
8. Drama / literary adaptations
9. Foreign / international classics
10. General sweep / catch-all (anything missed by the above)

Give every agent the same brief (fill in `{category}` and attach the
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
> - **Find a working archive.org source.** Confirm the item actually hosts
>   a full watchable copy (not just a trailer or partial reel) by checking
>   its `/metadata/{identifier}` file list for a full-length video file.
>   Prefer the highest-quality copy if multiple items exist for the same
>   film.
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
status and the archive.org URL by hand for anything borderline before
trusting an agent's confidence rating.

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

- Report how many new films were found per category and how many were
  dropped as duplicates or failed PD verification.
- Commit `apps/api/Storage/seed-films.json` with a message describing the
  batch (e.g. category names and count added).
- The bulk-sync admin page picks up new manifest entries automatically on
  its next run — no code changes needed unless the manifest schema itself
  changes.
