# giteaTime

## notizen allgemein

```
wget https://repo1.maven.org/maven2/io/swagger/swagger-codegen-cli/2.4.15/swagger-codegen-cli-2.4.15.jar -O swagger-codegen-cli.jar
java -jar swagger-codegen-cli.jar generate -i http://localhost:3000/swagger.v1.json -l csharp -o cs/
nuget install Newtonsoft.Json
msbuild giteaTime.sln
mono bin/giteaTime.exe
```