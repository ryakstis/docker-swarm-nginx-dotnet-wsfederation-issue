# docker-swarm-nginx-dotnet-wsfederation-issue

To see the error this causes, run `docker-compose up` with Docker installed.

I have a feeling it has something to do with the `ForwardedHeadersOptions` setup listed [here](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-2.1#use-a-reverse-proxy-server).
