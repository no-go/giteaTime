# giteaTime (still alpha)

A cSharp Windows/Linux taskmenu icon app to display running your gitea stopwatches.

## some notes

```
wget https://repo1.maven.org/maven2/io/swagger/swagger-codegen-cli/2.4.15/swagger-codegen-cli-2.4.15.jar -O swagger-codegen-cli.jar
java -jar swagger-codegen-cli.jar generate -i http://localhost:3000/swagger.v1.json -l csharp -o cs/

nuget install Newtonsoft.Json
msbuild giteaTime.sln
mono bin/giteaTime.exe http://localhost:3000/ app........token
```

the swagger stuff is not used but nice to see!

## gitea 1.13 patch

[some changes for more details via git API](gitea.patch)

## security

http is bad. And with user token you can do a lot!!
