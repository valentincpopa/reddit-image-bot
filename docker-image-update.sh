#!/usr/bin/bash

raw_token=$(curl https://ghcr.io/token\?scope\="repository:valentincpopa/reddit-image-bot:pull")

token=$(echo $raw_token | sed 's/.*token":"\(.*\)".*/\1/')

remote_digest=$(curl -H "Authorization: Bearer $token" https://ghcr.io/v2/valentincpopa/reddit-image-bot/manifests/release-1.0.0 | sed 's/.*sha256:\(.*\)".*/\1/' | sed '7q;d')

local_digest=$(docker inspect --format="{{.Id}}" ghcr.io/valentincpopa/reddit-image-bot:release-1.0.0 | sed 's/.*sha256:\(.*\).*/\1/')
#local_digest=$(docker images --digests | grep reddit-image-bot.*release | sed 's/.*sha256:\(.*\).*/\1/' | sed 's/\s.*//')

if [ $local_digest == $remote_digest ]
then
    exit 0
fi

container_id=$(docker ps --no-trunc | grep 'reddit-image-bot' | tr -s ' ' | cut -d ' ' -f 1)
docker stop $container_id
docker rm $container_id

docker image rm $local_digest

docker pull ghcr.io/valentincpopa/reddit-image-bot:release-1.0.0
docker run -e DOTNET_ENVIRONMENT=Development --network=reddit-image-bot_default -it -v ./appsettings.json:/app/appsettings.json ghcr.io/valentincpopa/reddit-image-bot:release-1.0.0
