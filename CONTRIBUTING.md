# Working in this repository

## Branching

`main` is always green and always releasable. Work happens on short-lived
feature branches and reaches `main` through a pull request.

```bash
git switch -c feature/short-description
# ... commits ...
git push -u origin feature/short-description
# open a PR into main, merge when CI is green
```

There are no release branches. See
[ADR 0006](docs/adr/0006-branching-and-versioning.md) for why.

## Versioning

Versions come from git tags via MinVer. You never edit a version number.

```bash
git switch main && git pull
git tag v1.0.0
git push origin v1.0.0     # this, and only this, publishes
```

An untagged commit builds as a prerelease of the next patch, for example
`1.0.1-alpha.0.7`. That is expected, and it is what `local-feed` gets during
development.

## Local verification

```bash
./build/verify.sh
```

That builds, tests, packs both package kinds, clears the JMCL NuGet cache, and
builds both sample consumers against the packed output at the version MinVer
computed. Run it rather than the individual steps: the cache clear is easy to
forget and silently makes the run meaningless.

The two samples are not in the solution on purpose: they consume JMCL as
outside consumers and cannot restore until the packages exist.

## Porting from upstream

Ported sources are generated, not hand-edited. Change the manifest in
`build/rename-upstream.py` and re-run it:

```bash
python3 build/rename-upstream.py --source ~/Repos/pp-itt
```

Hand-editing a file under `src/` that the porter owns means the next run
silently reverts your change.
