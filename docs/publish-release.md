# How to release a new version

1. Create an annotated tag `-a` with a new version number. Replace
pattern between`{}` with the new version number.
```
git tag v{x.x.x}
```
This should open up vim and allow you to add some extra information about 
the current release. 

2. Push new tag to github
```
git push origin --tags
```

3. Pipeline is configured to automatically push release artifacts to nuget and github release page.
